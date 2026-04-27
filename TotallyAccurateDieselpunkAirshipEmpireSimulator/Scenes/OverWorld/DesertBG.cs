using Godot;
using System;

public partial class DesertBG : TextureRect
{
	private bool isTransitioning = false;
	private Tween currentTween = null;
	
	private Color _initModulate;
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		_initModulate = SelfModulate;
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		var camera = GetViewport()?.GetCamera2D();
		if (camera == null)
			return;
			
		var zoom = camera.Zoom.X;
		
		if (zoom < 0.8f && !isTransitioning)
		{
			isTransitioning = true;
			TweenAlpha(0, 0.2f);  // Fade out to alpha 0 over 1 second
		}

		if (zoom > 0.8f && !isTransitioning)
		{
			isTransitioning = true;
			TweenAlpha(1, 0.2f);  // Fade in to alpha 1 over 1 second
		}
	}
	
	private void TweenAlpha(float targetAlpha, float duration)
	{
		// If there's an ongoing tween, stop it
		currentTween?.Kill();
		
		// Create a new tween
		currentTween = CreateTween();
		
		// Set up the tween
		currentTween.TweenProperty(this, "self_modulate:a", targetAlpha, duration);
		currentTween.SetIgnoreTimeScale(true);
		
		// Reset the transitioning flag when the tween finishes
		currentTween.Finished += () => isTransitioning = false;
	}
}
