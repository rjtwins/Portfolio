// using Godot;
// using System;
// using System.Collections.Generic;
// using System.Linq;

// public partial class ShipEditor3 : Node2D
// {
// 	public static ShipEditor3 Instance {get; private set;}
// 	public IComponent currentlyHeldNode;
// 	[Export] Button SaveButton;
// 	[Export] Button LoadButton;
// 	[Export] public Ship EditorShip;
// 	[Export] public Node2D ComponentNodesNode;
	
// 	[Export] Label WeightLabel;
// 	[Export] Label DownThrustLabel;
// 	[Export] Label DownTTWLabel;
	
// 	[Export] PackedScene BridgeScene;

// 	//private bool shouldUpdateSides;
// 	private List<IComponent> placedNodes = new();
	
// 	private List<PackedScene> actionHistory = new();
// 	private int actionHistoryIndex = 0;
	
// 	// Called when the node enters the scene tree for the first time.
// 	public override void _Ready()
// 	{
// 		// Instance = this;
// 		// EditorShip.InEditor = true;

// 		// SetupEmpty();
		
// 		// SaveButton.Pressed += Save;
// 		// LoadButton.Pressed += Load;
// 	}
	
// 	public void SetupEmpty()
// 	{
// 		var center = (GetViewport()
// 			.GetVisibleRect()
// 			.Size * 0.5f)
// 			.Snapped(Vector2.One * 45);
		
// 		var component = BridgeScene.Instantiate<ComponentRoot2>();
// 		var editorComponent = (IComponent)component;
// 		editorComponent.InEditor = true;
// 		editorComponent.InEditorOnMouse = false;
// 		EditorShip.AddChild(component);
// 		component.Owner = EditorShip;
// 		placedNodes.Add(component);
// 		component.Position = Vector2.Zero;
// 		//EditorShip.GlobalPosition = Vector2.Zero;
// 	}
	
// 	public override void _Process(double delta)
// 	{			
// 		HandleNodeManipulation();
// 		UpdateNodes();
// 		UpdateLabels();
// 	}
	
// 	private void UpdateLabels(){
// 		// WeightLabel.Text = $"{(EditorShip.Mass / 1000)} t";
// 		// var downEngines = EditorShip
// 		// 	.Components
// 		// 	.OfType<IComponent>()
// 		// 	.Where(x => x.IsFacingDown())
// 		// 	.Select(x => x.GetComponent())
// 		// 	.OfType<EngineComponent>()
// 		// 	.Where(x => x.Thrust > 0)
// 		// 	.ToList();
			
// 		// var downThrust = downEngines.Sum(x => x.Thrust);
		
// 		// DownThrustLabel. Text = $"{downThrust/1000000} MN";
// 		// DownTTWLabel.Text = $"{downThrust / (EditorShip.Mass * 9.81)}";
// 	}
	
// 	private void HandleNodeManipulation()
// 	{
// 		if(Input.IsActionJustReleased("shift_click", true) && currentlyHeldNode != null && currentlyHeldNode.CanPlaceDown())
// 		{
// 			var snappedMousePos = currentlyHeldNode.GlobalPosition;
// 			GetTree().CreateTimer(0.05f).Timeout += () =>
// 			{
// 				var node = currentlyHeldNode as Node2D;
// 				node.GetParent().RemoveChild(node);
				
// 				currentlyHeldNode.InEditorOnMouse = false;
// 				node.GlobalPosition = snappedMousePos;
// 				placedNodes.Add(currentlyHeldNode);
// 				EditorShip.AddComponent(node);
// 				currentlyHeldNode.PlacedDown();
				
// 				var copy = currentlyHeldNode.GetScene().Instantiate<Node2D>();
// 				copy.Owner = null;
// 				ComponentNodesNode.AddChild(copy);
// 				currentlyHeldNode = null;
// 				currentlyHeldNode = copy as IComponent;
// 				currentlyHeldNode.ZLevel = 0;
// 				currentlyHeldNode.InEditor = true;
// 				currentlyHeldNode.InEditorOnMouse = true;

// 				GD.Print("Putting down node and keeping duplicate");
				
// 				UpdateActionHistory();
// 			};
			
// 			return;
// 		};
		
