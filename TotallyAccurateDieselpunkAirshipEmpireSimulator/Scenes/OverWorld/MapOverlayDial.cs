using Godot;
using System;

public partial class MapOverlayDial : Control
{
	[Export]
	RotationControl RotationControl {get;set;}
	[Export]
	Button SelectedOnlyButton {get; set;}

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		RotationControl.OnRotated += RotationControlRotated;
		SelectedOnlyButton.Toggled += SelectedOnlyToggled;
	}

    private void SelectedOnlyToggled(bool toggledOn)
    {
        GetTree().CallGroup("Fleet", "SelectedOnly", toggledOn);
    }

    //TODO: Hook this up to a group call to show overlays for the fleets/settlements.
    private void RotationControlRotated(float NewRotationDegrees)
	{
		byte overlayMode = 0;
		var r = (int)MathF.Round(NewRotationDegrees);
		switch (r)
		{
			case 45:
			overlayMode = 0;
			break;
			case 90:
			overlayMode = 1;
			break;
			case 135:
			overlayMode = 2;
			break;
			case 180:
			overlayMode = 3;
			break;
			case 225:
			overlayMode = 4;
			break;
			default:
			//How?
			break;
		}
		
		GetTree().CallGroup("Fleet", "OverlayMode", overlayMode);
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		
	}
}
