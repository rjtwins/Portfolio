using Godot;
using System;

public partial class CloudParalaxLayer : ParallaxLayer
{
	[Export]
	public float WindSpeed {get; set;} = 10f;
	
	[Export]
	public float WindDirection {get; set;} = 0f;
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		MotionOffset += (Vector2.Right * WindSpeed * (float)delta).Rotated(WindDirection);
		if(this.Name == "CloudShadowLayer")
		{
			var ratio = globals.CalculateLightingLevel();
			this.Modulate = this.Modulate = new Color(1, 1, 1, ratio);
		}
	}
}
