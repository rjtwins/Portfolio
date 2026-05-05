using Godot;
using System;

public partial class HangerComponent : FunctionalComponent
{	
	public StrikeCraftData? StrikeCraft { get; set; }

    public override string SaveState()
    {
		var data = new
		{
			baseData = base.SaveState(),
			StrikeCraft, 
		};
		return Newtonsoft.Json.JsonConvert.SerializeObject(data);
    }

    public override void LoadState(string json)
    {
        dynamic data = Newtonsoft.Json.JsonConvert.DeserializeObject(json);
		this.StrikeCraft = data.StrikeCraft;
		
		base.LoadState((string)data.baseData);
    }
}
