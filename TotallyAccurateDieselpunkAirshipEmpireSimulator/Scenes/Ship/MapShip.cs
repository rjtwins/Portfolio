using Godot;
using System;
using System.Linq;

public partial class MapShip : Node
{
	[Export] public ShipData ShipData { get; set; }
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
    {
		this.ProcessMode = ProcessModeEnum.Disabled;
		this.SetPhysicsProcess(false);
    }

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
    {
        
    }
}
