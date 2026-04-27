using Godot;
using System;

public partial class MapToolButtons : Control
{
	[Export] Button PenToolButton;
	[Export] Button CircleToolButton;
	[Export] Button AngleToolButton;
	[Export] Button NoneButton;
	
	private ScreenLine2D screenLine2D;
	private CircleLine2D circleLine2D;
	private Vector2 MouseStartPos = Vector2.Zero;
	private bool DrawingInProgress = false;
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		PenToolButton.Pressed += () => globals.MapToolMode = MapToolMode.Pen;
		CircleToolButton.Pressed += () => globals.MapToolMode = MapToolMode.Circle;
		AngleToolButton.Pressed += () => globals.MapToolMode = MapToolMode.Angle;
		NoneButton.Pressed += () => globals.MapToolMode = MapToolMode.None;
		
		PenToolButton.Pressed += () => GetTree().CallGroup("MapToolGroup", "MapToolModeChanged");
		CircleToolButton.Pressed += () => GetTree().CallGroup("MapToolGroup", "MapToolModeChanged");
		AngleToolButton.Pressed += () => GetTree().CallGroup("MapToolGroup", "MapToolModeChanged");
		NoneButton.Pressed += () => GetTree().CallGroup("MapToolGroup", "MapToolModeChanged");
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		
	}
}
