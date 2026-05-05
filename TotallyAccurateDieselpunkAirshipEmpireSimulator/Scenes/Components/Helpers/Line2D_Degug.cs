using Godot;
using System;

public partial class Line2D_Degug : Line2D
{
	[Export] public float Glow {get;set;}
	public override void _Ready()
	{
		(Material as ShaderMaterial).SetShaderParameter("Glow", Glow);
	}

	public override void _Process(double delta)
	{

	}

	public override void _PhysicsProcess(double delta)
	{
		var zoom = GetViewport().GetCamera2D().Zoom.X;
		var newGlow = Glow * 1/zoom;
		(Material as ShaderMaterial).SetShaderParameter("Glow", newGlow);
	}
}
