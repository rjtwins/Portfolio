using Godot;
using System;
using System.Threading.Tasks;

public partial class MinimapTileMap : TileMap
{
	public static MinimapTileMap Instance;

	[Export] FastNoiseLite FastNoiseLite;

	Vector2I posTile = new Vector2I(4, 2);
	Vector2I negTiel = new Vector2I(1, 1);

	[Export] OrbitalCamera Camera3D;

	[Export] public uint XRex { get; set; } = 25;

	private Vector2 viewportSize;
	private float ratio;
	private float xRes;
	private float yRes;
	private float coverageFactor = 1.414f;
	
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
    {
		viewportSize = GetViewport().GetVisibleRect().Size;
		ratio = viewportSize.Y / viewportSize.X;
		xRes = XRex * coverageFactor;
		yRes = xRes * coverageFactor;

		Scale = Vector2.One * viewportSize.X / (xRes * 16);

		GlobalPosition = viewportSize / 2;

		Instance = this;
    }

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
    {
		if(!GameWorldTest.InMapMode)
        {
			Visible = false;
			return;
        }
        
		Visible = true;	
		UpdateMap();
    }
    
	// Note: This function should be called with the SAME parameters used 
	// when the map was generated (cameraPos, worldSize, xRes, yRes).
	public Vector2I? GetMinimapCoordinate(Vector3 worldPosition)
	{
		// The relevant world coordinates for a top-down minimap are X and Z.
		var worldSize = globals.GetWorldWidth3D(Camera3D, Camera3D.OrbitCenter.Y + Camera3D.OrbitDistance);
		var cameraPos = Camera3D.OrbitCenter;
		
		float Wx = worldPosition.X;
		float Wz = worldPosition.Z;
		
		int width = (int)xRes;
		int height = (int)yRes;

		// 1. Calculate normalized coordinates (nx, ny) ranging from -0.5 to 0.5
		// These represent the position relative to the camera center (cameraPos.X, cameraPos.Z).
		float nx = (Wx - cameraPos.X) / worldSize;
		float ny = (Wz - cameraPos.Z) / worldSize;
		
		// Check if the world position is outside the generated map area.
		// If it's outside, the resulting coordinates will be out of the [0, width/height] range.
		if (Mathf.Abs(nx) > 0.5f || Mathf.Abs(ny) > 0.5f)
		{
			return null;
			// Optionally, return a special value or clamp the coordinates
			// to indicate the position is off the map.
			// For this example, we'll let the calculation proceed, 
			// but be aware the result may be < 0 or >= width/height.
		}

		// 2. Scale normalized coordinates to minimap pixel coordinates (Mx, My)
		// The 0.5 shift centers the map, moving the range from [-0.5, 0.5] to [0, 1].
		float Mx_float = (nx + 0.5f) * width;
		float My_float = (ny + 0.5f) * height;

		// 3. Convert to integer coordinates
		int Mx = (int)Mathf.Floor(Mx_float);
		int My = (int)Mathf.Floor(My_float);

		// Clamp the coordinates to ensure they are within the bounds of the map array [0, width-1] and [0, height-1]
		Mx = Mathf.Clamp(Mx, 0, width - 1);
		My = Mathf.Clamp(My, 0, height - 1);
		
		// Return the centered tilemap cell coordinates
		int MapCellX = Mx - width / 2;
		int MapCellY = My - height / 2;

		return new Vector2I(Mx, My);
	}
	
	private void UpdateMap()
	{
		var worldSize = globals.GetWorldWidth3D(Camera3D, Camera3D.OrbitCenter.Y + Camera3D.OrbitDistance);
		var cameraPos = Camera3D.OrbitCenter;
		var cameraYaw = Camera3D.Yaw;
		GlobalRotationDegrees = Mathf.RadToDeg(cameraYaw);
				
		int width = (int)xRes;
		int height = (int)yRes;
		var result = new Vector2I[width, height];

		Parallel.For(0, width, xi =>
		{
			for (int yi = 0; yi < height; yi++)
			{
				float nx = (xi / (float)width) - 0.5f;
				float ny = (yi / (float)height) - 0.5f;

				float wx = cameraPos.X + nx * worldSize;
				float wz = cameraPos.Z + ny * worldSize;

				float value = FastNoiseLite.GetNoise2D(wx, wz);
				result[xi, yi] = value > 0 ? new Vector2I(4, 2) : new Vector2I(1, 1);
			}
		});

		// Apply results (main thread)
		for (int xi = 0; xi < width; xi++)
		{
			for (int yi = 0; yi < height; yi++)
			{
				var mapPos = new Vector2I(xi - width / 2, yi - height / 2);
				var cellValue = result[xi, yi];
				var currentCellValue = GetCellAtlasCoords(0, mapPos);

				if (cellValue != currentCellValue)
					SetCell(0, mapPos, 0, cellValue);
			}
		}
	}
	
