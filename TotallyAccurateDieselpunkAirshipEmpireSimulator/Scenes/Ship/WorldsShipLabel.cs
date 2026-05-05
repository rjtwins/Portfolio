using Godot;
using System;

public partial class WorldsShipLabel : RichTextLabel
{
	[Export] WorldShip worldShip;
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
    {
		Text = $"Altitude: {worldShip.GlobalPosition.Round().Y}\nVelocity: {worldShip.LinearVelocity.Round().Length()}";
    }
}