// 		if(Input.IsActionJustReleased("click") && currentlyHeldNode != null && currentlyHeldNode.CanPlaceDown())
// 		{
// 			var snappedMousePos = currentlyHeldNode.GlobalPosition;
// 			GetTree().CreateTimer(0.05f).Timeout += () => 
// 			{
// 				var node = currentlyHeldNode as Node2D;
// 				node.GetParent().RemoveChild(node);
// 				currentlyHeldNode.InEditorOnMouse = false;
// 				placedNodes.Add(currentlyHeldNode);
// 				EditorShip.AddComponent(currentlyHeldNode as Node2D);
// 				currentlyHeldNode.GlobalPosition = snappedMousePos;
// 				currentlyHeldNode.PlacedDown();
				
// 				currentlyHeldNode = null;
				
// 				GD.Print("Putting down node");
				
// 				UpdateActionHistory();
// 			};
			
// 			return;
// 		}
		
// 		if(Input.IsActionJustReleased("click", true) && currentlyHeldNode == null)
// 		{
// 			var snappedMousePos = GetGlobalMousePosition().Snapped(Vector2.One * 22.5f);
// 			var possibleNode = placedNodes.FirstOrDefault(x => x.GlobalPosition == snappedMousePos);
			
// 			GetTree().CreateTimer(0.05f).Timeout += () => 
// 			{
// 				if(possibleNode == null)
// 					return;

// 				possibleNode.InEditorOnMouse = true;
// 				currentlyHeldNode = possibleNode;
// 				currentlyHeldNode.PickedUp();
				
// 				(currentlyHeldNode as Node2D).Owner = null;
// 				placedNodes.Remove(currentlyHeldNode);
// 				EditorShip.RemoveChild(currentlyHeldNode as Node2D);
// 				ComponentNodesNode.AddChild(currentlyHeldNode as Node2D);
// 				//UpdateNodes();
				
// 				GD.Print("Picking node up");
				
// 				UpdateActionHistory();
// 			};
			
// 			return;
// 		}
		
// 		if(Input.IsActionJustReleased("right_click", true))
// 		{
// 			var toDelete = currentlyHeldNode;
// 			if(currentlyHeldNode == null)
// 			{
// 				var snappedMousePos = GetGlobalMousePosition().Snapped(Vector2.One * 22.5f);
// 				var possibleNode = placedNodes.FirstOrDefault(x => x.GlobalPosition == snappedMousePos);
				
// 				if(possibleNode == null)
// 					return;
				
// 				currentlyHeldNode = possibleNode;
// 				currentlyHeldNode.PickedUp();
// 				(currentlyHeldNode as Node2D).Owner = null;
// 				placedNodes.Remove(currentlyHeldNode);
// 				EditorShip.RemoveChild(currentlyHeldNode as Node2D);
// 				ComponentNodesNode.AddChild(currentlyHeldNode as Node2D);
// 				toDelete = possibleNode;
// 			}

// 			//We can never delete the bridge.
// 			if(toDelete is ComponentRoot2 cr && cr.GetComponent() is BridgeComponent)
// 				return;
			
// 			currentlyHeldNode = null;
			
// 			GetTree().CreateTimer(0.05f).Timeout += () => 
// 			{
// 				(toDelete as Node2D).GetParent().RemoveChild(toDelete as Node2D);
// 				(toDelete as Node2D).QueueFree();
// 				toDelete = null;
				
// 				GD.Print("Deleting node on mouse");
				
// 				UpdateActionHistory();
// 			};
			
// 			return;
// 		}
		
// 		if(Input.IsActionJustReleased("rotate", true) && currentlyHeldNode != null)
// 		{
// 			var tween = (currentlyHeldNode as Node2D).CreateTween();
// 			tween.TweenProperty(currentlyHeldNode as Node2D, "rotation", MathF.PI/2, 0.01).AsRelative();
// 			var localNode = currentlyHeldNode as Node2D;
// 			tween.Finished += () => localNode.Rotation = localNode.Rotation % (MathF.PI * 2);
			
// 			//currentlyHeldNode.Rotate(MathF.PI/2);
// 			GD.Print($"Rotating node on mouse sideways {Mathf.RadToDeg((currentlyHeldNode as Node2D).Rotation)}");
			
