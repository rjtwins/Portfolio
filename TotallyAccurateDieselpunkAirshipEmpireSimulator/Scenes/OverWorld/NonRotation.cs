using Godot;
using System;

public partial class NonRotation : Node2D
{
	[Export]
	public Node2D Follows {get; set;}
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		Follows = GetParent<Node2D>();
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		if(Follows == null)
			return;
			
		GlobalRotation = 0f;
		GlobalPosition = Follows.GlobalPosition;
	}
}
