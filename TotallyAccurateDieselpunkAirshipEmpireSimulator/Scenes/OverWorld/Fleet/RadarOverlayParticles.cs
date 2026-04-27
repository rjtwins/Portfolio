using System;
using Godot;

public partial class RadarOverlayParticles : GpuParticles2D
{
	private float _init_scale_min;
	private float _init_scale_max;
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		_init_scale_min = (float)ProcessMaterial.Get("scale_min");
		_init_scale_max = (float)ProcessMaterial.Get("scale_max");
		OverworldSpeedControl.GameSpeedChanged += OnGameSpeedChanged;
		
		
		TreeExited += () =>
		{
		    OverworldSpeedControl.GameSpeedChanged -= OnGameSpeedChanged;
		};
	}

    private void OnGameSpeedChanged(double newValue)
    {
        SpeedScale = newValue * 0.99;
        Restart();
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta)
	{			
		Visible = (GetParent().GetParent() as Node2D).Visible;
				
		if(!Visible)
			return;
		
		var _camera = GetViewport().GetCamera2D();
		
		if (_camera == null)
			return;
		
		//Scale
		ProcessMaterial.Set("scale_min", _init_scale_min * (1 / _camera.Zoom.X));
		ProcessMaterial.Set("scale_max", _init_scale_max * (1 / _camera.Zoom.X));
	}
}
