using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public partial class DragManager : Node2D
{
	public static DragManager Instance {get; private set;}
	
	[Export]
	public bool SingleSelection {get; set;} = true;
	
	private bool _dragging = false;
	public List<Node2D> Selectable = new();
	public List<Node2D> Selected = new();
	private Vector2 _drag_start = Vector2.Zero;
	
	private Rect2 _select_rect = new(0, 0, 0, 0);
	
	//private RectangleShape2D _select_rect = new();
	
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		Instance = this;
	}
	
	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		//GD.Print(_selected.Count());
	}

	public override void _UnhandledInput(InputEvent @event)
	{	
		if(globals.MapToolMode != MapToolMode.None)
			return;
		
		if(!SingleSelection)
		{
			if(HandleSelect(@event))
				return;
			
			HandleMove(@event);
		}
		
		if(@event is InputEventMouseButton mouseButton && mouseButton.IsReleased() && mouseButton.ButtonIndex == MouseButton.Left)
			Selected.ToList().ForEach(x => UnselectSelectable(x as Selectable));

		base._UnhandledInput(@event);
	}
	
	public void SelectSelectable(Selectable node)
	{
		if(SingleSelection)
			if(Selected.Count() != 1 || !Selected.Contains(node))
				Selected.ToList().ForEach(x => UnselectSelectable(x as Selectable));
			
		if(!Selectable.Contains(node))
			return;
		
		if(globals.MapToolMode != MapToolMode.None)
			return;
		
		Selected.Add(node);
		
		if(node.GetParent() is Fleet f)
			GetTree().CallGroup("UIFleetElements", "FleetSelected", f);
		else if(node.GetParent() is Settlement s)
			GetTree().CallGroup("UISettlementElements", "SettlementSelected", s);
	}

	public void UnselectSelectable(Selectable node)
	{
		Selected.Remove(node);
		
		if(node?.GetParent() == null)
			return;
			
		if(node.GetParent() is Fleet f)
			GetTree().CallGroup("UIFleetElements", "FleetUnselected");
		else if(node.GetParent() is Settlement s)
			GetTree().CallGroup("UISettlementElements", "SettlementUnselected");
	}

	private void HandleMove(InputEvent @event)
	{
		if(!_dragging)
			return;
			
		if(!(@event is InputEventMouseMotion mouseInput))
			return;
			
		QueueRedraw();
	}

	private bool HandleSelect(InputEvent @event)
	{
		if(!(@event is InputEventMouseButton mouseInput))
			return false;
			
		if(mouseInput.ButtonIndex != MouseButton.Left)
			return false;
			
		if(mouseInput.IsPressed() && Selected.Count() == 0)
		{
			StartDrag();
			return true;
		}

		if(mouseInput.IsReleased())
		{
			StopDrag();
			return true;
		}
			
		//GD.Print($"Selected {Selected.Count()}");
		
		return false;
	}
	
	private void StartDrag()
	{
		//Clear
		Selected.Clear();
		_drag_start = GetGlobalMousePosition();
		_dragging = true;
	}
	
	private void StopDrag()
	{
		var drag_end = GetGlobalMousePosition();
		_select_rect = CreateRectFromPoints(_drag_start, drag_end);
		var nodes = Selectable.Where(x => _select_rect.HasPoint(x.GlobalPosition)).OfType<Selectable>().ToList();
		nodes.ForEach(x => SelectSelectable(x));
		
		_dragging = false;
		_drag_start = Vector2.Zero;
		
		QueueRedraw();
	}

	public override void _Draw()
	{
		if(_dragging)
			DrawRect(new Rect2(_drag_start, GetGlobalMousePosition() - _drag_start), new Color("black"), false);
		base._Draw();
	}
	
	private Rect2 CreateRectFromPoints(Vector2 pointA, Vector2 pointB)
	{
		// Calculate the top-left corner
		Vector2 topLeft = new Vector2(Mathf.Min(pointA.X, pointB.X), Mathf.Min(pointA.Y, pointB.Y));
		
		// Calculate the size of the rectangle
		Vector2 size = new Vector2(Mathf.Abs(pointA.X - pointB.X), Mathf.Abs(pointA.Y - pointB.Y));
		
		// Create and return the Rect2
		return new Rect2(topLeft, size);
	}
	
}
