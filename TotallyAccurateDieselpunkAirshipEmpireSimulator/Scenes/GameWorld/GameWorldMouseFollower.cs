using Godot;
using System;

public partial class GameWorldMouseFollower : Area2D
{
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
    {
		AreaEntered += (e) =>
		{
			if(e.Name.ToString().StartsWith("UIArea"))
			{
				//GD.Print("Mouse entered UI");
				GameWorldTest.MouseInUI = true;
			}
			if(e.Name == "WorldShipSelectionArea")
			{
				//GD.Print("Mouse entered world ship selection area");
				GameWorldTest.UIMouseHoveringOverWorldShip = (e as WorldShipSelectionArea).WorldShip;
			}
		};

		AreaExited += (e) =>
		{
			if(e.Name.ToString().StartsWith("UIArea"))
			{
				GameWorldTest.MouseInUI = false;
			}
			if(e.Name == "WorldShipSelectionArea")
			{
				GameWorldTest.UIMouseHoveringOverWorldShip = null;
			}
		};
    }

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
    {
		GlobalPosition = GetGlobalMousePosition();
    }
}
