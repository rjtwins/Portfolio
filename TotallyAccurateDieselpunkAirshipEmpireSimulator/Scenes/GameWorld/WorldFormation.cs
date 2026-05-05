using Godot;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

public partial class WorldFormation : Node3D, IGameWorldSelectable, IGameWorldOrderable
{
	public static ObservableCollection<WorldFormation> Active { get; set; } = new();
	public static ObservableCollection<WorldFormation> Selected { get; set; } = new();	
	public string Label { get; set; }
	public List<WorldShip> WorldShips { get; set; } = new();
	public List<Vector3> PositionOffsets { get; set; } = new();	
	public WorldShip Anchor {get; set;}
	public bool UISelected { get; set; } = false;
	
	public Vector3 TargetPosition { get; set; }
	
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
    {
    	this.AddToGroup("UISelectable");
		this.TreeEntered += () => Active.Add(this);
		this.TreeExited += () => Active.Remove(this);
		Active.Add(this);
    }

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
    {
		if (Anchor == null)
			return;
			
		GlobalPosition = Anchor.GlobalPosition;
    }
    
    public void AddShip(WorldShip worldShip)
    {
		WorldShips.Add(worldShip);
		if (Anchor == null)
			Anchor = worldShip;

		PositionOffsets.Add(worldShip.GlobalPosition - Anchor.GlobalPosition);
		worldShip.Formation = this;
    }
    
    public void RemoveShip(WorldShip worldShip)
    {
		var i = WorldShips.IndexOf(worldShip);
		PositionOffsets.RemoveAt(i);
		WorldShips.RemoveAt(i);
        
        if(Anchor == worldShip && WorldShips.Count > 0)
        {
			Anchor = WorldShips.First();
			PositionOffsets.Clear();
			WorldShips.ForEach(x => PositionOffsets.Add(worldShip.GlobalPosition - Anchor.GlobalPosition));
        }

		worldShip.Formation = null;

		if (WorldShips.Count == 0)
			QueueFree();
    }
	
	public void SetMoveToCommand(Vector3 pos)
	{
		Anchor.TargetPosition = pos;
		for (int i = 0; i < WorldShips.Count; i++)
		{
			WorldShips[i].TargetPosition = PositionOffsets[i] + pos;
		}
		TargetPosition = pos;
	}

    public void Usurp(WorldFormation formation)
    {
		formation.WorldShips.ForEach(x => { formation.RemoveShip(x); AddShip(x); });
    }

    public bool IsInSelectionBox(Rect2 box)
    {
		Camera3D camera = GameWorldTest.InMapMode ? OrthogonalCamera3d.Instance: OrbitalCamera.Instance;
		return WorldShips.Any(x => box.HasPoint(camera.UnprojectPosition(x.GlobalPosition)));
    }

    public void Select()
    {
		// GD.Print("Select", this);
		UISelected = true;
		AddToGroup("UISelected");
		Selected.Add(this);
    }

    public void Deselect()
    {
		// GD.Print("Deselect", this);
		UISelected = false;
		RemoveFromGroup("UISelected");
		Selected.Remove(this);
    }

    public void MoveToPosition(Vector3 position)
    {
		SetMoveToCommand(position);
    }

    public void HoldPosition()
    {
		SetMoveToCommand(Anchor.GlobalPosition);
		TargetPosition = Anchor.GlobalPosition;
    }

    public float GetHeight()
    {
		return Anchor.GlobalPosition.Y;
    }

    public void SetTargetObject(Node3D target)
    {
		WorldShips.ForEach(x => x.SetTargetObject(target));
    }
}
