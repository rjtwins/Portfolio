using System;
using Godot;

public partial class Selectable : Node2D
{	
	// Called when the node enters the scene tree for the first time.
	DragManager _dragManger => DragManager.Instance;
	
	[Export] public bool CanSelect {get; set;} = true;
	public override void _Ready()
	{
		//Get selection manager:
		_dragManger.Selectable.Add(this);
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		
	}
	
	public void Select()
	{
		_dragManger.SelectSelectable(this);
	}
	
	public bool Selected => _dragManger.Selected.Contains(this);
	
	public void _on_mouse_detector_input_event(Node viewport, InputEvent @event, int shape_idx)
	{	
		if(!CanSelect)
			return;
			
		if(@event is InputEventMouseButton mouseButton && mouseButton.ButtonIndex == MouseButton.Left && mouseButton.IsReleased())
		{
			Select();
		}
	}

    internal void UnSelect()
    {
        _dragManger.UnselectSelectable(this);
    }

}
