using Godot;
using System;
using System.Linq;

public partial class Wander : StateBase
{
	[Export]
	public Fleet Fleet {get; set;}
	
	public Vector2 TargetPosition {get; set;}
	
	private void PickTarget()
	{			
		var targets = (Owner.Owner as OverWorld)
		.Settlements
		.OrderBy(x => x.GlobalPosition.DistanceTo(Fleet.GlobalPosition))
		.Take(5)
		.ToList();
		
	
		
		var random = new RandomNumberGenerator();
		random.Randomize();
		random.RandiRange(0, 4);
		
		TargetPosition = targets[random.RandiRange(0, 4)].GlobalPosition;
		Fleet.MoveToPoint(TargetPosition);
	}

	public override void Update(double delta)
	{
		if(!Active)
			return;
			
		//Are we at the target?
		if(Fleet.GlobalPosition.DistanceTo(TargetPosition) <= 1)
			EmitSignal("OnTransitioned", this, "Idle");
	}

	public override void Enter()
	{
		base.Enter();
		PickTarget();
		Fleet.FleetInfo.EngineLevel = 1f;
	}

	public override void Exit()
	{
		base.Exit();
		Fleet.MoveToPoint(Fleet.GlobalPosition);
	}
}
