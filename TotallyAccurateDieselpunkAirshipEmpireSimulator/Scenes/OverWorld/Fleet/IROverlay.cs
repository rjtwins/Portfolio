using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public partial class IROverlay : Node2D
{
	[Export]
	public Line2D Line {get; set;}
	[Export]
	public Timer UpdateTimer {get; set;}
	[Export]
	public IR IRSensor {get; set;}
	
	private RandomNumberGenerator _RNG;
	private Dictionary<int, float> _receivedContacts = new();
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		_RNG = new();
		_RNG.Randomize();
		UpdateTimer.Timeout += UpdateTimerTimeout;
		IRSensor.OnReturnFromPoint += ContactReceived;
		
		TreeExiting += () => 
		{
			UpdateTimer.Timeout -= UpdateTimerTimeout;
			IRSensor.OnReturnFromPoint -= ContactReceived;
		};
	}

	private void ContactReceived(Vector2 point, float strength)
	{
		var d = (int)Math.Round(Mathf.RadToDeg(GlobalPosition.DirectionTo(point).Angle()));
		d = d < 0 ? d + 360 : d;
		if(_receivedContacts.TryGetValue(d, out float s_strength))
			strength = Math.Max(strength, s_strength);
		
		_receivedContacts[d] = strength;
	}

	private void UpdateTimerTimeout()
	{
		var dirs = Enumerable
		.Range(0, 20)
		.ToList()
		.Select(x => x * 18)
		.Union(_receivedContacts.Select(x => x.Key).ToList())
		.Distinct()
		.OrderBy(x => x)
		.ToList();
		
		var points = dirs
		.Select(d =>
		{
			var r = _RNG.RandfRange(50, 60);
			if(_receivedContacts.ContainsKey(d))
				r = _receivedContacts[d];
			
			return globals.CalculateVector(Mathf.DegToRad(d), r);
		}).ToList();
		
		_receivedContacts.Clear();
		Line.ClearPoints();
		Line.Points = points.ToArray();
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		var camera = GetViewport().GetCamera2D();
		if (camera == null)
			return;
		Scale = Vector2.One * (Vector2.One / camera.Zoom);
	}
}
