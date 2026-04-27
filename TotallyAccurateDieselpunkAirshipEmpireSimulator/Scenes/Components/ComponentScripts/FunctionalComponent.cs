using System;
using Godot;

public partial class  FunctionalComponent : ComponentBase
{
	[Export] public float PowerGeneration {get; set;} = 0f;
	[Export] public float PowerUsage {get; set;} = 0f;
	[Export] public float FuelConsumption {get; set;} = 1f;
	[Export] public float PowerLevel {get; set;} = 0f;
	[Export] public float PassiveLift { get; set; } = 0f;

    public override string SaveState()
    {
		var data = new
		{
			baseData = base.SaveState(),
			PowerLevel,
		};
		return Newtonsoft.Json.JsonConvert.SerializeObject(data);
    }

    public override void LoadState(string json)
    {
        dynamic data = Newtonsoft.Json.JsonConvert.DeserializeObject(json);
		base.LoadState((string)data.baseData);

		this.PowerLevel = data.PowerLevel;
    }	
}