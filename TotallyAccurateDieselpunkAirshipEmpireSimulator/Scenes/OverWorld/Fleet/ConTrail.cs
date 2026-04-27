using System;
using Godot;

public partial class ConTrail : Line2D
{
	private Curve2D _curve;
	
	[Export] public int MaxPoints {get; set;} = 1000;
	[Export] public int MaxLength {get; set;} = 100;
	[Export] public Node2D ShipModel {get; set;}
	[Export] public SceneTreeTimer UpdateTimer {get; set;}
	[Export] public SceneTreeTimer RemoveTimer {get; set;}	
	
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		_curve = new();
		Points = new Vector2[0];
		UpdateTimer = GetTree().CreateTimer(0.05f, processAlways: false, ignoreTimeScale: false);
		UpdateTimer.Timeout += () => 
		{
			Update();
		};
		
		RemoveTimer = GetTree().CreateTimer(0.1f, processAlways: false, ignoreTimeScale: false);
		RemoveTimer.Timeout += () => 
		{
			RemovePoint();
		};
	}

	private void RemovePoint()
	{
		if(_curve.PointCount != 0)
		{
			_curve.RemovePoint(0);
		}
		
		RemoveTimer = GetTree().CreateTimer(0.1f, processAlways: false, ignoreTimeScale: false);
		RemoveTimer.Timeout += () =>
		{
			RemovePoint();
		};
	}

	private void Update()
	{		
		if(_curve.PointCount != 0 && _curve.GetPointIn(_curve.PointCount -1).DistanceTo(ShipModel.GlobalPosition) < 5)
			return;
		
		while(_curve.GetBakedLength() > MaxLength)	
			_curve.RemovePoint(0);
			
		_curve.AddPoint(ShipModel.GlobalPosition + (Vector2.Up * 2));
		
		if(_curve.PointCount > MaxPoints)
			_curve.RemovePoint(0);
			
		Points = _curve.GetBakedPoints();
		UpdateTimer = GetTree().CreateTimer(0.05f, processAlways: false, ignoreTimeScale: false);
		UpdateTimer.Timeout += () =>
		{
			Update();
		};
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{			
		GlobalPosition = Vector2.Zero;
		GlobalRotation = 0f;
		
		// if(GetViewport().GetCamera2D().Offset.DistanceTo(ShipModel.GlobalPosition) < 200 && GetViewport().GetCamera2D().Zoom.X > 3)
		// 	Visible = true;
		// else
		// 	Visible = false;
	}
}
