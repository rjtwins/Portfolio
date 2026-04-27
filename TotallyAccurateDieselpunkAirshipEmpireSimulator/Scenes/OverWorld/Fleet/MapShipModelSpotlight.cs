using Godot;
using System;

public partial class MapShipModelSpotlight : PointLight2D
{
	private float default_energy = 0f;
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
    {
        default_energy = this.Energy;
    }

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
    {
        var ratio = 1 - globals.CalculateLightingLevel();
		//this.Modulate = new Color(this.Modulate.R, Modulate.G, this.Modulate.B, ratio);
		this.Energy = default_energy * ratio;
		Enabled = ratio > 0.25f;
    }
}
