using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

public partial class SettlementDetails : TextureRect
{
	[Export] public Label NameLabel {get;set;}
	[Export] public Label FundsP {get; set;}
	[Export] public Label ManpowerP {get; set;}
	[Export] public Label MetalP {get; set;}
	[Export] public Label VolatileP {get; set;}
	[Export] public Label MunitionsP {get; set;}
	
	[Export] public Label SelectedBuildingName {get; set;}
	[Export] public Label SelectedBuildingLevel {get; set;}
	
	public List<Button> BuildingButtons {get; set;}
	[Export] public Button MetalExtractorButton {get; set;}
	[Export] public Button FuelRefineryButton {get; set;}
	[Export] public Button MunitionsFactoryButton {get; set;}
	[Export] public Button AcademyButton {get; set;}
	[Export] public Button BuildButton {get; set;}
	[Export] public Button StopBuildingButton {get; set;}
	[Export] public Button ManageFleetButton {get; set;}
	
	[Export] public ProgressBar MetalExtractorProgress {get; set;}
	[Export] public ProgressBar MunitionsFactoryProgress {get; set;}
	[Export] public ProgressBar AcademyProgress {get; set;}
	[Export] public ProgressBar RefineryProgress {get; set;}
	
	[Export] public ItemList BuildingBuildQueue {get; set; }
	
	[Export] public FleetPanel FleetPanel {get; set;}
	[Export] public ShipBuildingPanel ShipBuildingPanel { get; set; }
	private Settlement _settlement = null;
	private string _selectedBuilding = string.Empty;
	private int lastHash = 0;
	public bool FleetPanelStatus = false;
	
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		BuildingButtons = new ()
		{
			MetalExtractorButton,
			FuelRefineryButton,
			MunitionsFactoryButton,
			AcademyButton,
		};
		
		BuildingButtons.ForEach(x =>
			{
				var name = x.GetMeta("Building", "").As<string>();
				x.Toggled += (bool toggle_on) => BuildingButtonToggled(name, toggle_on);
				x.MouseEntered += () => BuildingMouseEntered(name);
			});
			
		BuildButton.Pressed += BuildButtonPressed;
		StopBuildingButton.Pressed += StopBuildingButtonPressed;
		ManageFleetButton.Pressed += ToggleMangeFleet;
	}

    private void StopBuildingButtonPressed()
    {
        throw new NotImplementedException();
    }

    private void ToggleMangeFleet()
	{
		if(_settlement == null)
			return;
			
		FleetPanelStatus = !FleetPanelStatus;
		if(FleetPanelStatus)
		{
			FleetPanel.FleetSelected(_settlement.SettlementFleet);
			FleetPanel.GetParent<FleetControlOverlay>().MovePanelDown();
			//_settlement.SettlementFleet.Selectable.Select();
			_settlement.Selectable.UnSelect();
			FleetPanelStatus = false;
		}else
		{
			FleetPanel.FleetUnselected();
			FleetPanel.GetParent<FleetControlOverlay>().MovePanelUp();
		}
	}

	private void BuildButtonPressed()
	{
		_settlement.QueueBuilding(_selectedBuilding);
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		if(_settlement == null)
			return;
		
		UpdatePanel(_settlement);
	}

	private void UpdatePanel(Settlement s)
	{
		// var hash = s.data.GetHashCode();
		// if(hash == lastHash)
		// 	return;
		// lastHash = hash;
		
		NameLabel.Text = s.Data.Name;
		
		FundsP.Text = s.Data.FundsProduction.ToString();
		ManpowerP.Text = s.Data.ManpowerProduction.ToString();
		MunitionsP.Text = s.Data.MunitionsProduction.ToString();
		MetalP.Text = s.Data.MetalProduction.ToString();
		VolatileP.Text = s.Data.VolatilesProduction.ToString();

		if(string.IsNullOrEmpty(_selectedBuilding))
		{
			SelectedBuildingLevel.Text = string.Empty;
			SelectedBuildingName.Text = string.Empty;
			return;
		}
		
		SelectedBuildingName.Text = _selectedBuilding;
		SelectedBuildingLevel.Text = ((byte?)_settlement.Data.GetType().GetProperty(_selectedBuilding).GetValue(_settlement.Data)).ToString();
		
		UpdateBuildingBuildQueue();
	}
		
	private void UpdateBuildingBuildQueue()
    {
		MetalExtractorProgress.Hide();
		MunitionsFactoryProgress.Hide();
		AcademyProgress.Hide();
		RefineryProgress.Hide();
		BuildingBuildQueue.Clear();
			
		if (!_settlement.Data.BuildingBuildQueue.Any())
		{
			return;
		}
		
		_settlement.Data.BuildingBuildQueue.ToList().ForEach(x => 
		{
		   BuildingBuildQueue.AddItem($"{x.Name} {x.Current}/{x.Required}"); 
		});
		
		
		
    }

	public void SettlementSelected(Settlement s)
	{
		_settlement = s;
		ShipBuildingPanel.Settlement = s;
	}
	
	public void SettlementUnSelected()
	{
		_settlement = null;
		ShipBuildingPanel.Settlement = null;
	}
	
	public void BuildingButtonToggled(string buildingName, bool toggle_on)
	{
		if(toggle_on)
			_selectedBuilding = buildingName;
		else
			_selectedBuilding = string.Empty;
	}
	
	private void BuildingMouseEntered(StringName name)
	{
		_selectedBuilding = name.ToString();
	}
}
