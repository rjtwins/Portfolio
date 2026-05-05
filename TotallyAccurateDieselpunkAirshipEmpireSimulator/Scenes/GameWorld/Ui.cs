using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Drawing;
using System.Linq;
using Godot;

public partial class Ui : Control
{
	[Export] public Node ShipContainer { get; set; }
	[Export] public PackedScene FormationScene { get; set; }

	[Export] public WorldFormation CurrentUIFormation
	{
		get => _currentUIFormation;
		set
		{
			_currentUIFormation = value;
			UIFormationChanged();
		}
	}
	private WorldFormation _currentUIFormation;
	[Export] public WorldShip CurrentUIShip
	{
		get => _currentUIShip;
		set
		{
			//GD.Print("Set Current UI Ship", value);
			_currentUIShip = value;
			UIShipChanged();
		}
	}
	private WorldShip _currentUIShip;


	[ExportCategory("UI Elements")]
	[Export] public Label FormationLabel { get; set; }
	[Export] public Label ShipLabel { get; set; }
	[Export] public Button NextShipButton { get; set; }
	[Export] public Button PrevShipButton { get; set; }

	[Export] public RichTextLabel ShipInfoLabel { get; set; }
	[Export] public OptionButton MovementBehaviorOptionButton { get; set; }
	[Export] public CheckButton BroadsideCheckButton { get; set; }

	[Export] public Line2D UISelectionLine { get; set; }
	[Export] public Marker2D UISelectionLineToMarker { get; set; }

	ContextMenu friendlyContextMenu;
	[Export] public WorldShip friendlyContextMenuWorldShip { get; set; }
	ContextMenu enemyContextMenu;
	[Export] public WorldShip enemyContextMenuWorldShip { get; set; }

	private List<Control> dynamicShipUIControls { get; set; } = new();

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		WorldShip.Active.CollectionChanged += ActiveWorldShipsChanged;
		WorldFormation.Active.CollectionChanged += ActiveWorldFormationsChanged;
		
		WorldShip.Selected.CollectionChanged += SelectedWorldShipsChanged;
		WorldFormation.Selected.CollectionChanged += SelectedWorldFormationsChanged;
	
		NextShipButton.Pressed += () =>
		{
			var selectedShips = GetCurrentlySelected<WorldShip>();
			if (CurrentUIFormation != null)
				selectedShips = CurrentUIFormation.WorldShips;
			var count = selectedShips.Count();
			if (count < 2)
				return;

			var index = selectedShips.IndexOf(CurrentUIShip);
			index = (index + 1) % count;

			CurrentUIShip = selectedShips[index];
		};

		PrevShipButton.Pressed += () =>
		{
			var selectedShips = GetCurrentlySelected<WorldShip>();

			if (CurrentUIFormation != null)
				selectedShips = CurrentUIFormation.WorldShips;

			var count = selectedShips.Count();
			if (count < 2)
				return;

			var index = selectedShips.IndexOf(CurrentUIShip);
			index = (index - 1) % count;

			CurrentUIShip = selectedShips[index];
		};

		MovementBehaviorOptionButton.ItemSelected += (e) =>
		{
			CurrentUIShip.DirectionBehavior = (WorldShipDirectionBehavior)e;
		};

		BroadsideCheckButton.Pressed += () =>
		{
			var broadside = BroadsideCheckButton.ButtonPressed;
			CurrentUIShip.Broadside = broadside;
		};

