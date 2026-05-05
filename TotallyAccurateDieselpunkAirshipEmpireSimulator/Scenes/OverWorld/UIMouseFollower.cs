using Godot;
using System;
using System.Collections.Generic;

public partial class UIMouseFollower : Area2D
{
	public static UIMouseFollower Instance {private set; get;}
	public List<Area2D> CollidingAreas = new();
		
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		Instance = this;
		AreaEntered += (Area2D area) => CollidingAreas.Add(area);
		AreaExited += (Area2D area) => CollidingAreas.Remove(area);
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		
	}
}
