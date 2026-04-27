using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public partial class Fleet : CharacterBody2D, IRWR
{
	[Export] float RotationSpeed = 0.01f;
	[Export] public Selectable Selectable {get; set;}
	[Export] public FleetInfo FleetInfo {get; set;}
	
	[Export] public Faction Faction { get => FleetInfo.Faction; set => FleetInfo.Faction = value; }
	
	public bool IsSelected => Selectable?.Selected ?? false;
	[Export] public Vector2 MoveToPosition = Vector2.Zero;
	public List<MovementWaypoint> MoveToQueue = new();
	public MovementWaypoint _nextPoint = null;
	[Export] RadarOverlay RadarOverlay {get;set;}
	[Export] IROverlay IROverlay {get;set;}
	[Export] RWROverlay RWROverlay {get;set;}
	
	[Export] public StrikeManager StrikeManager {get;set;}
	[Export] ShipModels ShipModels {get; set;}
	[Export] public bool IsSettlementFleet {get; set;}
	
	[Export] public Settlement LandedAtSettlement {get; set;}
		
	private bool _overlaySelectedOnlyMode = true;
	private byte _currentOverlayMode = 0;
	
	[Export] public bool Landed {get; set;} = true;
	[Export] public bool TakingOff {get; set;} = false;
	[Export] public bool Landing {get; set;} = false;
	public bool IgnoreOrders {get; set;} = false;
	
	[Export] ScreenLine2D MoveToLine {get; set;}
	[Export] Label MoveToLabel {get; set;}

	[Export] public bool DebugFleet { get; set; } = false;
		
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		if(IsSettlementFleet)
			SetupSettlementFleet();
		else
			SetupMobileFleet();
	}

	private void SetupMobileFleet()
	{
		if(DebugFleet)
		{
			for (int i = 0; i < 10; i++)
			{
				var ship = globals.MapShipFromShipFile("Ship3");
				FleetInfo.AddShip(ship);
			}

			//DEBUG:
			this.FleetInfo.Ships.Where(x => x.ShipData.CanTakeAircraft()).ToList().ForEach(x =>
			{
				x.ShipData.AddAircraftToShip(new StrikeCraftData() { AircraftType = AircraftType.A1, Health = 100 });
			});
			
			if(Faction != Faction.PLAYER)
			{
			    MoveToPoint(new Vector2(10, 10));
			}
		    
		}
	
		RadarOverlay.Visible = false;
		IROverlay.Visible = false;
		SetupShipModels();
		
		if(Faction != Faction.PLAYER)
		{
		    RadarOverlay.Visible = false;
		    IROverlay.Visible = false;
		    RWROverlay.Visible = false;
		    Selectable.CanSelect = false;
		    var mouseDetector = GetNode<Area2D>("MouseDetector");
		    mouseDetector.Monitorable = false;
		    mouseDetector.Monitoring = false;
		    
		    GetNode<Node2D>("NonRotation/SensorOverlay").Hide();
		    GetNode<CanvasItem>("CircleLine2D").Hide();
		    GetNode<CanvasItem>("DirectionLine").Hide();
		}
		//var aircraft = new List<MapAircraft>() { new MapAircraft() { AircraftType = AircraftType.A1 } };
	}

	private void SetupSettlementFleet()
	{
		// RadarOverlay.Visible = false;
		// IROverlay.Visible = false;
		
		//SetupShipModels();
		//var aircraft = new List<MapAircraft>() { new MapAircraft() { AircraftType = AircraftType.A1 } };
		
		//ShipModels.Hide();
		// GetNode<Node2D>("NonRotation").Hide();
		// GetNode<Area2D>("MouseDetector").Monitorable = false;
		// GetNode<Area2D>("MouseDetector").Monitoring = false;
		
		// GetNode<Area2D>("MouseDetector").Monitorable = false;
		// GetNode<Area2D>("MouseDetector").Monitoring = false;
		
		//Selectable.CanSelect = false;
		
		SetPhysicsProcess(false);
		SetProcess(false);
	}

	public override void _UnhandledInput(InputEvent @event)
	{
		if(IsSettlementFleet)
			return;
		
		if(Faction != Faction.PLAYER)
			return;
		
		if(OverworldMouseFollower.Instance.IsOverUI())
			return;
		
		if(IgnoreOrders)
			return;
			
		if(StrikeManager.StrikeReadyToLaunch)
			return;
			
		HandleOrder(@event);
		base._UnhandledInput(@event);
	}
	
	public void HandleOrder(InputEvent @event)
	{		
		if(!(@event is InputEventMouseButton mouseButton))
			return;
			
		if(mouseButton.ButtonIndex != MouseButton.Right)
			return;
		
		if(!mouseButton.IsReleased())
			return;
			
		if(!IsSelected)
			return;
			
		MoveToPoint(_nextPoint);
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		if(IsSettlementFleet)
			return;
			
		if(Faction != Faction.PLAYER)
			return;
			
		var points = MoveToQueue.ToList();
		points.Insert(0, new(MoveToPosition));
		points.Insert(0, new(GlobalPosition));
				
		var distance = points.Skip(1).Select((x, i) => x.Point.DistanceTo(points[i].Point)).Sum();
		var time = distance.MPixelToMeter() / FleetInfo.SpeedMS;
		var ts = TimeSpan.FromSeconds(time);
		
			
		if(distance < 1f && MoveToQueue.Count == 0 || MoveToPosition == default)
		{
			MoveToLine.Visible = false;
			MoveToLabel.Visible = false;
		}
		else
		{
			MoveToLine.Visible = true;
			MoveToLabel.Visible = true;
		}
		
		MoveToLabel.Text = $"{Math.Round(ts.TotalHours,1)} H";		
		MoveToLabel.GlobalPosition = MoveToLine.Points.LastOrDefault();
		MoveToLine.Points = points.Select(x => x.Point).ToArray();
		
		if(!IsSelected)
			return;
			
		if(OverworldMouseFollower.Instance.IsOverUI())
			return;
		
		if(IgnoreOrders)
			return;
			
		if(StrikeManager.StrikeReadyToLaunch)
			return;
		
		var orderPosition = GetGlobalMousePosition();
		Node2D orderAnchor = null;
		if(Input.IsActionPressed("right_click") && OverworldMouseFollower.Instance.HasFleetOrSettlementUnderMouse())
		{
			//var adjustment = OverworldMouseFollower.Instance.HasSettlementUnderMouse() ? Vector2.Up * 30 : Vector2.Zero;
			//orderPosition = OverworldMouseFollower.Instance.GetFleetOrSettlementUnderMouse().GlobalPosition + adjustment;
			orderAnchor = OverworldMouseFollower.Instance.GetFleetOrSettlementUnderMouse();
		}
		
		MovementWaypoint movementWaypoint;
		if(orderAnchor != null)
			movementWaypoint= new MovementWaypoint(orderAnchor);
		else
			movementWaypoint = new MovementWaypoint(orderPosition);
		
		
		//Handle input:
		if(Input.IsActionPressed("right_click") && Input.IsActionPressed("shift"))
		{
			points = MoveToQueue.ToList();
			points.RemoveAll(x => x == default);
			MoveToQueue = new(points);
			
			if(MoveToPosition != default)
				points.Insert(0, movementWaypoint);
				
			points.Insert(0, movementWaypoint);
			points.Add(movementWaypoint);
			
			distance = points.Skip(1).Select((x, i) => x.Point.DistanceTo(points[i].Point)).Sum();
			time = distance.MPixelToMeter() / FleetInfo.SpeedMS;
			ts = TimeSpan.FromSeconds(time);
			
			MoveToLabel.Text = $"{Math.Round(ts.TotalHours,1)} H";
			MoveToLine.Visible = true;
			MoveToLabel.Visible = true;
			MoveToLine.Points = points.Select(x => x.Point).ToArray();
		}
		else if(Input.IsActionPressed("right_click"))
		{
			distance = GlobalPosition.DistanceTo(orderPosition);
			time = (distance.MPixelToMeter() / FleetInfo.SpeedMS);
			ts = TimeSpan.FromSeconds(time);
			MoveToLabel.Text = $"{Math.Round(ts.TotalHours,1)} H";
			MoveToLine.Visible = true;
			MoveToLabel.Visible = true;
			MoveToLine.Points = new Vector2[] { GlobalPosition, orderPosition };
		}
	}

	public override void _PhysicsProcess(double delta)
	{
		if(IsSettlementFleet)
			return;
			
		delta *= 60;
		
		if(Landed || TakingOff)
			return;
		
		if(MoveToPosition == default)
		{
			MoveToPosition = MoveToQueue.FirstOrDefault().Point;
			if(MoveToPosition != default)
				MoveToQueue.RemoveAt(0);
		}
		
		if(MoveToPosition == default)
			return;
			
		GD.Print("Fleet ", this.Name, " moving to ", MoveToPosition);
		
		FleetInfo.Fuel -= FleetInfo.FuelConsumption * (float)delta;
		FleetInfo.Fuel = Math.Max(FleetInfo.Fuel, 0);
		
		//var targetVector = GlobalPosition.DirectionTo(_moveToPoint);
		var targetVector = MoveToPosition - GlobalPosition;
		float speed = FleetInfo.SpeedPX * (float)delta;
		
		var targetRotation = targetVector.Angle();
		var toRotate = (GlobalRotation - targetRotation) * -1;
		toRotate = Mathf.Wrap(toRotate, -Mathf.Pi, Mathf.Pi);
		var direction = Math.Sign(toRotate);
		toRotate = Math.Abs(toRotate);
		
		if(toRotate < 0.05f)
			toRotate = 0f;
			
		toRotate = MathF.Min(RotationSpeed * (float)delta, toRotate) * direction;
		
		Rotate(toRotate);
		
		if(speed > MoveToPosition.DistanceTo(GlobalPosition))
		{
			GlobalPosition = MoveToPosition;
			MoveToPosition = MoveToQueue.FirstOrDefault().Point;
			if(MoveToPosition != default)
				MoveToQueue.RemoveAt(0);
		}
		else
		{
			GlobalPosition = GlobalPosition.MoveToward(MoveToPosition, speed);
		}
	}

	public void MoveToPoint(MovementWaypoint movementWaypoint)
	{
		if(Input.IsActionPressed("shift"))
		{
			MoveToQueue.Add(movementWaypoint);
		}
		else
		{
			MoveToQueue.Clear();
			MoveToPosition = movementWaypoint.Point;
		}

        GD.Print("Fleet ", this.Name, " has been told to move to ", movementWaypoint);

		if(Landed && !TakingOff)
			TakeOff();
	}

	public void MoveToPoint(Vector2 point)
	{
		if(Input.IsActionPressed("shift"))
		{
			MoveToQueue.Add(new(point));
		}
		else
		{
			MoveToQueue.Clear();
			MoveToPosition = point;
		}

        GD.Print("Fleet ", this.Name, " has been told to move to ", point);

		if(Landed && !TakingOff)
			TakeOff();
	}

	private Tween TakeOff()
	{
		GD.Print(this, " Taking off");
		if(LandedAtSettlement != null)
		{
			TakeOffFromSettlement(LandedAtSettlement);
			return null;
		}
			
		TakingOff = true;
		var tween = CreateTween();
		tween.TweenProperty(this, "position:y", -30, 30).AsRelative();
				
		ShipModels.GetChildren().OfType<MapShipModel>().ToList().ForEach(x => 
		{
			x.TakeOff();
		});
		
		tween.Finished += () =>
		{
			Landed = false;
			TakingOff = false;
		};
		
		return tween;
	}
	
	private Tween Land()
	{
		Landing = true;
		var tween = CreateTween();
		tween.TweenProperty(this, "position:y", 30, 10).AsRelative();
				
		ShipModels.GetChildren().OfType<MapShipModel>().ToList().ForEach(x => 
		{
			x.Land();
		});
		
		tween.Finished += () =>
		{
			Landed = true;
			Landing = false;
		};
		
		return tween;
	}

	/// <summary>
	/// Get ETA to target in seconds
	/// </summary>
	/// <returns></returns>
	public float GetEta()
	{
		var distance = MoveToPosition.DistanceTo(GlobalPosition);
		if(distance < 1)
			return -1f;
			
		return distance / FleetInfo.SpeedPX;
	}
	
	public void OverlayMode(byte mode)
	{
		if(IsSettlementFleet)
			return;
			
		if(Faction != Faction.PLAYER)
			return;
			
		_currentOverlayMode = mode;
		
		if(_overlaySelectedOnlyMode && !IsSelected)
			mode = 0;
			
		switch (mode)
		{
			case 0:
				RadarOverlay.Visible = false;
				IROverlay.Visible = false;
				RWROverlay.Visible = false;
			break;
			case 1:
				IROverlay.Visible = false;
				RadarOverlay.Visible = true;
				RWROverlay.Visible = false;
				break;
			case 2:
				IROverlay.Visible = true;
				RadarOverlay.Visible = false;
				RWROverlay.Visible = false;
				break;
			case 3:
				RadarOverlay.Visible = false;
				IROverlay.Visible = false;
				RWROverlay.Visible = true;
				break;
			case 4:
				RadarOverlay.Visible = true;
				IROverlay.Visible = true;
				RWROverlay.Visible = true;
				break;
			default:
			break;
		}
	}

	public void SelectedOnly(bool state)
	{
		_overlaySelectedOnlyMode = state;
		OverlayMode(_currentOverlayMode);
	}
	
	public void FleetSelected(Fleet _)
	{
		OverlayMode(_currentOverlayMode);
	}
	
	public void FleetUnselected()
	{
		OverlayMode(_currentOverlayMode);
	}

	public void ReceiveRadiation(Vector2 point, RadiationType type, Guid sourceId)
	{
		RWROverlay.ReceiveRadiation(point, type, sourceId);
	}
	
	public void SetupShipModels()
	{
		ShipModels.Reset();
		FleetInfo.Ships.ForEach(x => 
		{
			ShipModels.AddShipModel();
		});
	}

	public void DetachShips(List<MapShip> ships)
	{
		FleetInfo.Ships.RemoveAll(x => ships.Contains(x));
		
		var newFleet = ResourceLoader.Load<PackedScene>("uid://dri03ny3fx5ny").Instantiate<Fleet>();
		newFleet.FleetInfo.Ships = ships;
		newFleet.Name = "TaskForce";
		
		FleetManager.Instance.AddChild(newFleet);
		newFleet.Owner = GetParent();
		
		newFleet.GlobalPosition = GlobalPosition;
		newFleet.SetupShipModels();
		
		if(FleetInfo.Ships.Count == 0 && !IsSettlementFleet)
			QueueFree();
	}

	public void JoinOtherFleet(Fleet fleet)
	{
		fleet.FleetInfo.Ships.AddRange(FleetInfo.Ships);
		fleet.SetupShipModels();
		
		FleetInfo.Ships.Clear();
		
		if(IsSelected)
			Selectable.UnSelect();
			
		QueueFree();
	}

	internal void LandAtSettlement(Settlement settlement)
	{
		Selectable.CanSelect = false;
		var tween = Land();
		LandedAtSettlement = settlement;
		tween.TweenProperty(this, "global_position", settlement.GlobalPosition, 1);
	}

	public void TakeOffFromSettlement(Settlement settlement)
	{
		Selectable.CanSelect = true;
		LandedAtSettlement = null;
		var tween = TakeOff();
		tween = tween == null ? CreateTween() : tween;
		tween.TweenProperty(this, "global_position", settlement.GlobalPosition, 1);
	}

	public List<MapAircraft> GetAirGroup()
	{
		return FleetInfo.GetAirGroup();
	}
	
	public List<MissileData> GetMissileStores()
	{
		return FleetInfo.GetMissileStores();
	}
	
	public void RemoveFromAirgroup(MapAircraft aircraft)
	{
		aircraft.Mothership.ShipData.RemoveAircraftFromShip(aircraft.StrikeCraftData.AircraftType);
	}
	
	public void RetrieveIntoAirgroup(MapAircraft aircraft)
	{
		if(aircraft.Mothership?.ShipData?.CanTakeAircraft() ?? false)
		{
			aircraft.Mothership.ShipData.AddAircraftToShip(aircraft.StrikeCraftData);
		}

		var newHome = this.FleetInfo.Ships.FirstOrDefault(x => x.ShipData.CanTakeAircraft());
		
		if (newHome == null)
		{
			aircraft.Mothership = null;
			aircraft.QueueFree();
			return;
		}

		newHome.ShipData.AddAircraftToShip(aircraft.StrikeCraftData);		
	}

	public float CompasRotation
	{
		get
		{
			var absRot = RotationDegrees + 90;
			absRot = absRot < 0 ? absRot + 360 : absRot;
			absRot = absRot > 360 ? absRot - 360 : absRot;
			return absRot;
		}
	}
}

public class MovementWaypoint
{
	public Vector2 Point 
	{
	    get
	    {
	        if(_point == null)
	        {
	            var adjustment = _anchor is Settlement ? Vector2.Up * 30 : Vector2.Zero;
	        	return _anchor.GlobalPosition + adjustment;
	        }
	        return _point.Value;
	    }
	}
	private Vector2? _point;
	private Node2D _anchor;
	
	public MovementWaypoint(Vector2 point)
	{
		this._point = point;
	}
	
	public MovementWaypoint(Node2D anchor)
	{
	    this._anchor = anchor;
	}
}
