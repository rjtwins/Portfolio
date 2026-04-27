using Godot;
using System;
using System.Runtime.Serialization;

public partial class Explosion : Node2D
{
	[Export] GpuParticles2D[] Particles;
	[Export] GpuParticles2D Debris;
	[Export] PackedScene ShockwaveScene;
	private RandomNumberGenerator rng = new();
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		Explode();
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}
	
	public void Explode()
	{		
		rng.Randomize();
		var index = rng.RandiRange(0, Particles.Length - 1);
		var template = Particles[index];
		var particle = template.Duplicate() as GpuParticles2D;
		var debris = Debris.Duplicate() as GpuParticles2D;
		particle.Visible = true;
		debris.Visible = true;
		debris.OneShot = true;
		particle.OneShot = true;
		//AddChild(debris);
		AddChild(particle);
		particle.GlobalPosition = GetGlobalMousePosition();
		debris.GlobalPosition = GetGlobalMousePosition();
		particle.Finished += () => QueueFree();
		debris.Finished += () => debris.QueueFree();
		particle.Emitting = true;
		
		//Shockwave
		// var sw = ShockwaveScene.Instantiate();
		// AddChild(sw);
		
		GetTree().CreateTimer(0.75).Timeout += () => debris.Emitting = true;
	}
}
