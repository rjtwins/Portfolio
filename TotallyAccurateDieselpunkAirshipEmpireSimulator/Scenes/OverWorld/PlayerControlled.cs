using Godot;
using System;

/// <summary>
/// Any fleet in this state will never exit if except for from external input.
/// </summary>
public partial class PlayerControlled : StateBase
{
	
	
	[Export]
	public Fleet Fleet {get; set;}
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		
	}
	
	public override void Update(double delta)
	{
		return;
	}

	public override void Enter()
	{
		base.Enter();
	}

	public override void Exit()
	{
		base.Exit();
	}
}
