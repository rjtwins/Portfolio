using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public partial class MapStrike : Node2D
{
	[Export] Selectable selectable;
	[Export] public MapAircraft[] aircraft;
	[Export] float FuelTime {get; set;} = 30f;
	[Export] float UpTime {get; set;} = 0f;
	[Export] float Velocity {get; set;} = 250f.MMeterToPixel();
	
	[Export] public Fleet CarrierFleet {get; set;}
	[Export] Node2D targetNode {get; set;}
	[Export] Godot.Collections.Array<Vector2> waypoints {get; set;} = new();
	[Export] Vector2 targetPosition {get; set;}
	[Export] float visionRange {get; set;}
	[Export] Area2D CollisionArea {get; set;}
	
	public float TotalRange => FuelTime * 60 * Velocity;
	public float MaxRange => TotalRange / 2;
	
	[Export] public bool ReturningToCarrier {get; private set;} = false;
	
	public override void _Ready()
	{
		targetPosition = (Vector2.Right * 1e9f).Rotated(GlobalRotation);
	}

	public override void _Process(double delta)
	{
		UpTime += (float)delta;
		
		if(UpTime >= FuelTime / 2 && !ReturningToCarrier)
		{
			MoveToNode(CarrierFleet);
			ReturningToCarrier = true;
		}
		
		if(ReturningToCarrier && GlobalPosition.DistanceTo(CarrierFleet.GlobalPosition) < 1f)
		{
			CarrierFleet.StrikeManager.RetrieveStrike(aircraft.ToList());
			GetParent().RemoveChild(this);
			QueueFree();
			return;
		}
		
		if(targetNode?.IsQueuedForDeletion() ?? false)
			targetNode = null;
		
		//Update target position
		if(targetNode != null)
			MoveToPoint(targetNode.GlobalPosition);
		
		if(targetPosition.DistanceTo(GlobalPosition) < 1)
		{
			targetPosition = waypoints.FirstOrDefault();
			
			if(targetPosition != default)
				waypoints.Remove(targetPosition);
		}
		
		if(targetPosition == default)
		{
			MoveToNode(CarrierFleet);
		}
		
		//Move towards target
		LookAt(targetPosition);
		GlobalPosition = GlobalPosition.MoveToward(targetPosition, Velocity * (float)delta * 60);
		
		base._Process(delta);
	}

	public override void _PhysicsProcess(double delta)
	{		
		base._PhysicsProcess(delta);
	}

	public override void _UnhandledInput(InputEvent @event)
	{
		HandleOrder(@event);
		base._UnhandledInput(@event);
	}
	
	public void HandleOrder(InputEvent @event)
	{
		if(ReturningToCarrier)
			return;
		if(!(@event is InputEventMouseButton mouseButton))
			return;
		if(mouseButton.ButtonIndex != MouseButton.Right)
			return;
		if(!mouseButton.IsReleased())
			return;
		if(!selectable.Selected)
			return;
		
		if(OverworldMouseFollower.Instance.HasFleetOrSettlementUnderMouse())
			MoveToNode(OverworldMouseFollower.Instance.GetFleetOrSettlementUnderMouse());
		else
			MoveToPoint(GetGlobalMousePosition());
	}

	public void MoveToWaypoints(List<Vector2> wps)
	{
		waypoints.Clear();
		waypoints.AddRange(wps);
		targetPosition = waypoints.FirstOrDefault();
		if(targetPosition != default)
			waypoints.Remove(targetPosition);
	}
	
	public void MoveToPoint(Vector2 point)
	{
		targetPosition = point;
	}
	
	private void MoveToNode(Node2D node)
	{
		targetNode = node;
	}
}
