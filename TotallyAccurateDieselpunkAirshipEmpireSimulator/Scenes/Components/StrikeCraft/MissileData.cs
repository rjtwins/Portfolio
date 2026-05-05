using Godot;
using System;

public class MissileData
{
    public string MissileIdentifier { get; set; } //Unique name for each missile "type". Like Harpoon or Tomahawk.
    public MissileGuidanceType MissileType { get; set; } = MissileGuidanceType.Command; //Guidance type
    public float FlyTime { get; set; } = 20f; //Sec
    public float Value { get; set; }
}
