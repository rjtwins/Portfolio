using Godot;
using System;
using System.Linq;

[Tool]
public partial class TerainGen : MeshInstance3D
{

	// [Export] public int XSize { get; set; }
	// //[Export] public float YSize { get; set; }
	// [Export] public int ZSize { get; set; }

	[Export] public bool Update { get; set; } = false;
	[Export] public bool clear_vert_vis { get; set; } = false;

	[Export] public float TerrainHeight { get; set; } = 5f;
	[Export] public float NoiseOffset { get; set; } = 0.5f;

	[Export] public float TerrainSize { get; set; } = 100f;
	[Export] public int Resolution { get; set; } = 30;

	[Export] public bool CreateCollision { get; set; } = true;
	[Export] public bool RemoveCollision { get; set; } = false;

	public float MinHeight = 0f;
	public float MaxHeight = 1f;

	const float centerOffset = 0.5f;
	
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
    {
		GenerateTerrain();
    }

    private void GenerateTerrain()
    {
		var aMesh = new ArrayMesh();
		var surfTool = new SurfaceTool();

		var n = new FastNoiseLite();
		n.NoiseType = FastNoiseLite.NoiseTypeEnum.Perlin;
		n.Frequency = 0.1f;

		surfTool.Begin(Mesh.PrimitiveType.Triangles);
		
		for (int z = 0; z < Resolution + 1; z++)
		{		    
			for (int x = 0; x < Resolution + 1; x++)
			{

				var percent = new Vector2(x, z) / Resolution;
				var pointOnMesh = new Vector3(percent.X - centerOffset, 0, percent.Y - centerOffset);
				var vertex = pointOnMesh * TerrainSize;
				vertex.Y = n.GetNoise2D(x + NoiseOffset, z + NoiseOffset) * TerrainHeight;

				// if (y > MaxHeight)
				// 	MaxHeight = y;

				// if (y < MinHeight)
				// 	MinHeight = y;
				
				var uv = Vector2.Zero;
				uv.X = percent.X;
				uv.Y = percent.Y;

				surfTool.SetUV(uv);
				surfTool.AddVertex(vertex);
				// draw_sphere(new Vector3(x, y, z));
			}
		}

		var vert = 0;
		
		for (int z = 0; z < Resolution; z++)
		{		    
			for (int x = 0; x < Resolution; x++)
			{
				surfTool.AddIndex(vert + 0);
				surfTool.AddIndex(vert + 1);
				surfTool.AddIndex(vert + Resolution + 1);
				surfTool.AddIndex(vert + Resolution + 1);
				surfTool.AddIndex(vert + 1);
				surfTool.AddIndex(vert + Resolution + 2);
				vert += 1;
			}

			vert += 1;
		}

		surfTool.GenerateNormals();
		aMesh = surfTool.Commit();
		Mesh = aMesh;

		updateShader();

		GD.Print("Done...");
    }
    
    // private void draw_sphere(Vector3 pos)
    // {
	// 	var ins = new MeshInstance3D();
	// 	AddChild(ins);
	// 	ins.Position = pos;
	// 	var sphere = new SphereMesh();
	// 	sphere.Radius = 0.1f;
	// 	sphere.Height = 0.2f;
	// 	ins.Mesh = sphere;
    // }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta)
    {
		// if (Engine.IsEditorHint())
		// {
		// 	// Code to run only in the editor
		// 	GD.Print("Running in editor!");
		// }
            
		if (Update)
		{
			GenerateTerrain();
			Update = false;
		}

		if (clear_vert_vis)
			GetChildren().ToList().ForEach(x => x.QueueFree());
    }
    
    public void updateShader()
    {
		var mat = (ShaderMaterial)GetActiveMaterial(0);
		mat.SetShaderParameter("min_height", MinHeight);
		mat.SetShaderParameter("max_height", MaxHeight);
    }
}
