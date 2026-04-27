using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public partial class OverWorld : Node2D
{
	//[Signal]
	//public delegate void OnWorldReadyEventHandler();
	
	[Export] 
	public int MinNodes = 5;
	[Export] 
	public int MaxNodes = 20;
	[Export] 
	public PackedScene SettlementScene;
	[Export] 
	public PackedScene LineScene;
	
	private List<Vector2> _positions = new();
	
	public List<Settlement> Settlements {get;set;}= new();
	
	public static OverWorld Instance { get; private set; }
	
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		Instance = this;
	
		GenerateRandomNodes();
		DrawLinesBetweenNodes();
		
		GetTree().CallGroup("FleetStateMachine", "OnWorldReady");
		
		//EmitSignal("OnWorldReady");
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		
	}
	
	private Vector2 GenerateNextPosition()
	{
		RandomNumberGenerator rng = new RandomNumberGenerator();
		Vector2 pos;
		var maxRange = 	10000000f.MMeterToPixel();
		var range = 	5001f;
		do
		{
			pos = new Vector2(rng.RandfRange(0, maxRange), rng.RandfRange(0, maxRange));
			if(_positions.Count() > 1)
				range = _positions.OrderBy(x => x.DistanceTo(pos)).Select(x => x.DistanceTo(pos)).First();
			
		} while (range <= 5000f);
		
		return pos;
	}
	
	private void GenerateRandomNodes()
	{
		// Get the number of nodes to create
		RandomNumberGenerator rng = new RandomNumberGenerator();
		int numNodes = rng.RandiRange(MinNodes, MaxNodes);

		for (int i = 0; i < numNodes; i++)
		{
			// Instantiate a node from the scene
			var node = (Settlement)SettlementScene.Instantiate();
			
			
			// Set a random position for the node
			Vector2 position = i == 0 ? Vector2.One * 1000 : GenerateNextPosition();
			node.Position = position;
			_positions.Add(position);
			AddChild(node);
			
			Settlements.Add(node);
			node.Data = new();
			node.Data.Name = init.SettlementNames[rng.RandiRange(0, init.SettlementNames.Count() -1)];
		}
		
		Settlements.First().Data.Owner = Faction.PLAYER;
	}
	
	private void DrawLinesBetweenNodes()
	{
		foreach (Vector2 position in _positions)
		{
			// Find the two nearest neighbors
			List<Vector2> nearestNeighbors = FindNearestNeighbors(position);
			
			// Draw lines to the two nearest neighbors
			foreach (Vector2 neighbor in nearestNeighbors)
			{
				var line = (ScreenLine2D)LineScene.Instantiate();
				line.Points = new Vector2[] { neighbor, position };
				line.DefaultColor = new Color("#CCD1D1", 0.75f);
				AddChild(line);
			}
		}
	}
	
	private List<Vector2> FindNearestNeighbors(Vector2 position)
	{
		return _positions.OrderBy(x => x.DistanceTo(position)).Skip(1).Take(2).ToList();
	}
	
	public override void _Notification(int what)
	{
		if(what != NotificationVisibilityChanged)
		{
			base._Notification(what);
			return;
		}
		
		var camera = GetViewport().GetCamera2D();
		if(camera != null)
		{
			camera.Offset = Vector2.Zero;
			camera.GlobalPosition = Vector2.Zero;
		
			if(!Visible)
				camera.Enabled = false;
			else
				camera.Enabled = true;
		}

		
		base._Notification(what);
	}
}
