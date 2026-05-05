using Godot;
using System;

public partial class MapModeIndicator : Line2D
{
	[Export] WorldShip worldShip;
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
    {
        GameWorldTest.MapModeChanged += MapModeChanged;
    }

    private void MapModeChanged(bool newValue)
    {
		if(newValue)
		{
			Show();
		}else
		{
			Hide();
		}
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta)
    {
		if(Visible)
		{
			GlobalPosition = OrthogonalCamera3d.Instance.UnprojectPosition(new Vector3(worldShip.GlobalPosition.X, 0, worldShip.GlobalPosition.Z));
		}
    }
}
