using Godot;
using System;

public partial class EngineTrailCPUParticles : CpuParticles2D
{
	private double _initLifeTime;
	private int _initParticleAmount;
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		_initLifeTime = Lifetime;
		_initParticleAmount = Amount;
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		// double GameSpeed = Engine.TimeScale;
		// var newLifeTime = _initLifeTime * GameSpeed;
		// var newAmount = (int)(_initParticleAmount * GameSpeed);
		
		// if(Lifetime != newLifeTime)
		// 	Lifetime = newLifeTime;
		// if(Amount != newAmount)
		// 	Amount = newAmount;
	}
}
