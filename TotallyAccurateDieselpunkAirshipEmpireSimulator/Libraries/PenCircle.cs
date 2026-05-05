using Godot;
using System;

public partial class PenCircle : Node2D
{
	[Export] public bool Placed {get; set;} = false;
	[Export] Label DistanceLabel;
	[Export] ScreenSpaceNode LabelParent;
	[Export] Area2D MouseDetector;
	[Export] CircleLine2D Circle;
	[Export] ScreenLine2D CenterLine;
	CollisionPolygon2D MouseDetectorCollisionShape;
	public Vector2? StartPoint = null;
	public Vector2? EndPoint = null;
	
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		MouseDetectorCollisionShape = new();
		MouseDetector.AddChild(MouseDetectorCollisionShape);
		MouseDetectorCollisionShape.BuildMode = CollisionPolygon2D.BuildModeEnum.Segments;
	}
	
	public void SetPlaced()
	{
		Placed = true;
		var points = Circle.Points;
		MouseDetectorCollisionShape.Polygon = points;
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		base._Process(delta);
		if(Input.IsActionJustReleased("delete_action") && MouseDetector.OverlapsArea(OverworldMouseFollower.Instance))
		{
			QueueFree();
		}
		
		if(Placed)
			return;
		
		if(StartPoint != null)
		{
			GlobalPosition = StartPoint.Value;
			CenterLine.Points = new Vector2[] { StartPoint.Value };
			CenterLine.GlobalPosition = Vector2.Zero;
		}
		
		if(StartPoint != null && EndPoint != null)
		{
			Circle.Radius = StartPoint.Value.DistanceTo(EndPoint.Value);
			Circle.WorldRadius = Circle.Radius;
			Circle.Redraw();
			var distance = Circle.Radius.MPixelToMeter();
			distance = MathF.Round(distance / 500, 1);
			DistanceLabel.Text = $"{distance} KM";
			LabelParent.Position = new Vector2(0, -1 * Circle.Radius);
			CenterLine.Points = new Vector2[] { StartPoint.Value, EndPoint.Value };
		}
	}
}
