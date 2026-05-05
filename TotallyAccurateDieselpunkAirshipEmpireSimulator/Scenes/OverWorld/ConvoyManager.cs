using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public partial class ConvoyManager : Node2D
{
	[Export]
	public Timer UpdateTimer {get; set;}
	[Export]
	public PackedScene ConvoyScene {get; set;}
	
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		UpdateTimer.Timeout += Update;
	}

	private void Update()
	{
		// var settlements = new Godot.Collections.Array<Settlement>((Owner as OverWorld).Settlements);
		// var manifest = new Dictionary<string, int>() 
		// {
		// 	{"Meta", 100},
		// 	{"Oil", 250}
		// };		
		// GenerateConvoy(settlements.PickRandom(), settlements.PickRandom(), manifest);
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		
	}
	
	public void GenerateConvoy(Settlement from, Settlement too, Dictionary<string, int> manifest)
	{
		//TODO: Determine ship type makeup.
		Convoy convoy = ConvoyScene.Instantiate() as Convoy;
		AddChild(convoy);
		convoy.Manifest = manifest;
		convoy.Fleet.FleetInfo.Name = "Convoy";
		convoy.From = from;
		convoy.Too = too;
		convoy.GlobalPosition = from.GlobalPosition;
		convoy.Fleet.MoveToPoint(too.GlobalPosition);
		
		convoy.ArrivedAtSettlement += ConvoyArrivedAtSettlement;
		
		GD.Print($"Sending convoy from {from}, too {too}");
	}

	private void ConvoyArrivedAtSettlement(Convoy convoy, Settlement too)
	{
		var manifest = convoy.Manifest;
		var data = too.Data;
		var type = data.GetType();
		manifest.ToList().ForEach(x => 
		{
			var updated = (float)type.GetProperty(x.Key).GetValue(data) + x.Value;
			type.GetProperty(x.Key).SetValue(data, updated);
		});
	}
}
