using Godot;
using System.Collections.Generic;
using System.Linq;

public partial class init : Node
{
	public static init Init => _init;
	private static init _init { get; set; }
	public static List<string> SettlementNames = new();
	
	[Export] public PackedScene[] ComponentScenes {get; set;}
	
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		_init = this;
			
		var settlementNamesFile = FileAccess.GetFileAsString("res://Resources/SettlementNames.txt");
		SettlementNames = settlementNamesFile.Split("\n").Select(x => x.Trim()).ToList();
		
		//Production building data:
		var productionBuildings = new Dictionary<string, int[]>()
		{
			{ "ShipYard", new int[] { 0,1,2,3,4,5 } },
			{ "FuelRefinery", new int[] { 0,1,2,3,4,5 } },
			{ "MunitionsFactory", new int[] { 0,1,2,3,4,5 } },
			{ "MetalExtractor", new int[] { 0,1,2,3,4,5 } },
			{ "Academy", new int[] { 0,1,2,3,4,5 } },
			{ "ResearchCenter", new int[] { 0,1,2,3,4,5 } }
		};

		globals.BuildingProduction = productionBuildings;

		//Debug:
		globals.MissileDictionary.Add("Harpoonsky", new MissileData() 
			{ 
				FlyTime = 30, 
				MissileIdentifier = "Harpoonsky", 
				MissileType = MissileGuidanceType.Radar, 
				Value = 100 
			});
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		//Not using this here
	}
}
