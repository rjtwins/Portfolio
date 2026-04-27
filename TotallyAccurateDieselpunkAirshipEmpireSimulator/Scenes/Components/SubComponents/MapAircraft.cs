using Godot;

public partial class MapAircraft : Node
{
	[Export] public MapShip Mothership {get; set;}
	public StrikeCraftData StrikeCraftData { get; set; }
}
