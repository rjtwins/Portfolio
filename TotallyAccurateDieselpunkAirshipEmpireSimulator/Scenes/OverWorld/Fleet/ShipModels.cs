using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public partial class ShipModels : Node2D
{
	private bool isTransitioning = false;
	private Tween currentTween = null;
	
	[Export] PackedScene ShipModelScene {get; set;}
	
	private List<MapShipModel> mapShipModels = new();
	
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{		
		var lightLevel = globals.CalculateLightingLevel();
		lightLevel = Math.Clamp(lightLevel, 0.5f, 1f);
		Modulate = new Color(lightLevel, lightLevel, lightLevel, SelfModulate.A);
		
		var camera = GetViewport()?.GetCamera2D();
		if (camera == null)
			return;
			
		var zoom = camera.Zoom.X;
		
		if (zoom < 0.8f && !isTransitioning)
		{
			isTransitioning = true;
			TweenAlpha(0, 0.2f, () => Visible = false);  // Fade out to alpha 0 over 1 second
		}

		if (zoom > 0.8f && !isTransitioning)
		{
			Visible = true;
			isTransitioning = true;
			TweenAlpha(1, 0.2f);  // Fade in to alpha 1 over 1 second
		}
	}
	
	public void AddShipModel()
	{
		var model = ShipModelScene.Instantiate<MapShipModel>();
		mapShipModels.Add(model);
		AddChild(model);
		
		ArrangeShipModels();
	}
	
	public void RemoveShipModel()
	{
		if(mapShipModels.Count == 0)
			return;
			
		var model = mapShipModels[0];
		RemoveChild(model);
		mapShipModels.Remove(model);
		model.QueueFree();
		
		ArrangeShipModels();
	}
	
	public void ArrangeShipModels()
	{

		var points = globals.GenerateVFormation2D(mapShipModels.Count, 5, 10, 10, 10);

		for (int i = 0; i < points.Count; i++)
		{
			mapShipModels[i].Position = points[i];
		}
		
		//OLD CODE:
		// // var max = mapShipModels.Count * 10f;
		// // var step = 10f;
		// // var ystep = 10f;
		// // var ysign = 1f;
		// // var offset = max * -.5f;
		// // var yoffset = max * -.5f;
		
		// // mapShipModels.Select((x, i) => (x , i)).ToList().ForEach(x => 
		// // {
		// // 	x.x.Position = new Vector2(yoffset, offset);
		// // 	offset += step;
			
		// // 	ysign = x.i >= (mapShipModels.Count / 2) ? -1 : 1;
		// // 	yoffset += ystep * ysign;
		// // });


		// var total = mapShipModels.Count;
		// var spacePerModel = 10f;
		// var modelsPerRow = Mathf.Min(4, total);
		// var rows = Mathf.Ceil(total / modelsPerRow);
		
		// var totalX = modelsPerRow * spacePerModel;
		// var totalY = rows * spacePerModel;

		// var xStart = totalX / 2 * -1;
		// var yStart = totalY / 2 * -1;

		// var maxOffset = modelsPerRow * -10f;
		// var modelIndex = 0;
		// GD.Print($"----------------------------");

		
		// for (int row = 1; row <= rows; row++)
        // {
		// 	var y = yStart + ((row -1) * spacePerModel);
        	
        //     for (int col = 1; col <= modelsPerRow; col++)
		// 	{
		// 		var x = xStart + ((col -1) * spacePerModel);

		// 		var sign = col > (modelsPerRow / 2) ? -1 : 0;
		// 		var offsetY = y;// + col * 10f + maxOffset * (col / modelsPerRow);
				
		// 		var model = mapShipModels[modelIndex];

		// 		model.Position = new Vector2(x, offsetY);
		// 		modelIndex += 1;
				
		// 		GD.Print($"nr:{modelIndex} sign: {sign} - {x}, {offsetY}");
		// 	}
        // }
	}
	
	private void TweenAlpha(float targetAlpha, float duration, Action onFinished = null)
	{
		// If there's an ongoing tween, stop it
		currentTween?.Kill();
		
		// Create a new tween
		currentTween = CreateTween();
		
		// Set up the tween
		currentTween.TweenProperty(this, "self_modulate:a", targetAlpha, duration);
		
		// Reset the transitioning flag when the tween finishes
		currentTween.Finished += () => 
		{
		    isTransitioning = false;
		    onFinished?.Invoke();
		};
	}

	internal void Reset()
	{
		mapShipModels.ForEach(x => x.QueueFree());
		mapShipModels.Clear();
	}

}
