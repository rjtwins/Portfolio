using Godot;

public partial class MapMissile : Node2D
{
	[Export]
	public Area2D DetectionArea {get; set;}
	public float InitVelocity {get; set;} = 500;
	//Fight time in sec.
	public float FlyTimeRemaining {get;set;} = int.MaxValue;
	public MissileData MissileData { get; set; }
	
	private Area2D _target = null;
	
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		//LinearVelocity = InitVelocity * Vector2.Right.Rotated(GlobalRotation);
		DetectionArea.AreaEntered += AreaEntered;
		DetectionArea.AreaExited += AreaExited;

		FlyTimeRemaining = MissileData.FlyTime;
	}

	private void AreaExited(Area2D area)
	{
		if(_target == area)
			_target = null;
	}

	private void AreaEntered(Area2D area)
	{
		if(area == DetectionArea)
			return;
		
		if(_target != null)
			return;
			
		_target = area;
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		if(FlyTimeRemaining <= 0)
		{
			GetParent().RemoveChild(this);
			QueueFree();
		}
		
		FlyTimeRemaining -= (float)delta;
	}

	public override void _PhysicsProcess(double delta)
	{
		if(_target == null || _target.IsQueuedForDeletion())
		{
			_target = null;
			GlobalPosition += InitVelocity * (float)delta * Vector2.Right.Rotated(GlobalRotation);
			return;
		}
		
		LookAt(_target.GlobalPosition);
		Position = Position.MoveToward(_target.GlobalPosition, InitVelocity * (float)delta);
				
		if(GlobalPosition.DistanceTo(_target.GlobalPosition) < 1)
			Hit();
	}

	public void Hit()
	{
		GD.Print($"Missile hit {_target.Owner.Name}");
		GetParent().RemoveChild(this);
		QueueFree();
	}
}
