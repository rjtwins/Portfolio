using Godot;
using Code;
using System;
using System.Linq;
using System.Collections.Generic;

public partial class Settlement : Node2D
{
	public SettlementDto Data {get; set;}
	[Export] public Label NameLabel {get; set;}
	[Export] public Timer UpdateTimer {get; set;}
	[Export] public EconInfo EconInfo {get; set;}
	private byte _currentOverlayMode;
	[Export] CircleLine2D MapCircle;
	[Export] Area2D MouseDetector;
	
	[Export] Color PlayerColor;
	[Export] Color AIColor;
	[Export] public Selectable Selectable;
	
	[Export] Area2D DetectionArea;
	[Export] public Fleet SettlementFleet { get; set; }
	
	public static Settlement FromDto(SettlementDto data)
	{
		return new()
		{
			Data = data
		};
	}
	
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		if(Data == null)
			Data = new();
			
		UpdateTimer.Timeout += Update;
		
		OverlayMode(0);
		
		DetectionArea.AreaEntered += DetectionAreaAreaEntered;
		DetectionArea.AreaExited += DetectionAreaAreaExited;
		
		if(SettlementFleet == null)
		{
			SettlementFleet = GD.Load<PackedScene>("uid://xddymkkrab8h").Instantiate<Fleet>();
			SettlementFleet.IsSettlementFleet = true;
			SettlementFleet.LandedAtSettlement = this;
			
			AddChild(SettlementFleet);
			SettlementFleet.GlobalPosition = this.GlobalPosition;
		}
	}

	private void DetectionAreaAreaExited(Area2D area)
	{
		
	}


	private void DetectionAreaAreaEntered(Area2D area)
	{
		if(!(area.GetParent() is Fleet f))
			return;
		
		if(f.Landed || f.Landing || f.TakingOff)
			return;
		
		if(f.FleetInfo.Faction == Data.Owner && 
			f.MoveToQueue.Count == 0 && 
			f.MoveToPosition.DistanceTo(GlobalPosition + (Vector2.Up * 30)) < 25)
		{
			GD.Print($"Fleet {f.Name} has entered settlement {Data.Name}");
			SettlementFleet = f;
			f.LandAtSettlement(this);
		}
		else if (f.FleetInfo.Faction != Data.Owner &&
			f.MoveToQueue.Count == 0 && 
			f.MoveToPosition.DistanceTo(GlobalPosition + (Vector2.Up * 30)) < 25)
		{
			GD.Print($"Fleet {f.Name} is attacking settlement {Data.Name}");
			GD.Print($"Fleet {f.Name} has captured settlement {Data.Name}");
			Data.Owner = f.FleetInfo.Faction;
		}
	}

	//This runs every second, se we do not need to worry about deltas
	private void Update()
	{
		//To prevent missing the area entered because of time warp.
		if(DetectionArea.HasOverlappingAreas())
			DetectionArea.GetOverlappingAreas()
				.ToList()
				.ForEach(x => DetectionAreaAreaEntered(x));
		
		
		//TODO: Player/AI difference:
		//Update production values:
		
		Data.ShipProduction = globals.BuildingProduction[nameof(Data.ShipYard)][Data.ShipYard];
		Data.VolatilesProduction = globals.BuildingProduction[nameof(Data.FuelRefinery)][Data.FuelRefinery];
		Data.MunitionsProduction =  globals.BuildingProduction[nameof(Data.MunitionsFactory)][Data.MunitionsFactory];
		Data.MetalProduction = globals.BuildingProduction[nameof(Data.MetalExtractor)][Data.MetalExtractor];
		Data.ManpowerProduction = globals.BuildingProduction[nameof(Data.Academy)][Data.Academy];
		Data.ResearchProduction = globals.BuildingProduction[nameof(Data.ResearchCenter)][Data.ResearchCenter];
		
		//TODO: Funds production
		Data.FundsProduction = 1f;
		
		globals.Funds += Data.FundsProduction;
		//globals.Officers += Data.OfficerProduction;
		globals.Manpower += Data.ManpowerProduction;
		globals.Research += Data.ResearchProduction;
		globals.Volatiles += Data.VolatilesProduction;
		
		
		if(Data.Owner == Faction.PLAYER)
		{
			Modulate = new Color(1, 1, 1, 1);
			MouseDetector.Monitorable = true;
			MouseDetector.Monitoring = true;
			Selectable.CanSelect = true;
			//(NameLabel.GetParent() as Node2D).Modulate = PlayerColor;
		}
		else
		{
			Modulate = new Color("ffffff5c");
			MouseDetector.Monitorable = false;
			MouseDetector.Monitoring = false;
			Selectable.CanSelect = false;
			//(NameLabel.GetParent() as Node2D).Modulate = new Color(0, 0, 0, 0.5f);
		}

		HandleBuildingBuildQueue();
		HandleShipBuildQueue();
	}
	
	//For now we share ship and building build power.
	private void HandleBuildingBuildQueue()
	{	
		if (!Data.BuildingBuildQueue.Any())
			return;

		var shipBuildingPower = Data.ShipProduction;
		var queueItem = Data.BuildingBuildQueue.First();
		queueItem.Current += shipBuildingPower;
		
		if(queueItem.Current >= queueItem.Required)
		{
			Data.BuildingBuildQueue.Remove(queueItem);
			var building = queueItem.Name;
			
			GD.Print($"Upgrading property: {building}");
			
			//TODO: Get rid of reflection.			
			byte level = (byte)Data.GetType().GetProperty(building).GetValue(Data);
			Data.GetType().GetProperty(building).SetValue(Data, (byte)(level + 1));
		}
	}
	
    private void HandleShipBuildQueue()
	{
		if (!Data.ShipBuildQueue.Any())
			return;

		var shipBuildingPower = Data.ShipProduction;
		var queueItem = Data.ShipBuildQueue.First();
		queueItem.Current += shipBuildingPower;
		
		if(queueItem.Current >= queueItem.Required)
		{
			//Ship finished!!
			//TODO: Add ship to settlement fleet!
			Data.ShipBuildQueue.Remove(queueItem);
			var ship = globals.MapShipFromShipBlueprint(queueItem.ShipBlueprint);
			SettlementFleet.FleetInfo.AddShip(ship);
		}
	}
	
	

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		if(Data.Name != NameLabel.Text)
			NameLabel.Text = Data.Name;
	}

	internal void UpgradeBuilding(string selectedBuilding)
	{
		byte lvl = (byte)Data.GetType().GetProperty(selectedBuilding).GetValue(Data);
		lvl += 1;
		Data.GetType().GetProperty(selectedBuilding).SetValue(Data, lvl);
	}
	
	public bool TryAddShipToQueue(ShipBlueprint bp)
	{
		Data.ShipBuildQueue.Add(new()
		{
			ShipBlueprint = bp,
			Required = 25,
			Current = 0
		});
		
		return true;
	}
	
	public void OverlayMode(byte mode)
	{
		_currentOverlayMode = mode;
		
		if(Data.Owner != Faction.PLAYER)
		{
			EconInfo.Visible = false;
			return;
		}
		
		switch (mode)
		{
			case 0:
				EconInfo.Visible = false;
			break;
			case 1:
				EconInfo.Visible = true;
				//EconInfo.Stockpile = true;
				EconInfo.Update();
				break;
			case 2:
				EconInfo.Visible = true;
				//EconInfo.Stockpile = false;
				EconInfo.Update();
				break;
			default:
			break;
		}
	}
	
    internal void QueueBuilding(string selectedBuilding)
    {
		Data.BuildingBuildQueue.Add(new()
		{
			Name = selectedBuilding,
			Required = 25,
			Current = 0
		});
    }

}
