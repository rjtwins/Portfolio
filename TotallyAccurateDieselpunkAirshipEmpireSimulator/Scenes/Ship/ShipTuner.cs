using Godot;
using System;
using System.Collections.Generic;
using System.IO.Pipes;
using System.Linq;

public partial class ShipTuner : Node
{
    [Export] public ShipPid Pid;
    [Export] public float Mass = 2000f;
    [Export] public float Gravity = 9.81f;
    [Export] public float TargetAltitude = 50f;
    [Export] public float MaxThrust { get; set; }
    [Export] public float MinThrust { get; set; }
    
    
    private float _position;
    private float _velocity;
    
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
    {
        
    }
    
    public void RunSimulation(float mass, float targetAltitude, float min, float max)
    {
		this.Mass = mass;
		this.TargetAltitude = targetAltitude;
		this.MinThrust = min;
		this.MaxThrust = max;
    
        // Range of PID parameters to test
		float[] kpValues = globals.GetRangeWithInterval(0f, 1f, 0.1f).Select(x => (float)x).ToArray();
		float[] kdValues = globals.GetRangeWithInterval(-1f, 0f, 0.1f).Select(x => (float)x).ToArray();
		float[] kiValues = globals.GetRangeWithInterval(0f, 1f, 0.1f).Select(x => (float)x).ToArray();

        Dictionary<(float, float, float), float> results = new();

        foreach (float kp in kpValues)
        foreach (float kd in kdValues)
        foreach (float ki in kiValues)
        {
            float score = Simulate(kp, ki, kd);
            results[(kp, kd, ki)] = score;
        }

        // Find best score (lowest error)
        var best = System.Linq.Enumerable.MinBy(results, kv => kv.Value);
        GD.Print($"Best PID: Kp={best.Key.Item1}, Ki={best.Key.Item3}, Kd={best.Key.Item2}, Score={best.Value}");

		Pid.Kp = best.Key.Item1;
		Pid.Ki = best.Key.Item3;
		Pid.Kd = best.Key.Item2;
		
		Pid.Reset();
    }
    
	private float Simulate(float kp, float ki, float kd)
    {
        // Reset state
        _position = 0.5f * TargetAltitude;
        _velocity = 0f;
        Pid.Kp = kp;
        Pid.Ki = ki;
        Pid.Kd = kd;
        Pid.Reset();
        
        float time = 0f;
        float dt = 0.02f;
        float totalError = 0f;
        float overshootPenalty = 0f;
        float velocityPenalty = 0f;
        
        while (time < 120f) // Simulate
        {
            float correction = Pid.DoPid(_position, TargetAltitude, _velocity, dt, -1, 1);
            correction = Math.Clamp(correction, -1, 1);
            float thrustCorrection = correction * MaxThrust;
            
            // Compute physics
            float acceleration = (thrustCorrection - Mass) / Mass;

            _velocity += acceleration * dt;
            _position += _velocity * dt;

            time += dt;
            
            //Error function
            float positionError = TargetAltitude - _position;

            totalError += time * Mathf.Abs(positionError) * dt;
            velocityPenalty += Mathf.Abs(_velocity) * dt * 100f;
            if (_position > TargetAltitude)
                overshootPenalty += (_position - TargetAltitude) * dt * 500f;
        }
        
        float fitness = totalError + overshootPenalty + velocityPenalty;
        return fitness;
    }
}
