using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public partial class FormationTree : Tree
{
	private TreeItem root;
	private TreeItem noFormationRoot;
	private int formationCount = 0;
	private int shipCount = 0;
	
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
    {
		root = CreateItem();
		HideRoot = true;

		noFormationRoot = CreateItem(root);
		noFormationRoot.SetText(0, "No formation:");
		ItemSelected += OnItemSelected;
    }

    private void OnItemSelected()
    {
		GetTree()
			.GetNodesInGroup("UISelected")
			.OfType<IGameWorldSelectable>()
			.ToList()
			.ForEach(x => x.Deselect());
		
		var item = GetSelected();
		if (item == null)
			return;
		
		var metadata = item.GetMetadata(0);

		if(metadata.Obj is WorldFormation wf)
		{
			wf.Select();
		}
		else if(metadata.Obj is WorldShip ws)
        {
			if (ws.GetFormation(out var formation))
				formation.Select();
			else
				ws.Select();
        }
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta)
    {
		var newShipCount = WorldShip.Active.Count;
		var newFormationCount = WorldFormation.Active.Count;

		if (newShipCount == shipCount && newFormationCount == formationCount)
			return;

		noFormationRoot.GetChildren().ToList().ForEach(x => noFormationRoot.RemoveChild(x));
		var formationLessShips = WorldShip.Active.Where(x => !x.GetFormation(out _)).ToList();
		
		formationLessShips.ForEach(x =>
		{
			var item = CreateItem(noFormationRoot);
			item.SetText(0, x.ShipData.Name);
			item.SetMetadata(0, x);
		});

		var formations = WorldFormation.Active.ToList();
		formations.ForEach(x =>
		{
			var formationItem = CreateItem(root);
			formationItem.SetText(0, x.Name);
			formationItem.SetMetadata(0, x);
			x.WorldShips.ForEach(y =>
			{
				var shipItem = CreateItem(formationItem);
				shipItem.SetText(0, y.ShipData.Name);
				shipItem.SetMetadata(0, y);
			});
		});

		shipCount = newShipCount;
		formationCount = newFormationCount;
    }
}
