using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public partial class UnitSelector : Control
{
	private bool selecting { get; set; } = false;
	Vector2 dragStart;
	Rect2 selectBox;

    public override void _Input(InputEvent e)
    {
		if (GameWorldTest.MouseInUI)
			return;
			
		HandleSelection(e);
		HandleCenteringOnObject(e);
    }
    
    private void HandleCenteringOnObject(InputEvent e)
    {		
        if(!(e is InputEventMouseButton m && m.ButtonIndex == MouseButton.Middle && m.IsReleased()))
        {
			return;
        }
        
		var selectedNodes = GetTree().GetNodesInGroup("UISelected").OfType<Node3D>();
		if(selectedNodes.Count() == 0)
		{
			OrbitalCamera.Instance.TrackObjects(new List<Node3D>());
		}
			
		OrbitalCamera.Instance.TrackObjects(selectedNodes);

    }
    
    private void HandleSelection(InputEvent e)
    {
		if(e is InputEventMouseButton m && m.ButtonIndex == MouseButton.Left)
        {
            if(m.Pressed)
            {
				selecting = true;
				dragStart = m.Position;
            }else
            {
				selecting = false;
				if (dragStart.IsEqualApprox(m.Position))
					selectBox = new Rect2(m.Position, Vector2.Zero);
				UpdateSelected();
				QueueRedraw();
            }
        }else if(selecting && e is InputEventMouseMotion v)
        {
			float xMin = Mathf.Min(dragStart.X, v.Position.X);
			float yMin = Mathf.Min(dragStart.Y, v.Position.Y);

			float xMax = Mathf.Max(dragStart.X, v.Position.X);
			float yMax = Mathf.Max(dragStart.Y, v.Position.Y);
			
			selectBox = new Rect2(xMin, yMin, xMax - xMin, yMax - yMin);
			
			UpdateSelected();
			QueueRedraw();
        }
    }

    private void UpdateSelected()
    {
		GetTree()
			.GetNodesInGroup("UISelectable")
			.OfType<IGameWorldSelectable>()
			.ToList()
			.ForEach(x =>
			{
				if (x.IsInSelectionBox(selectBox))
					x.Select();
				else
					x.Deselect();
			});
    }


    public override void _Draw()
    {
		if (!selecting)
			return;

		DrawRect(selectBox, new Color("#00ff0066"));
		DrawRect(selectBox, new Color("#00ff00"), false, 0.25f);
    }    

	// // Called when the node enters the scene tree for the first time.
	// public override void _Ready()
	// {
	// }

	// // Called every frame. 'delta' is the elapsed time since the previous frame.
	// public override void _Process(double delta)
	// {
	// }
}
