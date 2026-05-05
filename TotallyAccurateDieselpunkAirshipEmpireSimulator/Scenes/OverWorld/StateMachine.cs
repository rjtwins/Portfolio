using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public partial class StateMachine : Node
{
	
	[Export]
	public StateBase InitialState {get; set;}
	
	public StateBase ActiveState {get; set;}
	
	public List<StateBase> States {get; set;} = new();
	public Dictionary<string, StateBase> StateDict = new();
	
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		States = GetChildren().OfType<StateBase>().ToList();
		StateDict = States.ToDictionary(x => x.Name.ToString().ToLower(), x => x);
		States.ForEach(x => x.Connect("OnTransitioned", new(this, "OnChildTransitioned")));
	}
	
	public void OnWorldReady()
	{
		OnChildTransitioned(InitialState, InitialState.Name.ToString().ToLower());
	}
	
	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		
	}

	public override void _PhysicsProcess(double delta)
	{
		ActiveState?.Update(delta);
		base._PhysicsProcess(delta);
	}

	
	public void OnChildTransitioned(StateBase state, string newStateName)
	{
		// if(state != ActiveState)
		// 	return;
		
		if(!StateDict.TryGetValue(newStateName.ToLower(), out StateBase newState))
			return;
		
		//GD.Print($"Fleet {GetParent().Name} moved to state: {newStateName}");
		ActiveState?.Exit();
		newState.Enter();
		ActiveState = newState;
	}
}
