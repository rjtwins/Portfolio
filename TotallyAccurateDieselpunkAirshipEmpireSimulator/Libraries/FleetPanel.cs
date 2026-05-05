using Godot;
using System;
using System.Linq;

public partial class FleetPanel : TextureRect
{
	[Export] public HFlowContainer ItemContainer {get; set;}
	[Export] public VSlider Slider {get; set;}
	[Export] public Button ToggleMode {get; set;}
	[Export] public Button Detach {get; set;}
	[Export] public ItemList CurrentlySelectedStrikePanel { get; set; }
	
	[Export] public MissilePanel MissilePanel { get; set; }
	
	public FleetPanelMode FleetPanelMode {get; set;} = FleetPanelMode.ShipList;
	
	[Export] public Fleet SelectedFleet {get; set;}
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		ToggleMode.Pressed += ModeToggled;
		Detach.Pressed += DetachPressed;
	}

	private void DetachPressed()
	{
		switch (FleetPanelMode)
		{
			case FleetPanelMode.ShipList:
				DetachShips();
				break;
				
			case FleetPanelMode.Strike:
				SelectedFleet.StrikeManager.StrikeReadyToLaunch = true;
				break;
			
			case FleetPanelMode.Missile:
				break;
				
			default:
				break;
		}
	}

	private void DetachShips()
	{
		var ships = ItemContainer
			.GetChildren()
			.OfType<TextureButton>()
			.Where(x => x.ButtonPressed)
			.Select(x => SelectedFleet.FleetInfo.Ships[int.Parse(x.Name)])
			.ToList();

		SelectedFleet.DetachShips(ships);
		
		if(!SelectedFleet.IsSettlementFleet)
			SelectedFleet.SetupShipModels();
		
		SetupPanel();
	}

	private void ModeToggled()
	{
		int mode = ((int)FleetPanelMode + 1) % Enum.GetNames<FleetPanelMode>().Count();
		FleetPanelMode = (FleetPanelMode)mode;
		
		SetupPanel();
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		
	}
	
	private void SetupPanel()
	{
		if(SelectedFleet == null)
			return;
		
		ClearPanel();
		
		switch (FleetPanelMode)
		{
			case FleetPanelMode.ShipList:
				SetupShipPanel();
				break;
			
			case FleetPanelMode.Strike:
				SetupStrikePanel();
				break;
			
			case FleetPanelMode.Missile:
				MissilePanel.Show();
				MissilePanel.Setup(SelectedFleet.FleetInfo);
				break;
				
			default:
				break;
		}
	}
	
	private void SetupShipPanel()
	{
		Detach.Text = "Detach";
		var ships = SelectedFleet.FleetInfo.Ships;
		var icons = ships.Select((x, i) => 
		{
			var control = new TextureButton()
			{
				TextureNormal = GD.Load<Texture2D>("res://icon.svg"),
				TexturePressed = GD.Load<Texture2D>("res://icon.png"),
				ToggleMode = true,
				Name = i.ToString()
			};
			
			return control;
		}).ToList();
		
		icons.ForEach(x => ItemContainer.AddChild(x));
	}
	
	private void SetupStrikePanel()
	{
		ItemContainer.GetChildren().ToList().ForEach(x => x.QueueFree());
		
		Detach.Text = "Launch";
		var airGroupStores = SelectedFleet.GetAirGroup();
		var typeCount = airGroupStores.GroupBy(x => x.StrikeCraftData.AircraftType).ToDictionary(x => x.Key, x=> x.Count());
		var types = airGroupStores.Select(x => x.StrikeCraftData.AircraftType).Distinct();
		var icons = types.Select(x => 
		{
			var control = new TextureButton()
			{
				//TODO: make dynamic based on passed aircraft
				TextureNormal = GD.Load<Texture2D>("res://icon.svg"),
				ToggleMode = false,
				TooltipText = typeCount[x].ToString()
			};
			control.Pressed += () => airGroupButtonPressed(x);
			return control;
		}).ToList();
		
		icons.ForEach(x => ItemContainer.AddChild(x));
	}

	private void airGroupButtonPressed(AircraftType x)
	{
		SelectedFleet.StrikeManager.TryAddToStrike(x);
		SetupStrikePanel();
		UpdateCurrentlySelectedStrikePanel();
	}

    private void UpdateCurrentlySelectedStrikePanel()
    {
		CurrentlySelectedStrikePanel.Clear();

		if (SelectedFleet == null)
			return;
		
		SelectedFleet.StrikeManager.CurrentStrike.ForEach(x =>
		{
			CurrentlySelectedStrikePanel.AddItem(x.StrikeCraftData.AircraftType.ToString());
		});
    }

    private void SetupMissilePanel()
    {
		// var missileStores = SelectedFleet.GetMissileStores();
		// var types = missileStores.Select(x => x.MissileIdentifier).Distinct();
		
		// foreach(var type in types)
		// {
		// 	var nr = missileStores.Count(x => x.MissileIdentifier == type);
		// 	var control = new TextureButton()
		// 	{
		// 		//TODO: make dynamic based on passed missile
		// 		TextureNormal = GD.Load<Texture2D>("res://icon.svg"),
		// 		ToggleMode = false,
		// 		TooltipText = nr.ToString()
		// 	};
		// 	control.Pressed += () => missileGroupButtonPressed(type);
		// }
    }

    private void ClearPanel()
	{
		if(SelectedFleet != null)
		{
			SelectedFleet.StrikeManager.StrikeReadyToLaunch = false;
			SelectedFleet.StrikeManager.CurrentStrike.Clear();
		}
		
		ItemContainer.GetChildren().ToList().ForEach(x => ItemContainer.RemoveChild(x));
		UpdateCurrentlySelectedStrikePanel();

		MissilePanel.Hide();
	}

	public void FleetSelected(Fleet fleet)
	{
		SelectedFleet = fleet;
		SetupPanel();
	}
	
	public void FleetUnselected()
	{
		SelectedFleet = null;
		ClearPanel();
	}
}

public enum FleetPanelMode
{
	ShipList,
	Strike,
	Missile
}
