using Godot;
using System;

public partial class GridDots : Node3D
{
	[Export] public int GridSize = 10;
	[Export] public float DotScale = 0.1f;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
    {
		UpdateDots();
    }
    
    public void UpdateDots()
    {
		var mesh = new SphereMesh
        {
            Radius = DotScale,
            Height = DotScale * 2,
        };
        
        for (int x = 0; x <= GridSize; x++)
        {
            for (int y = 0; y <= GridSize; y++)
            {
                for (int z = 0; z <= GridSize; z++)
                {
                    var dot = new MeshInstance3D
                    {
                        Mesh = mesh,
                        Transform = new Transform3D(Basis.Identity, new Vector3(x, y, z))
                    };
                    AddChild(dot);
                }
            }
        }
    }

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
    {
        
    }
}
