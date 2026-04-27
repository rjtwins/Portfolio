using System;
using Godot;

public partial class TurretComponent : FunctionalComponent
{
	[Export]
	Turret Turret {get; set;}
	public void EngageTarget(Node3D target)
	{
		Turret?.EngageTarget(target);
	}
}
