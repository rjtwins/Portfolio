using Godot;
using System;

public partial class DebugClick : ColorRect
{

	[Export] PackedScene ExplosionScene;
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}

	public override void _GuiInput(InputEvent @event)
	{
		base._GuiInput(@event);		
		if(@event is InputEventMouseButton mouseButton && mouseButton.ButtonIndex == MouseButton.Left && mouseButton.IsReleased())
		{
			var explosion = ExplosionScene.Instantiate<Explosion>();
			AddChild(explosion);
		}
	}
}
