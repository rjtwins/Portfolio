using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public partial class FovChecker3D : Node3D
{
    [Export] public float FovDegrees = 360.0f;
    [Export] public int HorizontalRays = 36;
    [Export] public int VerticalRays = 18;
    [Export] public float MaxDistance = 250.0f;

	private List<MeshInstance3D> DrawnLines = new List<MeshInstance3D>();
	
	[Export] public SparseSphere SparseSphere { get; set; }

    
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
    {
        
    }

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}
	
	public void CheckAndDrawDebugLines()
	{
		this.GlobalPosition = GetParent<Node3D>().GlobalPosition;

		SparseSphere.Show();
		var visibleDirs = CheckFov3D();
		SparseSphere.Generate(visibleDirs.Where(x => x.Length() > 1f).ToList(), HorizontalRays * VerticalRays, MaxDistance);
	}
	
	public void ClearFOVLines()
	{
		SparseSphere.Hide();
	}

    private void DrawLines(List<Vector3> visibleDirs)
    {
		DrawnLines.ForEach(x =>
		{
			x.QueueFree();
		});

		DrawnLines.Clear();
    
		visibleDirs.Where(x => x.Length() > 1f)
		.ToList().ForEach(x =>
		{
			var line = DrawLine(this.GlobalPosition, x);
			// line.GetParent().RemoveChild(line);
			// this.GetParent().AddChild(line);
			DrawnLines.Add(line);
		});
    }

    private List<Vector3> CheckFov3D()
    {
        var visibleDirs = new List<Vector3>();
        var spaceState = GetWorld3D().DirectSpaceState;

		var horizontalStep = Mathf.DegToRad(FovDegrees) / HorizontalRays;
		var verticalStep = Mathf.DegToRad(FovDegrees) / VerticalRays;
		var maxRad = Mathf.DegToRad(FovDegrees);
		
		for (float x = 0; x < maxRad; x += horizontalStep)
		{		    
			for (float y = 0; y < maxRad; y += verticalStep)
			{
				var dir = Vector3.Forward;
				dir = dir.Rotated(Vector3.Up, x); //Yaw
				dir = dir.Rotated(Vector3.Forward, y);
				dir = dir * MaxDistance;
				
				var query = PhysicsRayQueryParameters3D.Create(GlobalPosition, GlobalPosition + dir);
                query.CollideWithBodies = false;
                query.CollideWithAreas = true;
				query.CollisionMask = 3;
				
				var result = spaceState.IntersectRay(query);

                //float distance = MaxDistance;
                if (result.Count != 0)
                {
					// distance = 0;//((Vector3)result["position"] - GlobalPosition).Length();
                }
                else
                {
                	visibleDirs.Add(dir);
                }

			}
		}
                
        // float halfFov = Mathf.DegToRad(FovDegrees / 2f);



        // for (int yawI = 0; yawI < HorizontalRays; yawI++)
        // {
        //     float yaw = Mathf.Lerp(-halfFov, halfFov, (float)yawI / Math.Max(1, HorizontalRays - 1));

        //     for (int pitchI = 0; pitchI < VerticalRays; pitchI++)
        //     {
        //         float pitch = Mathf.Lerp(-halfFov, halfFov, (float)pitchI / Math.Max(1, VerticalRays - 1));

        //         Vector3 dir = -Transform.Basis.Z;
        //         dir = dir.Rotated(Vector3.Up, yaw);
        //         dir = dir.Rotated(Transform.Basis.X, pitch);
        //         dir = dir.Normalized();

        //         var query = PhysicsRayQueryParameters3D.Create(GlobalPosition, GlobalPosition + dir * MaxDistance);
        //         query.CollideWithBodies = false;
        //         query.CollideWithAreas = true;
		// 		query.CollisionMask = 3;

        //         var result = spaceState.IntersectRay(query);

        //         float distance = MaxDistance;
        //         if (result.Count != 0)
        //         {
		// 			distance = 0;//((Vector3)result["position"] - GlobalPosition).Length();
        //         }

        //         visibleDirs.Add(dir * distance);
        //     }
        // }

        return visibleDirs;
    }    
    
    MeshInstance3D DrawLine(Vector3 pos1, Vector3 pos2, Color? color = null)
    {
		if (color == null)
			color = Colors.WhiteSmoke;
			
		var mesh = new MeshInstance3D();
		var iMesh = new ImmediateMesh();
		var mat = new OrmMaterial3D();

		mesh.Mesh = iMesh;
		mesh.CastShadow = GeometryInstance3D.ShadowCastingSetting.Off;

		iMesh.SurfaceBegin(Mesh.PrimitiveType.Lines, mat);
		iMesh.SurfaceAddVertex(pos1);
		iMesh.SurfaceAddVertex(pos2);
		iMesh.SurfaceEnd();

		mat.ShadingMode = BaseMaterial3D.ShadingModeEnum.Unshaded;
		mat.AlbedoColor = color.Value;
		GetTree().Root.AddChild(mesh);

		return mesh;		
    }
}
