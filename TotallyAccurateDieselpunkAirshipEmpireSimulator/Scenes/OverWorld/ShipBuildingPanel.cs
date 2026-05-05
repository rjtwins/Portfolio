using Code;
using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public partial class ShipBuildingPanel : Panel
{
	[Export] ItemList BlueprintList { get; set; }
	private List<ShipBlueprint> _blueprints { get; set; }
	
	[Export] ItemList BuildingQueue { get; set; }
	[Export] ItemList ShipInfo { get; set; }
	
	[Export] Button AddButton { get; set; }
	[Export] Button RemoveButton { get; set; }
	
	[Export] Button ToggleButton { get; set; }
	
	[Export] Button CloseButton { get; set; }
	
	[Export] UIArea2d UIArea2D { get; set; }
	
	public Settlement Settlement { get; set; }
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		BlueprintList.ItemSelected += BlueprintListItemSelected;
		BuildingQueue.ItemSelected += BuildingQueueItemSelected;
		AddButton.Pressed += AddButtonPressed;
		RemoveButton.Pressed += RemoveButtonPressed;

		ToggleButton.Pressed += ToggleButtonPressed;
		CloseButton.Pressed += ToggleButtonPressed;

		this.VisibilityChanged += OnVisibilityChanged;
	}

    private void OnVisibilityChanged()
    {
		UIArea2D.Monitoring = this.Visible;
		UIArea2D.Monitorable = this.Visible;
    
		if (this.Visible)
			Update();
    }


    private void ToggleButtonPressed()
    {
		this.Visible = !this.Visible;
    }


    private void RemoveButtonPressed()
    {
		if (!BuildingQueue.IsAnythingSelected())
			return;
			
		var index = BuildingQueue.GetSelectedItems().First();
		Settlement.Data.ShipBuildQueue.RemoveAt(index);

		Update();

		if (BuildingQueue.ItemCount == 0)
			return;

		BuildingQueue.Select(BuildingQueue.ItemCount - 1);	
    }


    private void AddButtonPressed()
    {
		if (!BlueprintList.IsAnythingSelected())
			return;

		var index = BlueprintList.GetSelectedItems().First();
		var selected = _blueprints[index];
		
		if(!Settlement.TryAddShipToQueue(selected))
		{
			return;
			//TODO: Inform user why.
		}

		Update();

		//Keep same item selected for ease of use.
		BlueprintList.Select(index);

		//BuildingQueue.AddItem(selected.Name);
    }


    private void BuildingQueueItemSelected(long index)
    {
        //throw new NotImplementedException();
    }


    private void BlueprintListItemSelected(long index)
    {
		ShipInfo.Clear();
		var bp = _blueprints[(int)index];
		ShipInfo.AddItem(bp.Name);
		//TODO: Add more info, like value and stuff.	
    }

    public void Update()
	{
		BlueprintList.Clear();
		
		var shipFiles = globals.GetFilesInFolder("user://ships");
		var blueprints = shipFiles.Select(x =>
		{
			var name = x.Split(@"/").Last().Split(".").First();
			var blueprint = globals.LoadShipFromFile(name);

			return blueprint;
		}).ToList();

		_blueprints = blueprints;

		_blueprints.ForEach(x =>
		{
			this.BlueprintList.AddItem(x.Name);
		});

		BuildingQueue.Clear();
		Settlement.Data.ShipBuildQueue.ToList().ForEach(x =>
		{
			BuildingQueue.AddItem(x.ShipBlueprint.Name);
		});
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
    {
		if (Settlement == null || Visible == false)
			return;
		
		int? index = null;
		if (BuildingQueue.IsAnythingSelected())
			index = BuildingQueue.GetSelectedItems().First();

		BuildingQueue.Clear();
		
		//Update building queue progress:
		for (int i = 0; i < Settlement.Data.ShipBuildQueue.Count; i++)
		{
			var item = Settlement.Data.ShipBuildQueue[i];
			BuildingQueue.AddItem($"{item.ShipBlueprint.Name} - {item.Current}/{item.Required}");
		}

		if (index != null && index.Value < BuildingQueue.ItemCount)
			BuildingQueue.Select(index.Value);
    }
}
