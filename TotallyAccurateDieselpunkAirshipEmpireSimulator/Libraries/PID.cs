using Godot;
using CSPID;
using System;

public partial class PID : Node
{
	IPIDController controller;
	
	[Export] public double Kp = 1;
	[Export] public double Ki = 0;
	[Export] public double Kd = 1;
	[Export] public double N = 1;
	[Export] public Timer Sampler {get; set;}
	
	[Export] public double OutputSignal {get; set;}
	[Export] public double InputSignal {get; set;}
	[Export] public double SetPoint {get; set;}
	[Export] public Label OutputLabel {get; set;}
	
	private TimeSpan sampleRate;
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		controller = new PIDController(new Range<double>(-1, 1), new Range<double>(-1 , 1));
		controller.ProportionalGain = Kp;
		controller.IntegralGain = Ki;
		controller.DerivativeGain = Kp;
		//controller = new PIDController(Kp, Ki, Kd, N, Upper, Lower);
		Sampler.Timeout += Sample;
		sampleRate = TimeSpan.FromSeconds(Sampler.WaitTime);
	}
	
	public void Reset(Range<double>? errorRange = null, Range<double>? controlRange = null)
	{
		errorRange = errorRange == null ? new(-1, 1) : errorRange;
		controlRange = controlRange == null ? new(-1, 1) : controlRange;

		controller = new PIDController(errorRange.Value, controlRange.Value);
		controller.ProportionalGain = Kp;
		controller.IntegralGain = Ki;
		controller.DerivativeGain = Kp;
	}

	public void UpdateParameters()
	{
		controller.ProportionalGain = Kp;
		controller.IntegralGain = Ki;
		controller.DerivativeGain = Kp;
	}

	private void Sample()
	{
		OutputSignal = controller.Next(InputSignal, sampleRate.TotalSeconds);
		//OutputLabel.Text = $"S {Math.Round(SetPoint,1)}, I {Math.Round(InputSignal, 1)}, O {Math.Round(OutputSignal,1)}";
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		UpdateParameters();
	}
}