		SetupContextMenus();
	}

    private void SelectedWorldFormationsChanged(object sender, NotifyCollectionChangedEventArgs e)
    {
		var last = GetTree().GetNodesInGroup("UISelected").OfType<WorldFormation>().LastOrDefault();
		CurrentUIFormation = last;
		//CurrentUIShip = last != null ? CurrentUIFormation.Anchor : CurrentUIShip;
    }


    private void SelectedWorldShipsChanged(object sender, NotifyCollectionChangedEventArgs e)
    {
		var last = GetTree().GetNodesInGroup("UISelected").OfType<WorldShip>().LastOrDefault();
		if(last?.GetFormation(out var lastsFormation) ?? false)
		{
			lastsFormation.Select();
		    CurrentUIFormation = lastsFormation;
		}else
		{
			CurrentUIShip = last;
		}
    }


    private void ActiveWorldFormationsChanged(object sender, NotifyCollectionChangedEventArgs e)
    {
        
    }

    private void ActiveWorldShipsChanged(object sender, NotifyCollectionChangedEventArgs e)
    {
        
    }

    private void SetupContextMenus()
	{
		//When clicked on friendly ship:
		friendlyContextMenu = new ContextMenu();
		friendlyContextMenu.attach_to(this);
		friendlyContextMenu.connect_to_conditional(this, () => 
		{
			return GameWorldTest.UIMouseHoveringOverWorldShip?.ShipData?.Faction == Faction.PLAYER;
		});
		
		friendlyContextMenu.set_minimum_size(new Vector2I(400, 0));
		friendlyContextMenu.add_item("Join formation", new Callable(this, nameof(FriendlyContextMenuJoinFormationClicked)));
		friendlyContextMenu.add_item("Hold Fire", new Callable(this, nameof(FriendlyContextMenuHoldFireClicked)));
		
		friendlyContextMenu.AboutToOpen += () =>
		{
			//GD.Print($"friendlyContextMenu about to open");
			friendlyContextMenuWorldShip = GameWorldTest.UIMouseHoveringOverWorldShip;
			friendlyContextMenu.set_item_disabled(0, CurrentUIShip == null || CurrentUIShip == friendlyContextMenuWorldShip);
		};

		// friendlyContextMenu.AboutToClose += () =>
		// {
		// 	GD.Print($"friendlyContextMenu about to close");
		// 	friendlyContextMenuWorldShip = null;
		// };

		//When clicked on hostile ship:
		enemyContextMenu = new ContextMenu();
		enemyContextMenu.attach_to(this);
		enemyContextMenu.connect_to_conditional(this, () => 
		{
			return GameWorldTest.UIMouseHoveringOverWorldShip?.ShipData?.Faction == Faction.ENEMY;
		});
		enemyContextMenu.set_minimum_size(new Vector2I(400, 0));
		enemyContextMenu.add_item("Attack", new Callable(this, nameof(EnemyContextMenuAttackClicked)));
		enemyContextMenu.AboutToOpen += () =>
		{
			enemyContextMenuWorldShip = GameWorldTest.UIMouseHoveringOverWorldShip;
			enemyContextMenu.set_item_disabled(0, CurrentUIShip == null);
		};
		
		// enemyContextMenu.AboutToClose += () =>
		// {
		// 	enemyContextMenuWorldShip = null;
		// };
	}
	
	private void FriendlyContextMenuHoldFireClicked()
	{
		IGameWorldOrderable from = CurrentUIFormation != null ? CurrentUIFormation : CurrentUIShip;
		from.SetTargetObject(null);
	}
		
	private void FriendlyContextMenuJoinFormationClicked()
	{
		var target = friendlyContextMenuWorldShip;
		if (target.GetFormation(out var targetsFormation))
		{
			UnitCommander.JoinFormation(CurrentUIShip, targetsFormation);
		}
		else
		{
			UnitCommander.FormFormation(CurrentUIShip, target);
		}
	}

	private void EnemyContextMenuAttackClicked()
	{
		var target = enemyContextMenuWorldShip;
		IGameWorldOrderable from = CurrentUIFormation != null ? CurrentUIFormation : CurrentUIShip;
		
		if (target == null || from == null)
			return;

		from.SetTargetObject(target);
	}

	private void UIShipChanged()
	{
		ShipLabel.Text = CurrentUIShip?.ShipData?.Name ?? "";
		if (CurrentUIShip == null)
		{
			BroadsideCheckButton.Disabled = true;
			MovementBehaviorOptionButton.Disabled = true;
			PrevShipButton.Disabled = true;
			NextShipButton.Disabled = true;
		}
		else
		{
			BroadsideCheckButton.Disabled = false;
			MovementBehaviorOptionButton.Disabled = false;
			PrevShipButton.Disabled = false;
			NextShipButton.Disabled = false;

			BroadsideCheckButton.ButtonPressed = CurrentUIShip.Broadside;
			MovementBehaviorOptionButton.Selected = (int)CurrentUIShip.DirectionBehavior;
		}

		SetupDynamicShipUI();
	}

    private void SetupDynamicShipUI()
    {
		//clear:
		dynamicShipUIControls.ForEach(x => x.QueueFree());
		dynamicShipUIControls.Clear();
		//TODO: Do this lol
		
    }

    private void UIFormationChanged()
	{
		FormationLabel.Text = CurrentUIFormation?.Name ?? "";
		if (CurrentUIFormation != null)
		{
			CurrentUIShip = CurrentUIFormation.Anchor;
		}else
		{
			CurrentUIShip = null;
		}
	}

	private List<T> GetCurrentlySelected<T>() where T : IGameWorldSelectable
	{
		return GetTree().GetNodesInGroup("UISelected").OfType<T>().ToList();
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{


		// if (count <= 1)
		// 	contextMenu.Disable();
		// else if (!contextMenu.IsEnabled())
		// 	contextMenu.Enable();

		//HandleUISelection();
		DrawSelectionLine();
		UpdateContextMenu();
	}

	private void UpdateContextMenu()
    {
        
    }

    // private void HandleUISelection()
    // {
    // 	int count = GetTree().GetNodeCountInGroup("UISelected");
	// 	if (count == 0)
	// 	{
	// 		CurrentUIFormation = null;
	// 		CurrentUIShip = null;
	// 	}
		
	// 	if(count > 0)
	// 	{			
	// 		var ships = GetTree().GetNodesInGroup("UISelected").OfType<WorldShip>().ToList();
	// 		var formations = GetTree().GetNodesInGroup("UISelected").OfType<WorldFormation>().ToList();
			
	// 		if(formations.Count() > 0)
	// 		{
	// 			CurrentUIFormation = formations.First();
	// 			CurrentUIShip = CurrentUIFormation.WorldShips.First();
	// 			GD.Print("HandleUISelection count > 0 - 1", CurrentUIShip);
	// 		}
	// 		if(CurrentUIFormation == null && ships.Count() > 0)
	// 		{
	// 			CurrentUIShip = ships.First();
	// 			GD.Print("HandleUISelection count > 0 - 2", CurrentUIShip);
	// 		}
	// 	}
    // }

    private void DrawSelectionLine()
    {
		if (CurrentUIShip == null)
		{
			UISelectionLine.Hide();
			return;
		}

		//GD.Print("DrawSelectionLine1");
		Camera3D camera = GameWorldTest.InMapMode ? OrthogonalCamera3d.Instance : OrbitalCamera.Instance;

		if (camera == null)
			return;
		
		//GD.Print("DrawSelectionLine2");
		
		var from = camera.UnprojectPosition(CurrentUIShip.GlobalPosition);
		var to = UISelectionLineToMarker.GlobalPosition;
		if(!GameWorldTest.InMapMode)
			from = from.MoveToward(to, 50);
		else
			from = from.MoveToward(to, 5);
			
		UISelectionLine.ClearPoints();
		UISelectionLine.AddPoint(from);
		UISelectionLine.AddPoint(to);
		UISelectionLine.AddPoint(to + new Vector2(30, 0));
		UISelectionLine.Show();
		
    }
}
