using Godot;
using System;
using System.Collections.Generic;

public partial class Convoy : Node2D
{
	[Signal]
	public delegate void ArrivedAtSettlementEventHandler(Convoy convoy, Settlement too);
	
	[Export]
	public Fleet Fleet {get; set;}
	
	public Dictionary<string, int> Manifest = new();
	public Settlement From {get; set;}
	public Settlement Too {get; set;}
	
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		Fleet = GetChild(0) as Fleet;
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		
	}
}
