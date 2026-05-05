using Godot;
using System;
using System.Collections.Generic;

public partial class RadarOverlay : Node2D
{
	[Export]
	public int RangeRingsSize {get; set;} = 100;
	[Export]
	public Radar Radar {get; set;}
	[Export]
	public Color Color {get; set;}
	
	[Export]
	public CircleLine2D OuterCircle {get; set;}
	
	[Export]
	public CircleLine2D InnerCircle {get; set;}
	
	[Export]
	public Label RadarIndicatorsTemplate {get; set;}
	
	[Export]
	public GpuParticles2D PingParticles {get; set;}
	[Export]
	public RadarPingHandler RadarPingHandler {get; set;}
	
	private List<Label> _labels = new();
		
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		Radar.OnReturnFromPoint += VisualizeRadarReturn;
		TreeExiting += () => 
		{
		    Radar.OnReturnFromPoint -= VisualizeRadarReturn;
		};
		
		// for (int i = 0; i < 12; i++)
		// {
		// 	Label l = RadarIndicatorsTemplate.Duplicate() as Label;
		// 	l.Visible = true;
		// 	_labels.Add(l);
		// 	AddChild(l);
		// }
		
		
		var degLineColor = Color;
		degLineColor.A = degLineColor.A * 0.25f;		
		InnerCircle = new()
		{
			ScreenWidth = 1,
			MaintainPixelWidth = true,
			WorldRadius = Radar.RadarRange * 0.5f,
			Radius = Radar.RadarRange * 0.5f,
			DefaultColor = degLineColor,
			Segments = 100,
		};
		AddChild(InnerCircle);
		
		OuterCircle = new()
		{
			ScreenWidth = 1,
			MaintainPixelWidth = true,
			WorldRadius = Radar.RadarRange,
			Radius = Radar.RadarRange,
			DefaultColor = degLineColor,
			Segments = 100,
		};
		AddChild(OuterCircle);
	}

	private void VisualizeRadarReturn(Vector2 point)
	{
		RadarPingHandler.ShowPing(point);
		// PingParticles.GlobalPosition = point;
		// PingParticles.Emitting = true;
		// PingParticles.OneShot = true;
		// // PingParticles.GlobalRotation = GlobalPosition.DirectionTo(point).Angle() + MathF.PI / 2;
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{	
		if(!Radar.Enabled)
		{
			Visible = false;
			return;
		}
		QueueRedraw();
	}

	public override void _Draw()
	{
		var a = globals.CalculateVector(Mathf.DegToRad(Radar.CurrentDir), 50);
		var b = globals.CalculateVector(Mathf.DegToRad(Radar.CurrentDir - 0.25f), Radar.RadarRange);
		var c = globals.CalculateVector(Mathf.DegToRad(Radar.CurrentDir + 0.25f), Radar.RadarRange);
		
		if(Godot.Engine.TimeScale == 0.01f)
		{
			//Draw cone:
			DrawPolygon(new Vector2[]{a, b, c}, new Color[]{Color});
		}
		
		var degLineColor = Color;
		degLineColor.A = degLineColor.A * 0.25f;
		
		for (int i = 0; i <= 360; i+= 30)
		{
			a = globals.CalculateVector(Mathf.DegToRad(i), Radar.RadarRange * 0.5f);
			b = globals.CalculateVector(Mathf.DegToRad(i), Radar.RadarRange);
			DrawLine(a, b, degLineColor);
			// var li = i/30;
			// li = li == 12 ? 0 : li;
			// var label = _labels[li];
			
			// label.Text = i == 360 ? "RADAR" : i.ToString();
			
			// Vector2 labelPosition = globals.CalculateVector(Mathf.DegToRad(i - 90), Radar.RadarRange + label.Size.X/2);
			// Vector2 labelSize = label.GetRect().Size;
			// Vector2 centeredPosition = labelPosition - (labelSize / 2);
   			// label.Position = centeredPosition;
		}
		
		//Scale = Vector2.One * (Radar.RadarRange / 500);
	}
}
