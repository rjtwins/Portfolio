using Godot;
using System;

public partial class DebugTargetBall : MeshInstance3D
{
	[Export] public Label3D DebugTextLabel3D { get; set; }
	
	public void SetDebugText(string text)
	{
		DebugTextLabel3D.Text = text;
	}
}
