using Godot;
using System;

public partial class ShadowedSprite : Node2D
{
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		GlobalRotation = 0f;
		// var fleet = (Owner.GetParent().Owner as Fleet);
		// Rotation = fleet.Rotation;
		//GlobalRotation = GetParent().GetNode<Sprite2D>("Sprite2D").GlobalRotation;
	}
}
