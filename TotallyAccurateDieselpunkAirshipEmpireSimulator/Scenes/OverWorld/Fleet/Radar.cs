using System;
using Godot;
using System.Linq;

public partial class Radar : Node2D
{
	[Signal]
	public delegate void OnReturnFromPointEventHandler(Vector2 point);
	
	// [Export]
	// Area2D RadarArea {get; set;}
	// [Export]
	// CollisionShape2D RadarRay {get; set;}
	[Export]
	public ShapeCast2D RadarRay {get; set;}
	[Export]
	public Area2D RadarArea {get; set;}

	private CollisionShape2D RadarAreaCollisionShape => RadarArea.GetChild<CollisionShape2D>(0);
	private CircleShape2D RadarAreaCircleShape => RadarAreaCollisionShape.Shape as CircleShape2D;
	
	[Export]
	public Area2D OwnRadarArea {get; set;}
	
	[Export]
	public Godot.Timer UpdateTimer {get; set;}
	
	[Export]
	public float ScanningSpeed {get; set;} = 10f;
	
	[Export] float MaxScanningSpeed {get; set;} = 8000f;
	
	[Export]
	public float RadarRange {get; set;} = 1000f;
	[Export]
	public float ConeMin {get; set;} = -5;
	[Export]
	public float ConeMax {get; set;} = 5;
	[Export]
	public bool ScanCone {get; set;} = true;
	[Export]
	public bool Enabled {get; set;} = true;
	[Export]
	public float CurrentDir {get; set;}
	private int dir = 1;
	
	private Guid _guid;
	
	private RandomNumberGenerator _RNG;	
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		RadarRay.TargetPosition = new Vector2(RadarRange, 0);
		RadarRay.AddException(OwnRadarArea);
		
		_RNG = new RandomNumberGenerator();
		_RNG.Randomize();
		_guid = Guid.NewGuid();		
		UpdateTimer.Timeout += Update;
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	private void Update()
	{
		//ReceiveJamming(new Vector2(500, 500));
		if(!Enabled)
		{
			return;
		}
		
		if(!RadarArea.Monitoring)
			return;
			
		var areas = RadarArea.GetOverlappingAreas().ToList();
		areas.RemoveAll(x => x == OwnRadarArea);
		
		for (int i = 0; i < areas.Count; i++)
		{			
			if((areas[i] as Node).Owner is IRWR RWR)
				RWR.ReceiveRadiation(this.GlobalPosition, RadiationType.SearchRadar, _guid);
			
			var pos = areas[i].GlobalPosition;
			pos += new Vector2(GD.Randf() * 25, GD.Randf() * 25);
			EmitSignal("OnReturnFromPoint", pos);
		}
	}

    public override void _PhysicsProcess(double delta)
    {
		if(!Enabled)
		{
		    RadarArea.Monitoring = false;
			return;
		}
		
		RadarAreaCircleShape.Radius = RadarRange;
		
		if(Engine.TimeScale >= 1)
        {
            RadarArea.Monitoring = true;
            RadarRay.TargetPosition = Vector2.Zero;
        }
        else
        {
        	RadarArea.Monitoring = false;
			var oldDir = CurrentDir;
			var newDir = CurrentDir;
			var scanMove = Math.Min(ScanningSpeed * (float)delta, MaxScanningSpeed);
			
			if(!ScanCone)
				newDir += scanMove * dir;
			else
			{
				newDir = Mathf.Clamp(CurrentDir, ConeMin, ConeMax);
				if(newDir == ConeMin)
					dir = 1;
				else if(newDir == ConeMax)
					dir = -1;
					
				newDir += scanMove * dir;
			}
			
			CurrentDir = newDir;
			var target = globals.CalculateVector(Mathf.DegToRad(newDir), RadarRange);
			RadarRay.TargetPosition = target;
        }
        
        RefineRayCollisions();
    }

	
	public override void _Process(double delta)
	{

	}
	
	//To be used to refine the collision point.
	private void RefineRayCollisions()
	{
		var nrOfColliders = RadarRay.GetCollisionCount();
		
		if(nrOfColliders > 0)
		{
			//GD.Print("COLLISION!!!");
			
			for (int i = 0; i < nrOfColliders; i++)
			{			
				if((RadarRay.GetCollider(i) as Node).Owner is IRWR RWR)
					RWR.ReceiveRadiation(this.GlobalPosition, RadiationType.SearchRadar, _guid);
					
				EmitSignal("OnReturnFromPoint", RadarRay.GetCollisionPoint(i));
			}
		}
	}
	
	//Display a jamming signal
	public void ReceiveJamming(Vector2 from)
	{
		
		var jammingDirection = GlobalPosition.DirectionTo(from).Angle();
		
		Enumerable.Range(0, 1000)
		.ToList()
		.ForEach(x => 
		{
			var dirDif = _RNG.RandfRange(-5, 5);
			var range = _RNG.RandfRange(0, RadarRange);
			var point = globals.CalculateVector(jammingDirection + Mathf.DegToRad(dirDif), range);
			point += GlobalPosition;
			EmitSignal("OnReturnFromPoint", point);
		});
	}
}
