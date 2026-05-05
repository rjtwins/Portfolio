using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using Plane = Godot.Plane;
using Vector3 = Godot.Vector3;

public partial class ShipEditor4 : Node3D
{
	[Export] public Camera3D Camera;
	[Export] public EditorShip EditorShip;
	[Export] public string UID { get; set; }
	[Export] public string CICUUID { get; set; }
	[Export] public SaveLoad SaveLoadPanel { get; set; }
	
	[Export] public GridContainer ComponentGrid { get; set; }
				
	private Component _selectedComponent { get; set; }
	
	private Vector3 _gridSize = new Vector3(0.5f, 0.5f, 0.5f);

	private HashSet<Component> _placedDown { get; set; } = new HashSet<Component>();
	[Export] public Component CIC;

	private List<Vector3> Directions;

	private bool _showLOS { get; set; } = false;
	
	// public void on_create_button_pressed()
	// {
	// 	CreateComponent(UID);
	// }
	
	// public void on_button_slope_pressed()
	// {
	// 	CreateComponent("uid://rtobt2ysuvo");
	// }
	
	// public void on_button_engine_pressed()
	// {
	//     CreateComponent("uid://qmjtvv1mjgqg");
	// }
	
	// public void on_button_armor_pressed()
	// {
	// 	CreateComponent("uid://b5sn32ys36xcs");
	// }
	
	// public void _on_button_lift_pressed()
	// {
	//     CreateComponent("uid://bcai3rwirsrts");
	// }
	
	// public void _on_button_turret_pressed()
	// {
	//     CreateComponent("uid://bby4asrxpurco");
	// }
	
	// public void _on_button_hanger_pressed()
	// {
	// 	CreateComponent("uid://daxtoq104motk");
	// }
	
	// public void _on_button_tac_missile_pressed()
	// {
	// 	CreateComponent("uid://b6ruy5sfg2kjy");
	// }
	
	// public void _on_button_ir_sensor_pressed()
	// {
	//     CreateComponent("uid://rag23wiwyyr8");
	// }
	
	// public void _on_button_turret_2_pressed()
	// {
	//     CreateComponent("uid://bnbjwpf5aycle");
	// }
	
	public void _on_toggle_los_pressed()
	{
		_showLOS = !_showLOS;
		
		_placedDown
		.Where(x => x.HasFOV)
		.ToList()
		.ForEach(x =>
		{
			if (_showLOS)
				x.ShowFOV();
			else
				x.HideFOV();	
		});
	}
	
	public void _on_save_load_pressed()
	{
		SaveLoadPanel.Open();
	}
	
	public void _on_button_save_pressed(string text)
	{
		var connected = this.GetAttachedNodes();
		var states = connected.Select(x => new ComponentBluePrint()
		{
			SceneFilePath = x.SceneFilePath,
			LocalPosition = x.Position,
			LocalRotation = x.Rotation
		}).ToList();

		var shipState = new ShipBlueprint()
		{
			Name = text,
			Description = "",
			Components = states
		};
		
		using var file = FileAccess.Open($"user://ships/{text}.json", FileAccess.ModeFlags.Write);
		file.StoreLine(System.Text.Json.JsonSerializer.Serialize(shipState));
	}
	
	public void _on_button_load_pressed(string text)
	{
		//Clearing editor:
		_placedDown.ToList().Where(x => !x.IsQueuedForDeletion()).ToList().ForEach(x => EditorShip?.RemoveChild(x));
		_placedDown.ToList().Where(x => !x.IsQueuedForDeletion()).ToList().ForEach(x => x?.QueueFree());
		_placedDown.Clear();
		_selectedComponent?.ShowOutline(false);
		_selectedComponent = null;
		
		using var file = FileAccess.Open($"user://ships/{text}.json", FileAccess.ModeFlags.Read);
		var json = file.GetLine();
		var shipState = System.Text.Json.JsonSerializer.Deserialize<ShipBlueprint>(json);

		shipState.Components.ForEach(x =>
		{
			var packed_scene = GD.Load<PackedScene>(x.SceneFilePath);
			var newInstance = packed_scene.Instantiate<Component>();
			EditorShip.AddChild(newInstance);
			_placedDown.Add(newInstance);
			newInstance.Position = x.LocalPosition;
			newInstance.Rotation = x.LocalRotation;
		});
		
		//Find CIC
		CIC = _placedDown.FirstOrDefault();
		if (CIC == null)
			return;
	}
	
	public void _on_button_rotate_ship_pressed()
	{
		EditorShip.Rotate(Vector3.Up, 0.5f * Mathf.Pi);
	}
	
