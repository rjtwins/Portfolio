using Godot;
using System.Collections.Generic;

public partial class TacMissileComponent : FunctionalComponent
{
    [Export] public float MissileCapacity { get; set; }

	public List<string> MissileStores { get; set; } = new();

    public override void _Ready()
    {
        base._Ready();
        
        //DEBUG:
        for (int i = 0; i < MissileCapacity; i++)
		{
			MissileStores.Add("Harpoonsky");
		}
    }

    
	public override string SaveState()
    {
		var data = new
		{
			baseData = base.SaveState(),
			MissileStores,
		};
		return Newtonsoft.Json.JsonConvert.SerializeObject(data);
    }

    public override void LoadState(string json)
    {
        dynamic data = Newtonsoft.Json.JsonConvert.DeserializeAnonymousType(json, new { baseData = "", MissileStores = new List<string>() });
		base.LoadState((string)data.baseData);

		this.MissileStores = data.MissileStores;
    }	
}
