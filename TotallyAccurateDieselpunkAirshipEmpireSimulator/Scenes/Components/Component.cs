using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public partial class Component : Node3D
{
	[Export] public string ComponentType = "Block";
	[Export] public bool IsMovable = true;
	[Export] public bool IsRotatable = true;
	[Export] public bool HasFOV = false;
	[Export] public Area3D CollisionDetector { get; set; }
	[Export] public Area3D ConnectionDetector { get; set; }
	[Export] public bool Highlighted { get; set; } = false;
	[Export] public Node3D Outline { get; set; }
	[Export] public ComponentBase Data { get; set; }
	
	[Export] public FovChecker3D? FovChecker3D { get; set; }

	// private readonly List<Vector3> Directions = new List<Vector3>() { Vector3.Up, Vector3.Down, Vector3.Left, Vector3.Right, Vector3.Forward, Vector3.Back };

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
    {
		if(Outline != null)
			Outline.Visible = Highlighted;
		
		//We are just data
		if(GetParent() is MapShip)
		{
			this.GlobalPosition = Vector3.One * 1000000;
			this.Visible = false;
			this.CollisionDetector.Monitorable = false;
			this.CollisionDetector.Monitoring = false;
			this.ConnectionDetector.Monitorable = false;
			this.ConnectionDetector.Monitoring = false;
			
			this.ProcessMode = ProcessModeEnum.Disabled;
		}
    }

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
    {
        
    }
    
    public List<Component> GetPossibleConnectedNeighbors()
    {
		return ConnectionDetector.GetOverlappingAreas().Select(x => x.GetParent<Component>()).ToList();
    }
    
    public bool GetFromRayCast(Vector3 from, Vector3 to, out Component component, List<Rid> excluded = null, bool includeAreas = true)
    {
		var spaceState = GetWorld3D().DirectSpaceState;
        var query = PhysicsRayQueryParameters3D.Create(from, to);
		query.CollideWithAreas = includeAreas;
		query.CollideWithBodies = true;
		
		if(excluded != null)
			query.Exclude.AddRange(excluded);
		
        var result = spaceState.IntersectRay(query);
        

		if (result.Count == 0)
		{
			component = null;
			return false;
		}
		
		try
		{
			//GD.Print("Hit object: ", result["collider"]);
			component = (Component)result["collider"];
			return true;
		}catch(InvalidCastException)
		{
			component = null;
			return false;
		}
    }
    
    public virtual bool CanFunction()
    {
		return true;
    }
    
    public virtual bool IsColliding()
    {
		return CollisionDetector.GetOverlappingAreas().Count != 0;
    }
    
    public void ShowOutline(bool state)
    {
		this.Highlighted = state;

		if (this.Outline == null)
			return;
		
		if (Highlighted)
			this.Outline.Visible = true;
		else
			this.Outline.Visible = false;
    }

	public void ShowFOV()
	{
		if (FovChecker3D == null)
			return;

		FovChecker3D.CheckAndDrawDebugLines();
	}
	
	public void HideFOV()
	{
		if (FovChecker3D == null)
			return;

		FovChecker3D.ClearFOVLines();
	}

	//TODO: Add damage type:
    internal void TakeDamage(float amount)
    {
		//GD.Print($"{Data.Label} has taken {amount} damage");
		Data.Health -= (int)Math.Floor(amount);
		if(Data.Health <= 0)
        {
			//GD.Print($"{Data.Label} was destroyed");
			QueueFree();
        }
    }
}
