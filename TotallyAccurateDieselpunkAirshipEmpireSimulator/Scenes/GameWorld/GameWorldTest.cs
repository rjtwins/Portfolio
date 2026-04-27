using Godot;
using System;
using System.Linq;
using System.Threading.Tasks;

public partial class GameWorldTest : Node3D
{
	// Called when the node enters the scene tree for the first time.
	[Export] public string ShipName { get; set; }
	[Export] public OrbitalCamera OrbitalCamera { get; set; }
	[Export] public Camera3D OrthogonalCamera { get; set; }
	
	public static WorldShip UIMouseHoveringOverWorldShip { get; set; }

	public delegate void OnMapModeChanged(bool newValue);
	public static event OnMapModeChanged MapModeChanged;
	
	public static bool MouseInUI { get; set; }

	private float lastPitch = 0f;
	private float lastYaw = 0f;
    public static bool WillOpenContextMenu {
    	get 
		{
			return UIMouseHoveringOverWorldShip != null;
		}
    }	
	public static bool InMapMode 
	{
		get => _inMapMode;
		set  
		{
			if (_inMapMode == value)
				return;
				
			_inMapMode = value;
			MapModeChanged.Invoke(value);
		}
	}


    private static bool _inMapMode = false;
	
	public override void _Ready()
    {
		MapModeChanged += OnOnMapModeChanged;
    
		var shipBlueprint = globals.LoadShipFromFile(ShipName);
		var shipState = globals.ConvertToWorldShipState(shipBlueprint);

		var ship1 = LoadShip(new Vector3(-30, 300, -30), shipState);
		// var ship2 = LoadShip(new Vector3(-15, 300, -15), shipState);
		// var ship3 = LoadShip(new Vector3(0, 300, 0), shipState);
		// var ship4 = LoadShip(new Vector3(15, 300, 15), shipState);
		// var ship5 = LoadShip(new Vector3(30, 300, 30), shipState);
		
		var ship6 = LoadShip(new Vector3(5000, 300, 30), shipState);
		ship6.ShipData.Faction = Faction.ENEMY;

		ship1.ShipData.Name = "Ship 1";

		ship1.TargetObject = ship6;
		ship1.Broadside = true;
		// ship2.ShipData.Name = "Ship 2";
		// ship3.ShipData.Name = "Ship 3";
		// ship4.ShipData.Name = "Ship 4";
		// ship5.ShipData.Name = "Ship 5";
		
		// ship1.TargetObject = ship6;
		// ship2.TargetObject = ship6;
		// ship3.TargetObject = ship6;
		// ship4.TargetObject = ship6;
		// ship5.TargetObject = ship6;
		
		//var ship2 = LoadShip(new Vector3(1000, 0, 0), shipState);

		// //ship1.TargetObject = ship2;
		// ship1.TargetPosition = new Vector3(0, 300, 0);
		// //ship2.TargetPosition = new Vector3(0, 5, 0);

		// Timer timer = new Timer();
		// timer.OneShot = false;
		// timer.WaitTime = 5f;
		// timer.IgnoreTimeScale = true;

		// timer.Timeout += () =>
		// {
		// 	var pos = new Vector3((float)Random.Shared.NextDouble() * 100, (float)Random.Shared.NextDouble() * 100 + 300, (float)Random.Shared.NextDouble() * 100);
		// 	ship1.TargetPosition = new Vector3(pos.X - 30, pos.Y, pos.Z - 30);
		// 	ship2.TargetPosition = new Vector3(pos.X - 15, pos.Y, pos.Z - 15);
		// 	ship3.TargetPosition = new Vector3(pos.X - 0, pos.Y, pos.Z - 0);
		// 	ship4.TargetPosition = new Vector3(pos.X + 15, pos.Y, pos.Z + 15);
		// 	ship5.TargetPosition = new Vector3(pos.X + 30, pos.Y, pos.Z + 30);
		// };

		// AddChild(timer);
		// timer.Start();
    }

    private void OnOnMapModeChanged(bool obj)
    {
    	if(obj)
    	{
			lastPitch = OrbitalCamera.Pitch;
			lastYaw = OrbitalCamera.Yaw;
			
			OrbitalCamera.Pitch = 1.553f;
			//OrbitalCamera.Yaw = 0f;
			OrbitalCamera.UpdateCameraPosition();
			OrbitalCamera.RotatingEnabled = false;
			OrbitalCamera.CameraZoomSpeed = 10000f;
			OrbitalCamera.CameraSpeed = 5000;
			
			OrbitalCamera.ClearCurrent();
			OrthogonalCamera.MakeCurrent();
    	}else
    	{
    	    OrbitalCamera.RotatingEnabled = true;
    	    OrbitalCamera.CameraZoomSpeed = 1000f;
    	    OrbitalCamera.CameraSpeed = 1000f;
			OrbitalCamera.Pitch = lastPitch;
			OrbitalCamera.OrbitDistance = 900f;
			//OrbitalCamera.Yaw = lastYaw;
			
			OrthogonalCamera.ClearCurrent();
			OrbitalCamera.MakeCurrent();
    	}
    }

    public WorldShip LoadShip(Vector3 position, WorldShipState shipState)
    {
		var ship = globals.WorldShipFromWorldShipState(shipState);
		this.AddChild(ship);
		
		//ship.GlobalPosition = new Vector3(0, 100, 0);
		
		//Find CIC
		ship.ShipData.CIC = (Component)ship.ShipData.Components.Where(x => x is Component).ToList().First();
		ship.Freeze = true;
		//ship.GlobalRotation = new Vector3(0, MathF.PI, 0);
		ship.GlobalPosition = position;
		ship.TargetPosition = position;
		
		Task.Factory.StartNew(async () =>
		{
			ship.TuneShipPid();			
			ship.Freeze = false;
		});

		return ship;
    }

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
    {
		if (OrbitalCamera.OrbitDistance > 1000f)
		{
			InMapMode = true;
		}
		else
		{
			InMapMode = false;
		}
		
		
    }
}
