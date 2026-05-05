using Godot;
using System.Linq;
using System.Runtime.Serialization;

public partial class Idle : StateBase
{	
	[Export]
	public Fleet Fleet {get; set;}
	
	private double _idleTime = 0d;
		
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		
	}
	
	private void PickIdleTime()
	{
		_idleTime = GD.RandRange(0, 60);
	}

	public override void Update(double delta)
	{
		if(!Active)
			return;
		
		if(_idleTime <= 0)
			EmitSignal("OnTransitioned", this, "Wander");
		
		_idleTime -= delta;
	}

	public override void Enter()
	{
		base.Enter();
		PickIdleTime();
		Fleet.FleetInfo.EngineLevel = 0f;
		
		GD.Print($"Fleet {Fleet.Name} is in Idle for {_idleTime}");
	}

	public override void Exit()
	{
		base.Exit();
		Fleet.MoveToPoint(Fleet.GlobalPosition);
	}
}
