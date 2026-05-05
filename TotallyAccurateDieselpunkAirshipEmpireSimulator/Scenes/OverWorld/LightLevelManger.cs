using Godot;
using System;

public partial class LightLevelManger : Node2D
{
	[Export]
	Timer UpdateTimer {get; set;}
	[Export]
	CanvasItem DesertBackground {get; set;}
	[Export]
	CanvasItem Clouds {get; set;}
	[Export]
	CanvasItem GlobalBackground {get; set;}	
	
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		UpdateTimer.Timeout += Update;
		GetViewport().GuiSnapControlsToPixels = false;
	}

	private void Update()
	{
		var lightLevel = globals.CalculateLightingLevel();
		var cloud_lightLevel = MathF.Max(0.2f, lightLevel);
		var desert_lightLevel = Mathf.Clamp(lightLevel, 0.1f, 1);
				
		(DesertBackground.Material as ShaderMaterial).SetShaderParameter("light_intensity", desert_lightLevel);
		Clouds.Modulate = new Color(cloud_lightLevel, cloud_lightLevel, cloud_lightLevel, 1);
		GlobalBackground.Modulate = new Color(desert_lightLevel, desert_lightLevel, desert_lightLevel, 1);
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		
	}
}
