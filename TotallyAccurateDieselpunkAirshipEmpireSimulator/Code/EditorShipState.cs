using System.Collections.Generic;
using Godot;

public partial class ShipBlueprint
{
    public string Name { get; set; }
    public string Description { get; set; }
    public List<ComponentBluePrint> Components { get; set; } = new();
}

public class WorldShipState : ShipBlueprint
{
    public List<string> ComponentStates { get; set; } = new();
}