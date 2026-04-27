using Godot;
using System;

public partial class ShipPid : Node
{
	// Called when the node enters the scene tree for the first time.
	[Export] public float Kp = 100f; // Proportional gain
    [Export] public float Kd = 10f; // Derivative gain
	[Export] public float Ki = 0.1f; // Integral gain

	[Export] public float Target = 0f;
	[Export] public float Current = 0f;
	[Export] public float Velocity = 0f;

	[Export] public float Error = 0f;

	[Export] public float ProportionalComponent = 0f;
	[Export] public float DerivativeComponent = 0f;
	[Export] public float IntegralComponent = 0f;
	[Export] public float Correction = 0f;
		
    private float _integral;	
    public float DoPid(float current, float target, float velocity, double delta, float min, float max)
    {
        float error = target - current;
        _integral += error * (float)delta;

		//Saturate:
		_integral = Math.Clamp(_integral, min, max);
        
        float correction = (Kp * error) + (Ki * _integral) + (Kd * velocity);

		this.Current = current;
		this.Target = target;
		this.Velocity = velocity;
		this.Error = error;
		
		this.DerivativeComponent = (Kd * velocity);
		this.ProportionalComponent = (Kp * error);
		this.IntegralComponent = (Ki * _integral);
		this.Correction = correction;
        return correction;
    }
    
	public void Reset()
    {
        _integral = Ki;
    }
}
