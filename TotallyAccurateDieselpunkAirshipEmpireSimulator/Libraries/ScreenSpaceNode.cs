using Godot;
using System;

public partial class ScreenSpaceNode : Node2D
{
	private Vector2 _initScale;
	private Camera2D _camera;
	
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		_initScale = Scale;
		_camera = GetViewport().GetCamera2D();
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		_camera = GetViewport()?.GetCamera2D();
		if (_camera == null)
			return;
			
		Scale = _initScale * (Vector2.One / _camera.Zoom);
	}
}
