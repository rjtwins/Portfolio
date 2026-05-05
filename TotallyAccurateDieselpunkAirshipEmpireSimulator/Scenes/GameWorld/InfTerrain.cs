using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public partial class InfTerrain : Node3D
{
	[Export] public int chunkSize = 100;
    [Export] public int terrain_height = 20;
    [Export] public int view_distance = 500;
    [Export] public OrbitalCamera viewer;
    [Export] public PackedScene chunk_mesh_scene;
    [Export] public bool render_debug = false;
    [Export] public FastNoiseLite noise;

	[Export] public float NoiseFrequency = 0.0002f;

    private Vector2 viewer_position = Vector2.Zero;
    private Dictionary<Vector2I, Chunk> terrain_chunks = new();
    private int chunksvisible = 0;

    public override void _Ready()
    {
        chunksvisible = Mathf.RoundToInt((float)view_distance / chunkSize);

        if (render_debug)
            SetWireframe();

        UpdateVisibleChunk(0);

		NoiseFrequency = noise.Frequency;
		
		GameWorldTest.MapModeChanged += MapModeChanged;
    }

    private void MapModeChanged(bool newValue)
    {
        Visible = !newValue;
    }


    private void SetWireframe()
    {
        RenderingServer.SetDebugGenerateWireframes(true);
        GetViewport().DebugDraw = Viewport.DebugDrawEnum.Wireframe;
    }

    public override void _Process(double delta)
    {

		try
		{
            var cameraOrbit = viewer.OrbitCenter;
			viewer_position.X = cameraOrbit.X;
			viewer_position.Y = cameraOrbit.Z;
		} catch (Exception)
		{		    
			viewer_position.X = viewer.GlobalPosition.X;
			viewer_position.Y = viewer.GlobalPosition.Z;
		}

		if (noise.Frequency != NoiseFrequency)
		{
			terrain_chunks.ToList().ForEach(x => x.Value.QueueFree());
			terrain_chunks.Clear();
			NoiseFrequency = noise.Frequency;
		}
        
        UpdateVisibleChunk(delta);
    }

    private void UpdateVisibleChunk(double delta)
    {
        // Compute viewer grid position
        int currentX = Mathf.RoundToInt(viewer_position.X / chunkSize);
        int currentY = Mathf.RoundToInt(viewer_position.Y / chunkSize);

        // Iterate across visible area
        for (int yOffset = -chunksvisible; yOffset < chunksvisible; yOffset++)
        {
            for (int xOffset = -chunksvisible; xOffset < chunksvisible; xOffset++)
            {
                Vector2I view_chunk_coord = new Vector2I(currentX - xOffset, currentY - yOffset);

                if (terrain_chunks.ContainsKey(view_chunk_coord))
                {
                    var chunk = terrain_chunks[view_chunk_coord];

                    chunk.UpdateChunk(viewer_position, view_distance);

                    if (chunk.UpdateLOD(viewer_position, delta))
                    {
                        chunk.GenerateTerrain(noise, view_chunk_coord, chunkSize, true);
                    }
                }
                else
                {
                    Chunk chunk = chunk_mesh_scene.Instantiate<Chunk>();
                    AddChild(chunk);

                    chunk.Terrain_Max_Height = terrain_height;

                    Vector2 pos = view_chunk_coord * chunkSize;
                    Vector3 worldPos = new Vector3(pos.X, 0, pos.Y);

                    chunk.GlobalPosition = worldPos;
                    chunk.GenerateTerrain(noise, view_chunk_coord, chunkSize, false);

                    terrain_chunks[view_chunk_coord] = chunk;
                }
            }
        }

        // Remove chunks out of range
        foreach (Node child in GetChildren())
        {
            if (child is Chunk chunk)
            {
                if (chunk.ShouldRemove(viewer_position, view_distance))
                {
                    if (terrain_chunks.ContainsKey(chunk.grid_coord))
                        terrain_chunks.Remove(chunk.grid_coord);

                    chunk.QueueFree();
                }
            }
        }
    }

    public int GetActiveThreads()
    {
        return 0;
    }
}
