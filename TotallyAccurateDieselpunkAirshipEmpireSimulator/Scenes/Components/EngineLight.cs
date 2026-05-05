using Godot;
using System;

public partial class EngineLight : MeshInstance3D
{
	[Export] public EngineComponent EngineComponent { get; set; }
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
    {
        if(EngineComponent.PowerLevel <= 0)
        {
			this.Visible = false;
			return;
        }
        
        this.Visible = true;
		var cylinder = this.Mesh as CylinderMesh;
		cylinder.Height = EngineComponent.PowerLevel * 5 + 1;
		this.Position = new Vector3(0f, -2.2f - cylinder.Height / 2, 0f);
    }
}