// 			return;
// 		}
		
// 		if(Input.IsActionJustReleased("shift_rotate", true) && currentlyHeldNode != null && currentlyHeldNode.CanRotateDown())
// 		{
// 			currentlyHeldNode.RotatedDown = !currentlyHeldNode.RotatedDown;
// 			GD.Print("Rotating node under mouse down");
// 			return;
// 		}
	
// 		if(Input.IsActionJustPressed("editor_z_up"))
// 		{
// 			var node = (ComponentRoot2)GetUnderMouse();
// 			if(node != null && node.ForceZLevel)
// 				node.ForcedZLevel += 1;
				
// 			UpdateActionHistory();
			
// 			return;
// 		}
		
// 		if(Input.IsActionJustPressed("editor_z_down"))
// 		{
// 			var node = GetUnderMouse() as ComponentRoot2;
// 			if(node != null && node.ForceZLevel)
// 				node.ForcedZLevel -= 1;
				
// 			UpdateActionHistory();
			
// 			return;
// 		}
		
// 		if(Input.IsActionJustPressed("editor_toggle_force_z"))
// 		{
// 			var node = (ComponentRoot2)GetUnderMouse();
// 			if(node != null)
// 				node.ForceZLevel = !node.ForceZLevel;
				
// 			UpdateActionHistory();
			
// 			return;
// 		}
		
// 		if(Input.IsActionJustReleased("editor_quick_save", true))
// 		{
// 			Save(true);
// 		}
		
// 		if(Input.IsActionJustReleased("editor_quick_load", true))
// 		{
// 			Load(EditorShip.ShipName);
// 		}
	
// 		if(Input.IsActionJustReleased("editor_undo", true))
// 		{
// 			Undo();
// 		}
		
// 		if(Input.IsActionJustReleased("editor_redo", true))
// 		{
// 			Redo();
// 		}
// 	}
	
// 	private IComponent GetUnderMouse()
// 	{
		
// 		return placedNodes.FirstOrDefault(x => x.GlobalPosition == GetGlobalMousePosition().Snapped(Vector2.One * 45));
// 	}
		
// 	private void UpdateNodes()
// 	{		
// 		placedNodes.OfType<ComponentRoot2>().OrderBy(x => x.ZLevel).ToList().ForEach(x => x.UpdateSides());
// 	}
	
// 	public bool IsDiagSnapped(ComponentRoot2 component, int zLevel)
// 	{
// 		var coordinate = component.GlobalPosition.Snapped(Vector2.One * 45);
		
// 		return placedNodes.Where(x => 
// 		{
// 			return
// 				x.ZLevel >= zLevel && (
// 					x.GlobalPosition == coordinate + Vector2.One * 90 ||
// 					x.GlobalPosition == coordinate - Vector2.One * 90 ||
// 					x.GlobalPosition == coordinate + new Vector2(90, -90) ||
// 					x.GlobalPosition == coordinate + new Vector2(-90, 90)
// 				);
// 		}).Count() == 4;
// 	}
	
// 	public bool IsSideSnapped(ComponentRoot2 component, int zLevel)
// 	{
// 		var coordinate = component.GlobalPosition.Snapped(Vector2.One * 45);

// 		return placedNodes.Where(x => 
// 		{
// 			return
// 				x.ZLevel >= zLevel && (
// 					x.GlobalPosition == coordinate + new Vector2(0, 90) ||
// 					x.GlobalPosition == coordinate + new Vector2(90, 0) ||
// 					x.GlobalPosition == coordinate + new Vector2(0, -90) ||
// 					x.GlobalPosition == coordinate + new Vector2(-90, 0)
// 				);
// 		}).Count() == 4;
// 	}
	
// 	public bool IsFullySnapped(ComponentRoot2 component, int zLevel)
// 	{
// 		if(!IsDiagSnapped(component, zLevel))
// 			return false;
		
// 		if(!IsSideSnapped(component, zLevel))
// 			return false;
			
// 		return true;
// 	}

// 	private PackedScene GetShipPacked(bool setSaved = false)
// 	{
// 		var scene = new PackedScene();
// 		var components = EditorShip.Components;
		
