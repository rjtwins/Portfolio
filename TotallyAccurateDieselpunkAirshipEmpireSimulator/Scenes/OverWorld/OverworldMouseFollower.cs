using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public partial class OverworldMouseFollower : Area2D
{
	public static OverworldMouseFollower Instance {private set; get;}
	public List<Area2D> CollidingAreas = new();
	
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		Instance = this;
		AreaEntered += (Area2D area) => CollidingAreas.Add(area);
		AreaExited += (Area2D area) => CollidingAreas.Remove(area);
	}
	
	public bool IsOverUI()
	{
		return UIMouseFollower.Instance.CollidingAreas.OfType<UIArea2d>().Any();
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		GlobalPosition = GetGlobalMousePosition();
		//GD.Print(CollidingAreas.Select(x => x.Name).ToArray());
	}
	
	public bool HasFleetOrSettlementUnderMouse()
	{
		return CollidingAreas
			.Select(x => x.Owner)
			.Any(x => x is Fleet || x is Settlement);
	}
	
	public bool HasFleetUnderMouse()
	{
		return CollidingAreas
			.Select(x => x.Owner)
			.Any(x => x is Fleet);
	}
	
	public bool HasSettlementUnderMouse()
	{
		return CollidingAreas
			.Select(x => x.Owner)
			.Any(x => x is Settlement);
	}
	
	public Node2D GetFleetOrSettlementUnderMouse()
	{
		return CollidingAreas
			.Select(x => x.Owner)
			.Where(x => x is Fleet || x is Settlement)
			.OfType<Node2D>()
			.FirstOrDefault();
	}
}