	// private void UpdateMap()
	// {
	// 	var viewport_size = GetViewport().GetVisibleRect().Size;
	// 	var ratio = viewport_size.Y / viewport_size.X;
		
	// 	var worldSize = globals.GetWorldWidth3D(Camera3D, Camera3D.OrbitCenter.Y + Camera3D.OrbitDistance);
	// 	float xRes = XRex;
	// 	float yRes = xRes;
	// 	float mapWorldWidth  = worldSize; // meters shown horizontally
	// 	float mapWorldHeight = worldSize; // meters shown vertically
	// 	var cameraPos = Camera3D.OrbitCenter;
		

	// 	Scale = Vector2.One * viewport_size.X / (xRes * 16);
	// 	GlobalPosition = viewport_size / 2;
	// 	var cameraYaw = Camera3D.Yaw;
	// 	GlobalRotationDegrees = Mathf.RadToDeg(cameraYaw);
		
	// 	// Scale factor to cover rotation (up to 45°)
	// 	float coverageFactor = 1.414f; // √2
	// 	xRes *= coverageFactor;
	// 	yRes *= coverageFactor;
		
	// 	/*
	// 	float cosYaw = Mathf.Cos(cameraYaw);
	// 	float sinYaw = Mathf.Sin(cameraYaw);
	// 	// for (int x = 0; x < xRes; x++)
	// 	// {
	// 	// 	for (int y = 0; y < yRes; y++)
	// 	// 	{
	// 	// 		// Local offsets in minimap space (-0.5 to +0.5)
	// 	// 		float nx = (x / xRes) - 0.5f;
	// 	// 		float ny = (y / yRes) - 0.5f;

	// 	// 		// Scale to world distances
	// 	// 		float localX = nx * mapWorldWidth;
	// 	// 		float localZ = ny * mapWorldHeight;

	// 	// 		// Rotate around camera orbit center by cameraYaw
	// 	// 		float wx = cameraPos.X + (localX * cosYaw - localZ * sinYaw);
	// 	// 		float wz = cameraPos.Z + (localX * sinYaw + localZ * cosYaw);

	// 	// 		// Sample noise
	// 	// 		float value = FastNoiseLite.GetNoise2D(wx, wz);
	// 	// 		var cellValue = value > 0 ? new Vector2I(4, 2) : new Vector2I(1, 1);

	// 	// 		var mapPos = new Vector2I(x, y);
	// 	// 		var currentCellValue = GetCellAtlasCoords(0, mapPos);
	// 	// 		if (cellValue == currentCellValue)
	// 	// 			continue;

	// 	// 		SetCell(0, mapPos, 0, cellValue);
	// 	// 	}
	// 	// }
	// 	*/
		
	// 	for (int xi = 0; xi < xRes; xi++)
	// 	{
	// 		for (int yi = 0; yi < yRes; yi++)
	// 		{
	// 			float nx = (xi / (float)xRes) - 0.5f; // range [-0.5, +0.5]
	// 			float ny = (yi / (float)yRes) - 0.5f;

	// 			float wx = cameraPos.X + nx * mapWorldWidth;
	// 			float wz = cameraPos.Z + ny * mapWorldHeight;

	// 			var mapPos = new Vector2I(xi - (int)(xRes/2), yi - (int)(yRes/2));

	// 			float value = FastNoiseLite.GetNoise2D(wx, wz);
	// 			var cellValue = value > 0 ? new Vector2I(4, 2) : new Vector2I(1, 1);
	// 			var currentCellValue = GetCellAtlasCoords(0, mapPos);

	// 			if (cellValue != currentCellValue)
	// 				SetCell(0, mapPos, 0, cellValue);
	// 		}
	// 	}
	// }
}
