using System;
using Godot;

public partial class GameTimer : Node2D
{
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		globals.GameTime += (float)delta * 60;
		
		//GD.Print($"{globals.HourOfDay}:{globals.MinOfHour}:{MathF.Round(globals.SecOfMin, 2)}");
	}
}
