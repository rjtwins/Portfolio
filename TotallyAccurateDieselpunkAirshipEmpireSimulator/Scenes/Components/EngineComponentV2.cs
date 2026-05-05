using Godot;
using System;

public partial class EngineComponentV2 : Component
{	
    [Export] public Node3D TopReference1 { get; set; }
    [Export] public Node3D TopReference2 { get; set; }
    [Export] public Node3D BottomReference1 { get; set; }
    [Export] public Node3D BottomReference2 { get; set; }
    [Export] Node3D Gimble { get; set; }
    
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
    {
        
    }

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
    {
        //CanFunction();
    }

    //Check for obstructions.
    public override bool CanFunction()
    {
        //Top:
        if (GetFromRayCast(TopReference1.GlobalPosition, TopReference2.GlobalPosition, out _, includeAreas: false))
        {
            //GD.Print("Engine cannot function");
            return false;
        }
        
        //Bottom:
        if (GetFromRayCast(BottomReference1.GlobalPosition, BottomReference2.GlobalPosition, out _, includeAreas: false))
        {
            //GD.Print("Engine cannot function");
            return false;
        }
            
        return true;
    }
}