	public void _on_button_flip_ship_pressed()
	{
		EditorShip.Rotate(Vector3.Forward, 0.5f * Mathf.Pi);
	}
	
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
    {
		var up = Vector3.Up;
		var down = Vector3.Down;
		var left = Vector3.Left;
		var right = Vector3.Right;
		var front = Vector3.Forward;
		var back = Vector3.Back;

		Directions = new List<Vector3>()
		{
			up, down, left, right, front, back
		};

		var packed_scene = GD.Load<PackedScene>(CICUUID);
		CIC = packed_scene.Instantiate<Component>();
		EditorShip.AddChild(CIC);
		CIC.GlobalPosition = new(0, 0, 0);
		_placedDown.Add(CIC);

		var componentScenes = globals.LoadAllScenes("res://Scenes/Components");

		componentScenes
			.Select(x => x.Instantiate())
			.OfType<Component>()
			.Select(x => new
			{
				Label = x.Data.Label,
				Desc = x.Data.Description,
				Path = x.SceneFilePath,
			}).ToList().ForEach(x => 
			{
				var button = new Button();
				button.Pressed += () => CreateComponent(x.Path);
				button.Text = x.Label;
				button.CustomMinimumSize = new Vector2(100, 100);
				ComponentGrid.AddChild(button);
			});	
    }

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
    {
		HandleMouseSelection();
		HandleSelectedObjectMovement();
		HandleSelectedObjectRotating();
		HandleDuplication();

		CalcShipStats();
		UpdateUI();
    }

	public float ShipMass = 0f;
	public float ShipLift = 0f;
	public float ShipThrust = 0f;

    private void CalcShipStats()
    {
		ShipThrust = _placedDown
			.Select(x => x.Data)
			.Where(x => x is EngineComponent)
			.Cast<EngineComponent>()
			.Sum(x => x.Thrust);
			
		ShipMass = _placedDown
			.Select(x => x.Data)
			.Cast<ComponentBase>()
			.Sum(x => x.Weight);
			
		ShipLift = _placedDown
			.Select(x => x.Data)
			.Where(x => x is FunctionalComponent)
			.Cast<FunctionalComponent>()
			.Sum(x => x.PassiveLift);
    }

	[Export] public Label MassLabel { get; set; }
	[Export] public Label LiftLabel { get; set; }
	[Export] public Label ThrustLabel { get; set; }

    private void UpdateUI()
    {
		MassLabel.Text = $"{ShipMass} kg";
		LiftLabel.Text = $"{ShipLift} kg";
		ThrustLabel.Text = $"{ShipThrust} kg";
    }
    
    private void HandleDuplication()
    {
		if (_selectedComponent != null)
			return;

		if (!Input.IsActionJustReleased("click"))
			return;

		if (!Input.IsActionPressed("ui_shift"))
			return;
					
		if (!GetMouseCollider(out var collided))
			return;
			
		var packedScene = ResourceLoader.Load<PackedScene>(collided.SceneFilePath);
		var newInstance = packedScene.Instantiate<Component>();
		
		EditorShip.AddChild(newInstance);
		newInstance.GlobalRotation = collided.GlobalRotation;
		_selectedComponent?.ShowOutline(false);
		_selectedComponent = newInstance;
		_selectedComponent.ShowOutline(true);
		_placedDown.Add(newInstance);
    }

	private bool CheckCanPlace(Component component)
	{
		return !component.IsColliding();
	}
    
    // Returns all connected nodes (non-recursive)
    public List<Component> GetAttachedNodes()
    {
		var connected = new List<Component>();
		connected.Add(CIC);
		HashSet<Component> visited = new HashSet<Component>();
		Queue<Component> queue = new Queue<Component>();
		queue.Enqueue(CIC);
		
		while(queue.Count != 0)
		{
			var current = queue.Dequeue();
			if (visited.Any(x => x.GetInstanceId() == current.GetInstanceId()))
				continue;
				
			var neighbors = current.GetPossibleConnectedNeighbors();
			neighbors = neighbors.Where(x => _placedDown.Contains(x)).ToList();

			visited.Add(current);
			connected.AddRange(neighbors);
			neighbors
				.ToList()
				.ForEach(x => queue.Enqueue(x));
		}

		connected = connected.DistinctBy(x => x.GetInstanceId()).ToList();

		return connected;
    }

