using Godot;
using System;

public partial class RadarPing : Sprite2D
{
	private Vector2 _init_scale;
	[Export] float lifeTime = 100f;
	
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
    {
        _init_scale = Scale;
        var _camera = GetViewport().GetCamera2D();
        Scale = _init_scale * (1 / _camera.Zoom.X);
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta)
    {
        var _camera = GetViewport().GetCamera2D();
        
		if (_camera == null)
			return;
			
		this.Scale = _init_scale * (1 / _camera.Zoom.X);
		
		lifeTime -= (float)delta;
		
		if(lifeTime <= 0)
			QueueFree();
    }
}
