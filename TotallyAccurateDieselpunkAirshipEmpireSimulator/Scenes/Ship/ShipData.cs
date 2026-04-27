using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public partial class ShipData : Node
{
	public List<Component> Components { get; set; } = new();
	public Component CIC { get; set; }
	
	[Export] public Node3D TargetObject { get; set; }
	[Export] public Vector3 TargetPosition { get; set; }
	
	[Export] public float Thrust { get; set; } //kg
	[Export] public float Weight { get; set; } //kg
	[Export] public float Lift { get; set; } //kg
	
	[Export] public bool IsWrecked { get; set; }

	[Export] public Faction Faction { get; set; } = Faction.PLAYER;

	private bool _mapShip;
	
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
    {
        _mapShip =  !(GetParent() is Node3D);
        
		Thrust = Components
			.Select(x => x.Data)
			.Where(x => x is EngineComponent)
			.Cast<EngineComponent>()
			.Sum(x => x.Thrust);
			
		Weight = Components
			.Select(x => x.Data)
			.Cast<ComponentBase>()
			.Sum(x => x.Weight);
			
		Lift = Components
			.Select(x => x.Data)
			.Where(x => x is FunctionalComponent)
			.Cast<FunctionalComponent>()
			.Sum(x => x.PassiveLift);
    }
    
    public void Update()
    {
		if (IsWrecked)
			return;
		
		Components = Components.Where(x => x.Data.Health > 0).ToList();
		
		if(!Components.Any(x => x.ComponentType == "CIC"))
		{
			IsWrecked = true;
			GD.Print("Ship was Wrecked");
			return;
		}
    
		Thrust = Components
			.Select(x => x.Data)
			.Where(x => x is EngineComponent)
			.Cast<EngineComponent>()
			.Sum(x => x.Thrust);
			
		Weight = Components
			.Select(x => x.Data)
			.Cast<ComponentBase>()
			.Sum(x => x.Weight);
			
		Lift = Components
			.Select(x => x.Data)
			.Where(x => x is FunctionalComponent)
			.Cast<FunctionalComponent>()
			.Sum(x => x.PassiveLift);

		Components
			.Select(x => x.Data)
			.OfType<TurretComponent>()
			.ToList()
			.ForEach(x =>
			{
				x.EngageTarget(TargetObject);
			});
    }

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
    {


		//GD.Print($"Thrust: {Thrust}, Weight: {Weight}");
    }

	public List<MissileData> GetMissileStores()
	{
		return Components.Select(x => x.Data).OfType<TacMissileComponent>().SelectMany(x => x.MissileStores).Select(x => globals.MissileDictionary[x]).ToList();
	}
	
	public void RemoveMissileFromStore(string missileIdentifier)
	{
		var component = Components
			.Select(x => x.Data)
			.OfType<TacMissileComponent>()
			.FirstOrDefault(x => x.MissileStores.Contains(missileIdentifier));
		
		component.MissileStores.Remove(missileIdentifier);
	}
	
	public void AddMissileToStore(string missileIdentifier)
	{
		var component = Components
			.Select(x => x.Data)
			.OfType<TacMissileComponent>()
			.FirstOrDefault(x => x.MissileStores.Count < x.MissileCapacity);

		component.MissileStores.Add(missileIdentifier);
	}

    public void RemoveAircraftFromShip(AircraftType aircraftType)
    {
		var component = Components
			.FirstOrDefault(x => x.Data is HangerComponent hanger && hanger.StrikeCraft?.AircraftType == aircraftType).Data as HangerComponent;

		component.StrikeCraft = null;
    }
    
    public void AddAircraftToShip(StrikeCraftData strikeCraft)
    {
		var component = Components
			.FirstOrDefault(x => x.Data is HangerComponent hanger && hanger.StrikeCraft == null).Data as HangerComponent;

		component.StrikeCraft = strikeCraft;
    }
    
    public bool CanTakeAircraft()
    {
        return Components
			.Any(x => x.Data is HangerComponent hanger && hanger.StrikeCraft == null);
    }
}