	private void HandleSelectedObjectMovement()
	{
		if (_selectedComponent == null)
			return;
			
		if(Input.IsActionJustReleased("editor_move_down"))
		{
			_selectedComponent.GlobalPosition += Vector3.Down * 0.5f;
			Input.WarpMouse(Camera.UnprojectPosition(_selectedComponent.GlobalPosition));
			return;
		}
		
		if(Input.IsActionJustReleased("editor_move_up"))
		{
			_selectedComponent.GlobalPosition += Vector3.Up * 0.5f;
			Input.WarpMouse(Camera.UnprojectPosition(_selectedComponent.GlobalPosition));
			return;
		}

		var mousePos = GetViewport().GetMousePosition();
		// Create a ray from the camera through the mouse position
		Vector3 from = Camera.ProjectRayOrigin(mousePos);
		Vector3 dir = Camera.ProjectRayNormal(mousePos);
		Plane plane = new Plane(Vector3.Up, _selectedComponent.GlobalPosition.Y); // y=0 plane
		Vector3 hit = plane.IntersectsRay(from, dir).Value;

		if (!_selectedComponent.IsMovable)
			return;
		
		_selectedComponent.GlobalPosition = hit.Snapped(_gridSize);
    }
    
    private void HandleSelectedObjectRotating()
    {
		if (_selectedComponent == null)
			return;

		var alt_rotate = Input.IsActionPressed("ui_shift");
		
		if (!Input.IsActionJustReleased("rotate"))
			return;
			
		if(!alt_rotate)
		{
			_selectedComponent.RotateY(0.5f * Mathf.Pi);
			return;
		}
		
		_selectedComponent.RotateX(0.5f * Mathf.Pi);
    }

    private void HandleMouseSelection()
    {
		if(Input.IsActionJustReleased("right_click") && _selectedComponent != null)
		{
			_selectedComponent.ShowOutline(false);
			EditorShip.RemoveChild(_selectedComponent);
			_placedDown.Remove(_selectedComponent);
			_selectedComponent.QueueFree();
			_selectedComponent = null;
			
			return;
		}
    
		if(Input.IsActionJustReleased("click") && _selectedComponent != null && CheckCanPlace(_selectedComponent))
		{
			//Duplicate:
			if (Input.IsActionPressed("ui_shift"))
			{
				var packedScene = ResourceLoader.Load<PackedScene>(_selectedComponent.SceneFilePath);
				var newInstance = packedScene.Instantiate<Component>();
				
				EditorShip.AddChild(newInstance);
				newInstance.GlobalRotation = _selectedComponent.GlobalRotation;
				newInstance.GlobalPosition = _selectedComponent.GlobalPosition;
				_placedDown.Add(newInstance);

				//newInstance.IsPlacedDown();
				
			}else
			//Place down:
			{
				_placedDown.Add(_selectedComponent);
				_selectedComponent.ShowOutline(false);
				//_selectedComponent.IsPlacedDown();
				_selectedComponent = null;
			}
			
			return;
		}
    
		if (!Input.IsActionJustReleased("click") || _selectedComponent != null)
			return;
			
		if (Input.IsActionPressed("ui_shift"))
			return;

		if (!GetMouseCollider(out var collided))
			return;

		if (!collided.IsMovable)
			return;
		
		//Pickup
		_selectedComponent = collided;
		_selectedComponent.ShowOutline(true);
		_placedDown.Remove(_selectedComponent);
		
		//GD.Print($"clicked on {collided}");		
    }
    
    private bool GetMouseCollider(out Component collided)
    {
		collided = null;
		var spaceState = GetWorld3D().DirectSpaceState;
		var mousePos = GetViewport().GetMousePosition();
		var from = Camera.ProjectRayOrigin(mousePos);
		var to = from + Camera.ProjectRayNormal(mousePos) * 1000.0f;

		var query = PhysicsRayQueryParameters3D.Create(from, to);
		query.CollideWithBodies = false;
		query.CollideWithAreas = true;
		query.CollisionMask = 2;

		var result = spaceState.IntersectRay(query);

		if (result.Count == 0)
			return false;

		collided = ((Area3D)result["collider"]).GetParent<Component>();
		return true;
    }
    	
	public void CreateComponent(string path)
	{
		var packed_scene = GD.Load<PackedScene>(path);
		Component c = packed_scene.Instantiate<Component>();
		EditorShip.AddChild(c);
		c.GlobalPosition = new(1, 1, 1);
		
		if(_selectedComponent != null)
		{
			EditorShip.RemoveChild(_selectedComponent);
			_selectedComponent.ShowOutline(false);
			_selectedComponent.QueueFree();
			_placedDown.Remove(_selectedComponent);
		}
		
		_selectedComponent = c;
		_selectedComponent.ShowOutline(true);
	}

	public List<string> GetSavedShips()
	{
		return globals.GetFilesInFolder("user://ships");
	}

}
