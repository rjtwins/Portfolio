using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Code
{
	public class SettlementDto: DtoBase<Settlement>
	{
		public Faction Owner {get; set;} = Faction.ENEMY;

		//Flags
		public byte ShipYard { get; set; } = 1; //DEBUG SET BACK TO 0;
		public byte Radar{get; set;} = 0;
		public byte FuelingStation {get; set;} = 0;
		public byte FuelRefinery{get; set;} = 0;
		public byte MunitionsFactory {get; set;} = 0;
		public byte MetalExtractor {get; set;} = 0;
		public byte Academy {get; set;} = 0;
		public byte ResearchCenter {get; set;} = 0;
		
		
		//Production and stores
		public float MetalProduction {get; set;} = 0;
		public float VolatilesProduction {get; set;} = 0;
		
		public float MunitionsProduction {get; set;} = 0;
		public float ManpowerProduction {get; set;} = 0;
		public float ShipProduction {get; set;} = 0;
		public float ResearchProduction {get; set;} = 0;
		public float FundsProduction {get; set;} = 0f;

		public ObservableCollection<ShipQueueItem> ShipBuildQueue { get; set; } = new();
		
		public ObservableCollection<BuildingQueueItem> BuildingBuildQueue {get; set;} = new();
    }
	
	public class ShipQueueItem
	{
		public ShipBlueprint ShipBlueprint { get; set; }
		public float Required { get; set; }
		public float Current { get; set; }
	}
	
	public class BuildingQueueItem
	{
		public string Name {get; set;}
	    public float Required {get; set;}
	    public float Current {get; set;}
	}
}