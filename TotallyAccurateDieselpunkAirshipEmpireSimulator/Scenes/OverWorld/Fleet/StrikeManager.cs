using Godot;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

public partial class StrikeManager : Node
{
	private Fleet fleet => Owner as Fleet;
	[Export] public PackedScene MissileScene {get;set;}
	[Export] public PackedScene StrikeScene {get; set;}
	
	private ReadOnlyCollection<MapAircraft> AirGroupStores => fleet.GetAirGroup().AsReadOnly();
	[Export] public Godot.Collections.Array<MissileGuidanceType> MissileStores {get; set;} = new();
	
	[Export] ScreenLine2D TargetLine;
	[Export] CircleLine2D RangeCircle;
	[Export] Label TargetRangeLabel;
	
	public List<MapAircraft> CurrentStrike = new();
	public bool StrikeReadyToLaunch {get; set;} = false;
	public List<Vector2> CurrentStrikeWaypoints = new();
	
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		if(OverworldMouseFollower.Instance.IsOverUI())
		{
			TargetLine.Visible = false;
			RangeCircle.Visible = false;
			TargetRangeLabel.Visible = false;
			return;
		}
		
		UpdateUI();

		if(Input.IsActionJustReleased("right_click") && CurrentStrike.Count() > 0 && StrikeReadyToLaunch)
		{
			if(Input.IsActionPressed("shift"))
			{
				CurrentStrikeWaypoints.Add(fleet.GetGlobalMousePosition());
			}
			else
			{
				CurrentStrikeWaypoints.Add(fleet.GetGlobalMousePosition());
				LaunchAirstrike(CurrentStrike, CurrentStrikeWaypoints);
			}
		}
			
		if(Input.IsActionJustReleased("click"))
		{
			CurrentStrike.ForEach(x => fleet.RetrieveIntoAirgroup(x));
			CurrentStrike.Clear();
		}
	}

	private void UpdateUI()
	{
		if(!(CurrentStrike.Count() > 0 && StrikeReadyToLaunch))
		{
			TargetLine.Visible = false;
			RangeCircle.Visible = false;
			TargetRangeLabel.Visible = false;
			return;
		}
		
		TargetLine.Visible = true;
		RangeCircle.Visible = true;
		TargetRangeLabel.Visible = true;
		
		var points = CurrentStrikeWaypoints.ToList();
		points.Insert(0, fleet.GlobalPosition);
		points.Add(fleet.GetGlobalMousePosition());
		
		TargetLine.Points = points.ToArray();
		RangeCircle.GlobalPosition = fleet.GlobalPosition;
		RangeCircle.Radius = 1000f.MMeterToPixel();
		
		var distance = points.Skip(1).Select((x, i) => x.DistanceTo(points[i])).Sum();
		TargetRangeLabel.Text = $"{MathF.Round(distance.MPixelToMeter() / 1000f, 1)} KM";
		TargetRangeLabel.GlobalPosition = fleet.GlobalPosition.MoveToward(fleet.GetGlobalMousePosition(), distance + 50);
	}

	public void FireMissile(MissileData data, float direction)
	{
		var missile = MissileScene.Instantiate<MapMissile>();
		missile.MissileData = data;
		missile.FlyTimeRemaining = data.FlyTime;
		
		AddChild(missile);
		missile.GlobalRotation = direction;
		missile.GlobalPosition = (Owner as Node2D).GlobalPosition;
		missile.Owner = this;
	}
	
	//Todo: support launching at fleet/settlement
	public void LaunchAirstrike(List<MapAircraft> strikeAircraft, List<Vector2> waypoints)
	{
		var strike = StrikeScene.Instantiate<MapStrike>();
		strike.aircraft = strikeAircraft.ToArray();
		AddChild(strike);
		strike.MoveToWaypoints(waypoints);
		strike.GlobalPosition = (Owner as Node2D).GlobalPosition;
		strike.Owner = this;
		strike.CarrierFleet = fleet;
		CurrentStrike.Clear();
		CurrentStrikeWaypoints.Clear();
		fleet.IgnoreOrders = true;
		GetTree().CreateTimer(0.1).Timeout += () => 
		{
			fleet.IgnoreOrders = false;
			StrikeReadyToLaunch = false;
		};
	}
	
	public void RetrieveStrike(List<MapAircraft> strikeAircraft)
	{
		strikeAircraft.ForEach(x => fleet.RetrieveIntoAirgroup(x));
	}

	internal void TryAddToStrike(AircraftType aircraftType)
	{
		MapAircraft aircraft = AirGroupStores.FirstOrDefault(x => x.StrikeCraftData.AircraftType == aircraftType);
		if(aircraft == null)
			return;
			
		CurrentStrike.Add(aircraft);
		fleet.RemoveFromAirgroup(aircraft);
	}
}