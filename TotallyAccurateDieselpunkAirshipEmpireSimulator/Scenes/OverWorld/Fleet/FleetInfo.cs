using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public partial class FleetInfo : Node
{
	[Export] public float MaxSpeedMS = 100f;
	public float MaxSpeedPX => MaxSpeedMS.MMeterToPixel();
	public float MaxSpeedKPH => MaxSpeedMS * 3.6f;

	public float SpeedMS => EngineLevel * MaxSpeedMS;
	public float SpeedPX => SpeedMS.MMeterToPixel();
	public float SpeedKPH => SpeedMS * 3.6f;

	public float RangeM => MaxBurnTime * SpeedMS;
	public float RangePX => RangeM.MMeterToPixel();
	public float RangeKM => RangeM / 1000;

	//KG fuel;
	public float Fuel = 1000000f;
	public float MaxFuel = 1000000f;
	//0-1
	public float EngineLevel = 1f;
	//Kg/s
	public float FuelConsumption = 100f;
	//s
	public float MaxBurnTime => Fuel / FuelConsumption;


	public bool Radar {get; set;}
	public float RadarRange {get; set;} = 250;
	public bool IRSensor {get; set;}
	public float IRRange {get; set;} = 100;
	public bool ELINT {get; set;}
	public float ELINTRange {get; set;} = 1000;
	
	//Active sensor.
	public bool RadarOn {get; set;} = false;
	
	//Kg
	[Export] public float Weight = 10e6f;
	
	public List<MapShip> Ships {get; set;} = new();
	public List<Vector3> FormationOffset = new();
	
	[Export] public Faction Faction {get; set;} = Faction.PLAYER;
	
	[Export] public bool Convoy {get; set;} = false;
	
	[Export] public PackedScene DebugShipScene { get; set; }
		
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		if(Ships.Any())
			return;

		// Ships.Add(globals.MapShipFromShipFile("Ship1"));
		// Ships.Add(globals.MapShipFromShipFile("Ship1"));
		// Ships.Add(globals.MapShipFromShipFile("Ship1"));
		// Ships.Add(globals.MapShipFromShipFile("Ship1"));
		
		// // Ships.Add(DebugShipScene.Instantiate<Ship>());
		// // Ships.Add(DebugShipScene.Instantiate<Ship>());
		// // Ships.Add(DebugShipScene.Instantiate<Ship>());
		// // Ships.Add(DebugShipScene.Instantiate<Ship>());
		
		// Ships.ForEach(x => 
		// {
		// 	AddChild(x);
		// 	x.Owner = this;
		// });
	}
	
	public void RemoveShip(MapShip ship)
	{
		RemoveChild(ship);
		Ships.Remove(ship);
		ship.Owner = null;
	}
	
	public void DeleteShip(MapShip ship)
	{
		Ships.Remove(ship);
		ship.QueueFree();
	}
	
	public void AddShip(MapShip ship)
	{
		//ship.InEditor = true;
		AddChild(ship);
		ship.Owner = this;
		Ships.Add(ship);
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		
	}

    internal List<MapAircraft> GetAirGroup()
    {
		return Ships
			.SelectMany(x => x.ShipData.Components
				.Select(y => y.Data)
				.Where(y => y is HangerComponent)
				.Cast<HangerComponent>()
				.Where(y => y.StrikeCraft?.AircraftType != null)
				.Select(y => new MapAircraft()
				{
					Mothership = x,
					StrikeCraftData = y.StrikeCraft,
				})
			)
			.ToList();
    }

    internal List<MissileData> GetMissileStores()
    {
        return Ships
			.SelectMany(x => x.ShipData.Components
				.Select(y => y.Data)
				.Where(y => y is TacMissileComponent)
				.Cast<TacMissileComponent>()
				.SelectMany(y => y.MissileStores)
				.Select(x => globals.MissileDictionary[x])
			)
			.ToList();
    }

}
