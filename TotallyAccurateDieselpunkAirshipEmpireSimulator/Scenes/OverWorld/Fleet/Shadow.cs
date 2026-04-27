using Godot;
using System;

public partial class Shadow : Sprite2D
{
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		GlobalRotation = 0f;
		var ratio = globals.CalculateLightingLevel();
		this.Modulate = new Color(this.Modulate.R, Modulate.G, this.Modulate.B, ratio);
	}
}
