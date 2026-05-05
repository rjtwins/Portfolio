using Godot;
using System;

public partial class ASprite : Sprite2D
{
	[Export] Vector2 init_scale;
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		init_scale = Scale;
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		var camera = GetViewport().GetCamera2D();
		Scale = init_scale * (Vector2.One / camera.Zoom);
	}
}
