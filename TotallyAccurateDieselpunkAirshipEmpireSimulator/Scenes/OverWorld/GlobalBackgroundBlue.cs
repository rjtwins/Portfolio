using Godot;
using System;

public partial class GlobalBackgroundBlue : ColorRect
{
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		var lightLevel = globals.CalculateLightingLevel();
		lightLevel = Math.Clamp(lightLevel, 0.3f, 1f);
		Modulate = new Color(lightLevel, lightLevel, lightLevel, 1);
	}
}
