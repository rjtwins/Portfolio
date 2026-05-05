using Godot;
using System;
using System.Runtime.CompilerServices;

public partial class GridMapMinimap : GridMap
{
	[Export] FastNoiseLite Noise;
	[Export] public int MapSize { get; set; } = 1000;
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
    {
		SetupMap();
    }

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
    {
		
    }
    
    private void SetupMap()
    {
        for (int x = -1 * MapSize; x < MapSize; x += 50)
		{
			for (int y = -1 * MapSize; y < MapSize; y += 50)
			{
				var gridCoordinate = new Vector3(x, 0, y);
				var n = Noise.GetNoise2D(x, y);
				n = (float)globals.Normalize(n, -1, 1);
				//GD.Print(n);
				SetCellItem(LocalToMap(gridCoordinate), Mathf.RoundToInt(n));
			}
		}
    }
    
    private void UpdateMap()
    {
		this.GetUsedCells();
    }
}
