using Godot;

public interface ISavable
{
	public string SaveState();
}

public interface ILoadable
{
	public void LoadState(string data);
}

public partial class ComponentBase : Node, ISavable, ILoadable
{
	[Export] public string Label {get; set;}
	[Export] public string Description {get; set;}
	[Export] public int Weight { get; set; } //kg
	[Export] public int Value { get; set; }
	
	// public int CostA { get; set; }
	// public int CostB { get; set; }
	// public int CostC { get; set; }
	// public int CostD { get; set; }
	[Export] public int Health { get; set; } = 100;
	[Export] public int Armor { get; set; }
	
	//Instance variables
	[Export] public int CurrentHealth {get; set;} = 100;
	
	public override void _Ready()
	{
		this.ProcessMode = ProcessModeEnum.Disabled;
		//this.PhysicsInterpolationMode = PhysicsInterpolationModeEnum.Off;
	}
	
	public virtual void OnShipLoaded()
	{
		
	}

    public virtual string SaveState()
    {
		return Newtonsoft.Json.JsonConvert.SerializeObject(new
		{
			CurrentHealth,
		});
    }

    public virtual void LoadState(string json)
    {
		dynamic data = Newtonsoft.Json.JsonConvert.DeserializeObject(json);
		this.CurrentHealth = data.CurrentHealth;
    }
}