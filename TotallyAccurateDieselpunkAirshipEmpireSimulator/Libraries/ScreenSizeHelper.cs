using Godot;
using System;

public partial class ScreenSizeHelper : Node
{
	[Export] public Node Node {get; set;}
	[Export] public float MinPixelSize {get; set;}
	[Export] public bool KeepScreenSize {get; set;}
	
	
	Control controlNode;
	Node2D node2D;
	Line2D lineNode;
	Vector2? initScale;
	Vector2? initSize;
	Vector2? initMinSize;
	float? initLineWidth;
	
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{		
		if(Node is Node2D n)
			node2D = n;
		
		if(Node is Control c)
			controlNode = c;
			
		if(Node is Line2D l)
			lineNode = l;
			
		initScale = node2D?.Scale ?? controlNode?.Scale ?? lineNode?.Scale;
		initSize = controlNode?.Size;
		initMinSize = controlNode?.CustomMinimumSize;
		initLineWidth = lineNode?.Width;
	}

    public override void _PhysicsProcess(double delta)
    {
		var zoom = GetViewport().GetCamera2D().Zoom.X;
		
		if(KeepScreenSize && initScale != null)
		{
			Node.Set("scale", initScale.Value * 1/zoom);
		}
		else if(MinPixelSize != 0)
		{
			if(initLineWidth != 0)
			{
				var onScreenWidth = initLineWidth * zoom;
				if(onScreenWidth < MinPixelSize)
					Node.Set("width", MinPixelSize/zoom);
				else
					Node.Set("width", initLineWidth.Value);
			}
			else if(initMinSize != null)
			{
				
			}
		}
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta)
	{
		
	}
}