// 		if(setSaved)
// 		{
// 			components.OfType<ComponentRoot2>()
// 				.Where(x => !x.IsConnectedToBridge())
// 				.ToList()
// 				.ForEach(x => {
// 					x.GetParent().RemoveChild(x);
// 					x.QueueFree();
// 				});
			
// 			//Loose sub components:
// 			components.OfType<SubComponentRoot>()
// 				.Where(x => !x.IsMounted)
// 				.ToList()
// 				.ForEach(x => 
// 				{
// 					x.GetParent().RemoveChild(x);
// 					x.QueueFree();
// 				});
			
// 			placedNodes.RemoveAll(x => ((Node)x).IsQueuedForDeletion());
// 		}
		
// 		EditorShip.SetOwnerOffAllDescendants();
// 		EditorShip.NewShip = !setSaved;
// 		scene.Pack(EditorShip);
		
// 		return scene;
// 	}

// 	public void Save(bool o = false, string name = null, string customPath = "")
// 	{		
// 		var scene = GetShipPacked(true);
// 		var shipName = name != null ? name : EditorShip.ShipName;
		
// 		var path = customPath;
// 		if(string.IsNullOrEmpty(customPath))
// 			path = $"user://ships/{shipName}_ship.tscn";
		
// 		if(!string.IsNullOrEmpty(customPath) && o)
// 		{
// 			var dir = DirAccess.Open("user://ships");
// 			if(dir.FileExists($"{shipName}_ship.tscn"))
// 			{
// 				dir.Remove($"{shipName}_ship.tscn");
// 			}
// 		}
	
// 		var result = ResourceSaver.Save(scene, path);
		
// 		GD.Print(result);
// 	}
	
// 	public void Load(string shipName, string customPath = "")
// 	{
// 		var path = customPath;
// 		if(string.IsNullOrEmpty(customPath))
// 			path = $"user://ships/{shipName}_ship.tscn";
		
// 		var scene = ResourceLoader.Load<PackedScene>(path);
// 		Load(scene);
// 	}
	
// 	private void Load(PackedScene shipScene)
// 	{
// 		//Clear editor:
// 		RemoveChild(EditorShip);
// 		EditorShip.QueueFree();
// 		placedNodes.Clear();
		
// 		//Load ship:
// 		EditorShip = shipScene.Instantiate<Ship>();
// 		EditorShip.InEditor = true;
		
// 		AddChild(EditorShip);
// 		EditorShip.Owner = this;
// 		EditorShip.Show();
		
// 		var loadedComponents = EditorShip.GetChildren().OfType<IComponent>().ToList();
		
// 		GD.Print(loadedComponents.Count());
		
// 		loadedComponents.ForEach(x => x.InEditor = true);
// 		loadedComponents.ForEach(x => x.OnShipLoaded());
// 		placedNodes.AddRange(loadedComponents);
// 	}

// 	internal void DeleteSavedShip(string shipName)
// 	{
// 		var dir = DirAccess.Open("user://ships");
// 		if(dir.FileExists($"{shipName}_ship.tscn"))
// 			dir.Remove($"{shipName}_ship.tscn");
// 	}

// 	private void UpdateActionHistory()
// 	{
// 		var updated = GetShipPacked();
		
// 		if(actionHistory.Count() > actionHistoryIndex + 1)
// 			actionHistory.RemoveRange(actionHistoryIndex, actionHistory.Count() - 1 - actionHistoryIndex);
			
// 		actionHistory.Add(updated);
// 		actionHistoryIndex = actionHistory.Count - 1;
// 	}
	
// 	private void Undo()
// 	{
// 		actionHistoryIndex = actionHistoryIndex - 1;
// 		actionHistoryIndex = Math.Max(0, actionHistoryIndex);
// 		var toLoad = actionHistory[actionHistoryIndex];
// 		Load(toLoad);
// 	}
	
// 	private void Redo()
// 	{
// 		actionHistoryIndex = actionHistoryIndex + 1;
// 		actionHistoryIndex = Math.Min(actionHistory.Count - 1, actionHistoryIndex);
// 		var toLoad = actionHistory[actionHistoryIndex];
// 		Load(toLoad);
// 	}
// }
