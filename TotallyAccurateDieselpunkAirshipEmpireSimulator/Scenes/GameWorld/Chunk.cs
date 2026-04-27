using System;
using Godot;

public partial class Chunk : MeshInstance3D
{
    [Export(PropertyHint.Range, "20,400,1")]
    public int Terrain_Size = 200;

    [Export(PropertyHint.Range, "1,100,1")]
    public int resolution = 10;

    [Export]
    public int Terrain_Max_Height = 5;

	[Export]
	public int Terrain_Min_Height = 1;

    [Export]
    public int[] chunk_lods = new int[] { 2, 4, 8, 15, 20, 50 };

    [Export]
    public int[] LOD_distances = new int[] { 2000, 1500, 1050, 900, 790, 550 };

    public Vector2 position_coord = Vector2.Zero;
    public Vector2I grid_coord = Vector2I.Zero;

    private const float CENTER_OFFSET = 0.5f;

    private bool set_collision = false;

    public override void _Ready()
    {
        
    }

    public override void _Process(double delta)
    {
        
    }


    // -------------------------------------------------------------------------
    // GENERATE TERRAIN
    // -------------------------------------------------------------------------
    public void GenerateTerrain(FastNoiseLite noise, Vector2 coords, float size, bool initially_visible)
    {
        Terrain_Size = (int)size;

        grid_coord = (Vector2I)coords;
        position_coord = coords * size;

        ArrayMesh aMesh;
        SurfaceTool st = new SurfaceTool();
        st.Begin(Mesh.PrimitiveType.Triangles);

        // Vertex generation
        for (int z = 0; z < resolution + 1; z++)
        {
            for (int x = 0; x < resolution + 1; x++)
            {
                Vector2 percent = new Vector2((float)x, (float)z) / resolution;

                Vector3 pointOnMesh = new Vector3(
                    percent.X - CENTER_OFFSET,
                    0,
                    percent.Y - CENTER_OFFSET
                );

                Vector3 vertex = pointOnMesh * Terrain_Size;

				// Continuous world noise
				var noiseY = noise.GetNoise2D(position_coord.X + vertex.X, position_coord.Y + vertex.Z);
				vertex.Y = noiseY > 0 ? noiseY * Terrain_Max_Height : noiseY * Terrain_Max_Height * 0.3f;

                Vector2 uv = percent;

                st.SetUV(uv);
                st.AddVertex(vertex);
            }
        }

        // Indices (triangles)
        int vert = 0;
        for (int z = 0; z < resolution; z++)
        {
            for (int x = 0; x < resolution; x++)
            {
                st.AddIndex(vert + 0);
                st.AddIndex(vert + 1);
                st.AddIndex(vert + resolution + 1);

                st.AddIndex(vert + resolution + 1);
                st.AddIndex(vert + 1);
                st.AddIndex(vert + resolution + 2);

                vert++;
            }
            vert++;
        }

        st.GenerateNormals();
        aMesh = st.Commit();

        Mesh = aMesh;

        if (set_collision)
            CreateCollision();

        SetChunkVisible(initially_visible);
    }


    // -------------------------------------------------------------------------
    // COLLISION
    // -------------------------------------------------------------------------
    private void CreateCollision()
    {
        // Same as GDScript: placeholder
        // If needed you can uncomment and use CreateTrimeshCollision()
        // if (GetChildCount() > 0) GetChild(0).QueueFree();
        // CreateTrimeshCollision();
    }


    // -------------------------------------------------------------------------
    // VISIBILITY UPDATE
    // -------------------------------------------------------------------------
    public void UpdateChunk(Vector2 viewPos, float maxViewDist)
    {
        float viewer_distance = position_coord.DistanceTo(viewPos);
        bool is_visible = viewer_distance <= maxViewDist;
        SetChunkVisible(is_visible);
    }


    // -------------------------------------------------------------------------
    // REMOVE CHECK (SLOW)
    // -------------------------------------------------------------------------
    public bool ShouldRemove(Vector2 viewPos, float maxViewDist)
    {
        float viewer_distance = position_coord.DistanceTo(viewPos);
        return viewer_distance > maxViewDist;
    }


    // -------------------------------------------------------------------------
    // LOD UPDATE
    // -------------------------------------------------------------------------
    public bool UpdateLOD(Vector2 viewPos, double delta)
    {
        float viewer_distance = position_coord.DistanceTo(viewPos);
        bool update_terrain = false;

		// DebugDraw3D.ScopedConfig().SetTextOutlineSize(0);
		// DebugDraw3D.DrawText(GlobalPosition + Vector3.Up * 300, $"{viewer_distance}", 4000);

        if (chunk_lods.Length != LOD_distances.Length)
        {
            GD.Print("ERROR Lods and Distance count mismatch");
            return false;
        }

        int newLOD = chunk_lods[0];

        for (int i = 0; i < chunk_lods.Length; i++)
        {
            int lod = chunk_lods[i];
            int dist = LOD_distances[i];

            if (viewer_distance < dist)
                newLOD = lod;
        }

        // Collision only in highest LOD
        if (newLOD >= chunk_lods[chunk_lods.Length - 1])
            set_collision = true;
        else
            set_collision = false;

        if (resolution != newLOD)
        {
            resolution = newLOD;
            update_terrain = true;
        }

        return update_terrain;
    }

    // -------------------------------------------------------------------------
    // VISIBILITY CONTROL
    // -------------------------------------------------------------------------
    public void SetChunkVisible(bool isVisible)
    {
        Visible = isVisible;
    }
}
