using Godot;
using System;

public partial class ScreenLine2D : Line2D
{
	[Export]
	public int ScreenWidth = 2;
	[Export]
	public bool MaintainPixelWidth = true;
	[Export]
	public bool MaintainSize = false;
	
	private Vector2 _initScale;
	private Camera2D _camera;
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		_initScale = Scale;
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		UpdateLineWidth();
	}
	
	private void UpdateLineWidth()
	{
		_camera = GetViewport().GetCamera2D();

		if (_camera != null)
		{
			if(MaintainSize)
			{
				Scale = _initScale * (Vector2.One / _camera.Zoom);
			}else
			{
				Width = ((float)ScreenWidth) / _camera.Zoom.X;
			}
		}
	}
}
