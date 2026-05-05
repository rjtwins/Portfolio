using Godot;
using System;

public partial class ShaderMiniMap : TextureRect
{
	private ShaderMaterial minimapMaterial;
	[Export] public FastNoiseLite Noise { get; set; }
	[Export] public OrbitalCamera Camera3D { get; set; }
	[Export] public float Terrain_Max_Height { get; set; }
	
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
    {
        // Get a reference to your ShaderMaterial (applied to the minimap's Control node)
		minimapMaterial = (ShaderMaterial)GetMaterial();
		Camera3D = GetViewport().GetCamera3D() as OrbitalCamera;
		
		// World size displayed on the minimap
		minimapMaterial.SetShaderParameter("world_width", 1920f);
		minimapMaterial.SetShaderParameter("world_height", 1080f);

		// Camera xz position
		var cameraPos = Camera3D.OrbitCenter;	
		minimapMaterial.SetShaderParameter("camera_position", new Vector2(cameraPos.X, cameraPos.Z));

		// Noise parameters
		minimapMaterial.SetShaderParameter("frequency", 0.0003f);
		minimapMaterial.SetShaderParameter("terrain_max_height", 1000f);
    }

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		minimapMaterial = (ShaderMaterial)GetMaterial();

		var cameraPos = Camera3D.OrbitCenter;	
		minimapMaterial.SetShaderParameter("camera_position", new Vector2(cameraPos.X, cameraPos.Z));
	}
}
