using Godot;
using System;

public partial class TestMinimap2 : TextureRect
{
	FastNoiseLite noise;
	OrbitalCamera camera3D;
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
    {
		noise = (this.Texture as NoiseTexture2D).Noise as FastNoiseLite;
		camera3D = GetViewport().GetCamera3D() as OrbitalCamera;
    }

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
    {
		var pos = camera3D.OrbitCenter;
		noise.Offset = pos;
    }
}
