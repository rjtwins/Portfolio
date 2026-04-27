using System;
using Godot;

public partial class Minimap : TextureRect
{
	[Export] public FastNoiseLite Noise { get; set; }
	[Export] public OrbitalCamera Camera3D { get; set; }
	[Export] public float Terrain_Max_Height { get; set; }

	private Vector2 _oldRefPos { get; set; } = Vector2.One * int.MaxValue;
	
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
    {
		Camera3D = GetViewport().GetCamera3D() as OrbitalCamera;

		UpdateMap();
    }

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
    {
		// var worldSize = globals.GetWorldWidth3D(Camera3D, Camera3D.GlobalPosition.Y);
		
		// GD.Print(worldSize);
		
		Visible = Camera3D.OrbitCenter.Y + Camera3D.OrbitDistance > 1000F;
		
		if (!Visible)
			return;
			
		UpdateMap();
    }
    
    private void UpdateMap()
    {
		var cameraPos = Camera3D.OrbitCenter;
		
		Camera3D.Pitch = 1.553f;
		Camera3D.Yaw = 0f;
		Camera3D.UpdateCameraPosition();
		
		if (_oldRefPos == new Vector2(cameraPos.X, cameraPos.Z))
			return;
		_oldRefPos = new Vector2(cameraPos.X, cameraPos.Z);
		
		var img = Image.CreateEmpty((int)Size.X, (int)Size.Y, false, Image.Format.Rgb8);
		var xRes = Size.X;
		var yRes = Size.Y;

		var ratio = yRes / xRes;
		
		var worldSize = globals.GetWorldWidth3D(Camera3D, Camera3D.OrbitCenter.Y + Camera3D.OrbitDistance);
		//GD.Print(worldSize);
		
		float mapWorldWidth  = worldSize; // meters shown horizontally
		float mapWorldHeight = worldSize * ratio; // meters shown vertically

		//GD.Print("Minimap process");

		
		for (int x = 0; x < xRes; x += 2)
		{
			for(int y = 0; y < yRes; y += 2)
			{
				float wx = cameraPos.X - mapWorldWidth*0.5f + (x / xRes) * mapWorldWidth;
				float wz = cameraPos.Z - mapWorldHeight*0.5f + (y / yRes) * mapWorldHeight;
				
				float n = Noise.GetNoise2D(wx, wz);
				
				if (n < 0) n *= 0.3f;
				
				n *= Terrain_Max_Height;
				
				float g = Mathf.InverseLerp(0, Terrain_Max_Height, n);
				Color c = new Color(g, g, g);
				img.SetPixel(x, y, c);
			}
		}
		
		//img.GenerateMipmaps();
		ImageTexture tex = ImageTexture.CreateFromImage(img);
		Texture = tex;
    }
}
