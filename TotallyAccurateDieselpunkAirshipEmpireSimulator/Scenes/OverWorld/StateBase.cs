using Godot;
using System;

public partial class StateBase : Node
{
	[Signal]
	public delegate void OnTransitionedEventHandler();
	
	public bool Active { get; set; } = false;
	
	//Setup state
	public virtual void Enter()
	{
		Active = true;
	}
	
	//Clean up state
	public virtual void Exit()
	{
		Active = false;
	}
	
	//Update owner
	public virtual void Update(double delta)
	{
		
	}
}
