using Godot;
using System;
using System.Linq;

public partial class PenLine : ScreenLine2D
{
	[Export] public bool Placed {get; set;} = false;
	[Export] CircleLine2D StartCircle;
	[Export] CircleLine2D EndCircle;
	[Export] Label DistanceLabel;
	[Export] ScreenSpaceNode LabelParent;
	[Export] Area2D MouseDetector;
	CollisionShape2D MouseDetectorCollisionShape;
	public override void _Ready()
	{
		MouseDetectorCollisionShape = new();
		MouseDetector.AddChild(MouseDetectorCollisionShape);
	}

	public void SetPlaced()
	{
		Placed = true;
		
		var height = Points[0].DistanceTo(Points[1]);
		var rot = Points[0].DirectionTo(Points[1]).Angle() - Mathf.DegToRad(90);
		var pos = Points[0] + Points[0].DirectionTo(Points[1]).Normalized() * height / 2;
		MouseDetector.GlobalPosition = pos;
		var shape = new CapsuleShape2D();
		shape.Height = height;
		shape.Radius = 5;
		MouseDetector.Rotation = rot;
		MouseDetectorCollisionShape.Shape = shape;
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
		
		if(Points.Count() > 0)
		{
			StartCircle.GlobalPosition = Points[0];
		}

		if(Points.Count() > 1)
		{
			EndCircle.GlobalPosition = Points[1];
			LabelParent.GlobalPosition = Points[0] + Vector2.Down * 10;
			
			var distance = Points[0].DistanceTo(Points[1]);
			distance = distance.MPixelToMeter();
			distance = MathF.Round(distance / 1000, 1);
			DistanceLabel.Text = $"{distance} KM";
		}
	}
}
