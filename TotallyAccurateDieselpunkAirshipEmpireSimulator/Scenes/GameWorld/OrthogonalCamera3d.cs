using Godot;
using System;
using System.Diagnostics.CodeAnalysis;

public partial class OrthogonalCamera3d : Camera3D
{
	[Export] OrbitalCamera OrbitalCamera;
	public static OrthogonalCamera3d Instance;
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
    {
		Instance = this;
    }

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
    {
		GlobalPosition = new Vector3(OrbitalCamera.GlobalPosition.X, 3500, OrbitalCamera.GlobalPosition.Z);
		Size = globals.GetWorldWidth3D(OrbitalCamera, OrbitalCamera.OrbitDistance + OrbitalCamera.OrbitCenter.Y);
		GlobalRotation = OrbitalCamera.GlobalRotation;
    }
}
