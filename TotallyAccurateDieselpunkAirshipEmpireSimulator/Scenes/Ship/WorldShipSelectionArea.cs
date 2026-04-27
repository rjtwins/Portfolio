using Godot;
using System;

public partial class WorldShipSelectionArea : Area2D
{
	[Export] public WorldShip WorldShip;
	[Export] CollisionShape2D Shape;
	CircleShape2D circleShape2D;
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
    {
		circleShape2D = (Shape.Shape as CircleShape2D);
    }

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
    {
		Camera3D camera = GameWorldTest.InMapMode ? OrthogonalCamera3d.Instance : OrbitalCamera.Instance;
		Visible = !camera.IsPositionBehind(WorldShip.GlobalPosition);

		if (GameWorldTest.InMapMode)
			circleShape2D.Radius = 5;
		else
			circleShape2D.Radius = 20;

		if (!Visible)
			return;
			
		GlobalPosition = camera.UnprojectPosition(WorldShip.GlobalPosition);
    }
}
