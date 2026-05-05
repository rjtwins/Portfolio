using Godot;
using System;

public partial class GPUParticlesTrail : GpuParticles2D
{
	double initLifetime = 0d;
	int initAmount = 0;
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
    {
		initLifetime = this.Lifetime;
		initAmount = this.Amount;
    }

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
    {
		// this.Lifetime = initLifetime * Godot.Engine.TimeScale;
		// this.Amount = initAmount * (int)Godot.Engine.TimeScale;
    }
}
