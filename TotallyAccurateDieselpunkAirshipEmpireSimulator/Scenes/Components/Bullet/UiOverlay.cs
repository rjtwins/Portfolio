using Godot;
using System;

public partial class UiOverlay : Control
{
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
    {
        
    }

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
    {
		Camera3D camera = GameWorldTest.InMapMode ? OrthogonalCamera3d.Instance : OrbitalCamera.Instance;
		Visible = !camera.IsPositionBehind(GetParent<Node3D>().GlobalPosition);

		if (!Visible)
			return;
			
		GlobalPosition = camera.UnprojectPosition(GetParent<Node3D>().GlobalPosition);
    }
}
