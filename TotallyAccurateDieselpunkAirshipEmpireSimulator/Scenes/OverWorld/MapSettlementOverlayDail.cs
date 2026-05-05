using System;
using Godot;

public partial class MapSettlementOverlayDail : Control
{
	[Export]
	RotationControl RotationControl {get;set;}
	
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		RotationControl.OnRotated += RotationControlRotated;
	}

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
			default:
			//How?
			break;
		}
		
		GetTree().CallGroup("Settlements", "OverlayMode", overlayMode);
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}
}
