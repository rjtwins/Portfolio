using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public partial class UnitCommander : Control
{
	private static UnitCommander _instance;
	[Export] public PackedScene FormationScene { get; set; }
	
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
    {
		_instance = this; ;
    }

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
    {
        if(Input.IsActionJustReleased("ui_create_formation"))
        {
			CreateFormation();
        }
    }
    
    public override void _Input(InputEvent e)
    {
		if (GameWorldTest.MouseInUI)
			return;

		if (GameWorldTest.WillOpenContextMenu)
			return;
			
		if (!(e is InputEventMouseButton m))
			return;

		if (m.ButtonIndex != MouseButton.Right)
			return;

		var tree = GetTree();
		var orderableSelected = tree.GetNodesInGroup("UISelected").OfType<IGameWorldOrderable>();

		if (!orderableSelected.Any())
			return;

		var screenPos = m.Position;
		var height = orderableSelected.Average(x => x.GetHeight());

		var plane = new Plane(Vector3.Up, height);
		var camera = GetViewport().GetCamera3D();

		var point = plane.IntersectsRay(camera.ProjectRayOrigin(screenPos), camera.ProjectRayNormal(screenPos));
		
		if (!point.HasValue)
			return;

		orderableSelected.ToList().ForEach(x => x.MoveToPosition(point.Value));
    }
    
    public static void JoinFormation(WorldShip ship, WorldFormation formation)
    {
		if (ship.GetFormation(out var shipFormation))
		{
			formation.Usurp(shipFormation);
		}
		else
		{
			formation.AddShip(ship);
		}
    }
    
    public static void LeaveFormation(WorldShip ship)
    {
		if (!ship.GetFormation(out var shipFormation))
			return;
		shipFormation.RemoveShip(ship);
    }
    
    public void CreateFormation()
    {
		var tree = GetTree();
		int count = tree.GetNodeCountInGroup("UISelected");
		if (count == 0)
			return;

		var nodes = tree.GetNodesInGroup("UISelected");
		var ships = nodes.OfType<WorldShip>();
		var formations = nodes.OfType<WorldFormation>();

		ships.ToList().ForEach(x => x.Deselect());
		formations.ToList().ForEach(x => x.Deselect());

		ships = ships.Where(x => !x.InFormation);

		if (ships.Count() == 0 && formations.Count() == 0)
			return;
		
		if(formations.Any())
		{
			ships.ToList().ForEach(x => formations.First().AddShip(x));
		}else
		{
			var newFormation = FormationScene.Instantiate<WorldFormation>();
			tree.Root.AddChild(newFormation);
			ships.ToList().ForEach(x => newFormation.AddShip(x));
		}
		if(formations.Count() > 1)
		{
			formations.Skip(1).ToList().ForEach(x => formations.First().Usurp(x));
			var finalFormation = formations.First();
			formations = new List<WorldFormation>() { finalFormation };
		}
		
		// if(formations.Any())
		// {
		// 	formations.ToList().ForEach(x => x.Select());
		// 	return;
		// }
    }
    
    private void FormFormationInternal(WorldShip currentUIShip, WorldShip target)
    {
		var newFormation = FormationScene.Instantiate<WorldFormation>();
		GetTree().Root.AddChild(newFormation);
		newFormation.AddShip(currentUIShip);
		newFormation.AddShip(target);
    }

    public static void FormFormation(WorldShip currentUIShip, WorldShip target)
    {
		_instance.FormFormationInternal(currentUIShip, target);
    }

}
