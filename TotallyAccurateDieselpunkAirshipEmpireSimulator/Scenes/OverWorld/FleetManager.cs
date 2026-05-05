using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public partial class FleetManager : Node2D
{
	public static FleetManager Instance { get; private set; }
	// [Export] ParallaxBackground parallaxBackground;
	// [Export] PackedScene overlayScene;
	// Dictionary<Fleet, FleetVisionOverlay> overlays = new();
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		Instance = this;
		// this.ChildEnteredTree += FleetManagerFleetAdded;
		// this.ChildExitingTree += FleetManagerFleetRemoved;		
	}

    // private void FleetManagerFleetRemoved(Node node)
    // {
    //     //throw new NotImplementedException();
    // }


    // private void FleetManagerFleetAdded(Node node)
    // {
    //     if(!(node is Fleet fleet))
	// 		return;
			
	// 	fleet.MoveToPoint(new Vector2(1000, 1000));
    // }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta)
	{
		// //Update list
		// var children = GetChildren().OfType<Fleet>();		
		// if(children.Count() < overlays.Count())
		// {
		// 	//remove
		// 	var toRemove = overlays.Keys.ToList().Except(children.ToList()).ToList();
		// 	toRemove.ForEach(x => 
		// 	{
		// 		var overlay = overlays[x];
		// 		overlay.QueueFree();
		// 		overlays.Remove(x);
		// 	});
			
		// }
		// else if(children.Count() > overlays.Count())
		// {
		// 	var toAdd = children.ToList().Except(overlays.Keys.ToList()).ToList();
		// 	toAdd.ForEach(x => 
		// 	{
		// 		var overlay = overlayScene.Instantiate<FleetVisionOverlay>();
		// 		parallaxBackground.AddChild(overlay);
		// 		overlays.Add(x, overlay);
		// 	});
		// }
		
		// //Move stuff around:
		// overlays.ToList().ForEach(x => 
		// {
		// 	x.Value.GlobalPosition = x.Key.GlobalPosition - Vector2.One * 0.5f * x.Value.Size;
		// });
	}
}
