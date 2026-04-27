using Godot;
using System;
using System.Linq;

public partial class BulletBase : RigidBody3D
{
	[Export] Timer LifeTimeTimer { get; set; }
	[Export] Timer PhysicsTimer { get; set; }
	[Export] float Damage { get; set; }
	[Export] float DamageRadius { get; set; }
	[Export] Curve DamageFalloff { get; set; }

	[Export] public float FusingTime { get; set; } = 1f; //Sec

	private Vector3 _startPos { get; set; }
	private Vector3 _lastPos { get; set; }
	PhysicsDirectSpaceState3D _spaceState { get; set; }
	
	//[Export] Node3D Line3D { get; set; }
	
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
    {
		_startPos = GlobalPosition;
		_lastPos = GlobalPosition;
		_spaceState = GetWorld3D().DirectSpaceState;
		
		LifeTimeTimer.Timeout += LifeTimeTimerTimeout;
		PhysicsTimer.Timeout += () =>
		{
			if (FusingTime > 0)
			{
			    _lastPos = GlobalPosition;
				return;
			}

			if(DetectHit())
				HasHit();
				
			_lastPos = GlobalPosition;
		};
    }

    private void LifeTimeTimerTimeout()
    {
		GetParent().RemoveChild(this);
		// Line3D.Call("clear_points");
		// Line3D.QueueFree();
		QueueFree();
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta)
    {        
		FusingTime -= (float)delta;
    }
	
	public override void _PhysicsProcess(double delta)
	{
		var vel = LinearVelocity;
		if (vel.LengthSquared() > 0.0001f)
			LookAt(GlobalPosition + vel * (float)delta);
	}

    private bool DetectHit()
    {
		var results = _spaceState.IntersectRay(new PhysicsRayQueryParameters3D()
		{
			CollideWithAreas = true,
			CollideWithBodies = false,
			CollisionMask = 2,
		 	From = _lastPos,
		 	To = GlobalPosition,
		});

		if (results.Any())
		{
			//GD.Print($"HIT: {results["collider"]}");
			return true;
		}

		return false;
    }


    public void HasHit()
    {
        _spaceState = GetWorld3D().DirectSpaceState;
        var sphereShape = new SphereShape3D { Radius = DamageRadius };
		var queryParams = new PhysicsShapeQueryParameters3D
        {
            Shape = sphereShape,
            Transform = new Transform3D(Basis.Identity, GlobalPosition),
            CollisionMask = 2, // optional: limit to certain layers
            CollideWithAreas = true,
            CollideWithBodies = false
        };
        
        var results = _spaceState
        	.IntersectShape(queryParams, 128)
			.Where(x => x.ContainsKey("collider"))
			.Select(x => (Node3D)x["collider"])
			.Where(x => x.GetParent() is Component)
			.Select(x => new { Distance = GlobalPosition.DistanceTo(x.GlobalPosition), Node = x.GetParent<Component>() })
			.OrderBy(x => x.Distance)
			.ToList();

		//GD.Print($"{string.Join(',', results.Select(x => $"{x.Node}, at {x.Distance}"))}");

		results.ForEach(x => x.Node.TakeDamage(DamageFalloff.Sample(x.Distance) * Damage));
		
		// Line3D.Call("clear_points");
		// Line3D.QueueFree();
		QueueFree();
    }
}
