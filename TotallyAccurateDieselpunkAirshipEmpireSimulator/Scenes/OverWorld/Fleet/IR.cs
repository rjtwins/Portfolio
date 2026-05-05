using Godot;
using System;
using System.Collections.Generic;



public partial class IR : Node2D
{
	
	[Signal]
	public delegate void OnReturnFromPointEventHandler(Vector2 point, float strength);
	
	[Export]
	public Area2D IRDetectionArea {get; set;}
	
	[Export]
	public Area2D OwnIrArea {get; set;}
	
	[Export]
	public bool Enabled {get; set;}
	
	private List<Area2D> _detectedAreas = new();
	
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		IRDetectionArea.AreaEntered += IRAreaEntered;
		IRDetectionArea.AreaExited += IRAreaExited;
	}

	private void IRAreaEntered(Area2D area)
	{
		if(area == OwnIrArea)
			return;
			
		_detectedAreas.Add(area);
	}
	
	private void IRAreaExited(Area2D area)
	{
		if(area == OwnIrArea)
			return;
			
		_detectedAreas.Remove(area);
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		if(!Enabled)
			return;
			
		_detectedAreas.ForEach(x => RefineSignal(x));
	}


	private void RefineSignal(Area2D x)
	{
		//Figure out strength.
		var strength = 100f;
		//Filter out unwanted areas.
		
		//Notify
		EmitSignal("OnReturnFromPoint", x.GlobalPosition, strength);
	}

}
