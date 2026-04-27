using Godot;
using System;
using System.Diagnostics;
using System.Linq;

public partial class WorldShipUI : Control
{
	[Export] WorldShip worldShip;
	[Export] CanvasItem MapModeIndicator;
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
    {
		Camera3D camera = GameWorldTest.InMapMode ? OrthogonalCamera3d.Instance : OrbitalCamera.Instance;
		Visible = worldShip.UISelected && !GameWorldTest.InMapMode && !camera.IsPositionBehind(worldShip.GlobalPosition) && worldShip.UISelected;
		
		if (!Visible)
			return;
			
		GlobalPosition = camera.UnprojectPosition(worldShip.GlobalPosition);
    }
}
