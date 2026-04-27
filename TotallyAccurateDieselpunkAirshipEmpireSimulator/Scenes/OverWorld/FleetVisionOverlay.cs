using Godot;
using System;

public partial class FleetVisionOverlay : ColorRect
{
	private bool isTransitioning = false;
	private Tween currentTween = null;
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		var camera = GetViewport().GetCamera2D();
		var zoom = camera.Zoom.X;
		
		if (zoom < 0.7f && !isTransitioning)
		{
			isTransitioning = true;
			TweenAlpha(0, 0.1f);  // Fade out to alpha 0 over 1 second
		}

		if (zoom > 0.7f && !isTransitioning)
		{
			isTransitioning = true;
			TweenAlpha(1, 0.5f);  // Fade in to alpha 1 over 1 second
		}
		
		var lightLevel = globals.CalculateLightingLevel();
		var desert_lightLevel = Mathf.Clamp(lightLevel, 0.1f, 1);
				
		Color = new Color(desert_lightLevel, desert_lightLevel, desert_lightLevel, Color.A);
	}
	
	private void TweenAlpha(float targetAlpha, float duration)
	{
		// If there's an ongoing tween, stop it
		currentTween?.Kill();
		
		// Create a new tween
		currentTween = CreateTween();
		
		// Set up the tween
		currentTween.TweenProperty(this, "color:a", targetAlpha, duration);
		
		// Reset the transitioning flag when the tween finishes
		currentTween.Finished += () => isTransitioning = false;
	}
}
