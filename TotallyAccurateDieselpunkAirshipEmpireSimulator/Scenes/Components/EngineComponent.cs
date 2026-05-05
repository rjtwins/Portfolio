using Godot;
using System;

public partial class EngineComponent : FunctionalComponent
{
	[Export] public float Thrust { get; set; }
	[Export] public float Gimble { get; set; } //Max gimble in rad.
	[Export] public Vector3 DesiredThrustDirection { get; set; }
}
