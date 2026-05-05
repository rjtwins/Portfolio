using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public partial class SidePanel : Panel
{
	public FleetManager FleetManager => FleetManager.Instance;
	[Export] public ItemList FleetList { get; set; }
	[Export] public VBoxContainer SettlementList { get; set; }
	private List<Settlement> currentSettlements {get; set;}
	[Export] public Timer UpdateTimer { get; set; }

	private bool _setup = false;
	
	
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
    {
		FleetList.ItemClicked += FleetListItemClicked;
		// FleetManager.ChildEnteredTree += FleetManagerFleetAdded;
		// FleetManager.ChildExitingTree += FleetManagerFleetRemoved;
		UpdateTimer.Timeout += UpdateTimerTimeout;
		
		var timer = GetTree().CreateTimer(3, true, false, true);
		timer.Timeout += () => 
		{		    
			var settlements = OverWorld.Instance.GetChildren().OfType<Settlement>().ToList();
			settlements.ForEach(x => 
			{
				var item = RightPanelSettlementInfo.CreateNew(x);
				SettlementList.AddChild(item);
			});
		};
    }

    private void UpdateList()
    {
    
		var fleets = FleetManager.GetChildren().OfType<Fleet>().Where(x => x.FleetInfo.Faction == Faction.PLAYER).ToList();
		FleetList.Clear();
		
		fleets.ForEach(x => 
		{
			//TODO ADD MORE INFO:
			FleetList.AddItem($"{x.FleetInfo.Name}", selectable: false);
		});
    }

    private void UpdateTimerTimeout()
    {
    	UpdateList();
    }

    private void FleetListItemClicked(long index, Vector2 atPosition, long mouseButtonIndex)
    {
		var camera = GetViewport()?.GetCamera2D();
		if (camera == null)
			return;
		
		var fleets = FleetManager.GetChildren().OfType<Fleet>().Where(x => x.FleetInfo.Faction == Faction.PLAYER).ToList();
		var fleet = fleets[(int)index];
		fleet.Selectable.Select();

		camera.GlobalPosition = fleet.GlobalPosition;
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta)
    {
        
    }
}
