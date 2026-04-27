using Godot;
using System;

public partial class ScreenCollisionShape2D : CollisionShape2D
{
	private float _init_radius;
	private Camera2D _camera;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		_init_radius = (Shape as CircleShape2D).Radius;
		_camera = GetViewport().GetCamera2D();
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		_camera = GetViewport().GetCamera2D();
		if (_camera == null)
			return;
			
		(Shape as CircleShape2D).Radius = _init_radius * (1 / _camera.Zoom.X);
	}
}
