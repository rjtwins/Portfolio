using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public partial class globals : Node
{
	public static globals Instance;

	public override void _Ready()
	{
		Instance = this;
	}

	public static bool ReadyToLaunchMissile = false;
	
	public static float Gravity = 9.81f;
	
	//InGameTime in sec, updated by node in OverWorld.
	public static float GameTime = 0f;
	//public static float GameSpeed = 1f;
	private static readonly DateTime StartTime = DateTime.MinValue;
	public const int SecPerSec = 60;
	private static DateTime CurrentDateTime => StartTime.AddSeconds(GameTime);
	public static int HourOfDay => CurrentDateTime.Hour;
	public static int MinOfHour => CurrentDateTime.Minute;
	public static float SecOfMin => CurrentDateTime.Second;
	
	public static float MapMeterPerPixel => 1/MapPixelPerMeter;
	public static float MapPixelPerMeter = 0.01f;
	
	public static float MeterToPixel(float meters)
	{
		return meters * 8f;
	}
	
	public static float PixelToMeter(float pixels)
	{
		return pixels * 0.125f;
	}
	
	public static float Manpower {get; set;}
	public static float Funds {get; set;}
	public static float Metals {get; set;}
	public static float Munitions {get; set;}
	public static float Volatiles {get;set;}
	public static float Research {get; set;}
	
	public static MapToolMode MapToolMode {get; set;}
	
	public static Dictionary<string, int[]> BuildingProduction = new();

	public static Dictionary<string, MissileData> MissileDictionary = new();
	
	// public static void OpenEditor(){
	// 	var shipEditor = (Node2D)Instance.GetTree()?.Root?.GetNode("Master/ShipEditor");
	// 	var overworld = (Node2D)Instance.GetTree()?.Root?.GetNode("Master/Overworld");
		
	// 	if(shipEditor == null || overworld == null)
	// 		return;
		
	// 	shipEditor.Visible = true;
	// 	shipEditor.GetChildren().OfType<CanvasLayer>().ToList().ForEach(x => x.Visible = true);
	// 	shipEditor.GetChildren().OfType<ParallaxBackground>().ToList().ForEach(x => x.Visible = true);
		
	// 	overworld.Visible = false;
	// 	overworld.GetChildren().OfType<CanvasLayer>().ToList().ForEach(x => x.Visible = false);
	// 	overworld.GetChildren().OfType<ParallaxBackground>().ToList().ForEach(x => x.Visible = false);
		
	// 	shipEditor.ProcessMode = ProcessModeEnum.Pausable;
	// 	overworld.ProcessMode = ProcessModeEnum.Disabled;
	// }
	// public static void CloseEditor()
	// {
	// 	var shipEditor = Instance.GetTree().Root.GetNode("Master/ShipEditor") as Node2D;
	// 	var overworld = Instance.GetTree().Root.GetNode("Master/Overworld") as Node2D;
		
	// 	shipEditor.Visible = false;
	// 	shipEditor.GetChildren().OfType<CanvasLayer>().ToList().ForEach(x => x.Visible = false);
	// 	shipEditor.GetChildren().OfType<ParallaxBackground>().ToList().ForEach(x => x.Visible = false);
		
	// 	overworld.Visible = true;
	// 	overworld.GetChildren().OfType<CanvasLayer>().ToList().ForEach(x => x.Visible = true);
	// 	overworld.GetChildren().OfType<ParallaxBackground>().ToList().ForEach(x => x.Visible = true);
		
	// 	shipEditor.ProcessMode = ProcessModeEnum.Disabled;
	// 	overworld.ProcessMode = ProcessModeEnum.Pausable;
	// }
	public static Vector2 CalculateVector(float angleRadians, float distance)
	{
		float x = (float)(MathF.Cos(angleRadians) * distance);
		float y = (float)(MathF.Sin(angleRadians) * distance);
		return new Vector2(x, y);
	}
	public static float CalculateLightingLevel()
	{
		var hour = HourOfDay;
		var minute = MinOfHour;
		var second = SecOfMin;
		
		// Convert the time to a float representing hours since midnight
		float time = hour + minute / 60.0f + second / 3600.0f;

		// Define the times for sunrise and sunset
		float sunrise = 6.0f;
		float sunset = 18.0f;

		// Calculate the lighting level
		float lightingLevel;

		if (time < sunrise || time >= sunset)
		{
			// It's night time, so the lighting level is 0
			lightingLevel = 0.0f;
		}
		else if (time >= sunrise && time <= sunset)
		{
			// It's day time, calculate the lighting level based on the time
			if (time < 12.0f)
			{
				// Morning (sunrise to noon): interpolate from 0 to 1
				lightingLevel = (time - sunrise) / (9.0f - sunrise);
			}
			else
			{
				// Afternoon (noon to sunset): interpolate from 1 to 0
				lightingLevel = (sunset - time) / (sunset - 16.0f);
			}
		}
		else
		{
			// Should never reach here due to the initial condition check
			lightingLevel = 0.0f;
		}
		
		lightingLevel = Math.Clamp(lightingLevel, 0, 1);

		return lightingLevel;
	}
	
	# region saving and loading
	public static ShipBlueprint LoadShipFromFile(string text)
	{
		using var file = FileAccess.Open($"user://ships/{text}.json", FileAccess.ModeFlags.Read);
		var json = file.GetLine();
		var shipState = System.Text.Json.JsonSerializer.Deserialize<ShipBlueprint>(json);

		return shipState;
	}
    
    //A bit hacky but whatever..
    public static WorldShipState ConvertToWorldShipState(ShipBlueprint shipBlueprint)
    {
		WorldShipState worldShipState = new();
		shipBlueprint.Components.ForEach(x =>
		{
			var packed_scene = GD.Load<PackedScene>(x.SceneFilePath);
			var newInstance = packed_scene.Instantiate<Component>();
			newInstance.Position = x.LocalPosition;
			newInstance.Rotation = x.LocalRotation;

			var componentData = (ComponentBase)newInstance.Data;
			var dataString = componentData.SaveState();

			worldShipState.ComponentStates.Add(dataString);
			worldShipState.Components.Add(x);
			
			newInstance.QueueFree();
		});

		return worldShipState;
    }
    
    //Returns a object that can be safely serialized to json and back.
    public static WorldShipState SerializeWorldShip(WorldShip worldShip)
    {
		List<ComponentBluePrint> bps = new();
		List<string> states = new();
    
		worldShip
			.GetChildren()
			.Where(x => x is Component)
			.Cast<Component>()
			.ToList()
			.ForEach(x =>
		{
			var bp = new ComponentBluePrint()
			{
				SceneFilePath = x.SceneFilePath,
				LocalPosition = x.Position,
				LocalRotation = x.Rotation
			};

			var state = x.Data.SaveState();

			bps.Add(bp);
			states.Add(state);
		});

		var shipState = new WorldShipState()
		{
			Name = "",
			Description = "",
			Components = bps,
			ComponentStates = states
		};

		return shipState;
    }
    
    public static WorldShip WorldShipFromWorldShipState(WorldShipState worldShipState)
    {
		var components = LoadWorldComponentsFromWorldShipState(worldShipState);
		var worldShip = GD.Load<PackedScene>("uid://buc2m62sgwj4x").Instantiate<WorldShip>();
		components.ForEach(x => 
		{
			worldShip.GetNode("ComponentsContainer").AddChild(x);
			worldShip.ShipData.Components.Add(x);
		});

		return worldShip;
    }
    
    public static List<Component> LoadWorldComponentsFromWorldShipState(WorldShipState worldShipState)
    {
		List<Component> components = new List<Component>();
        for (int i = 0; i < worldShipState.Components.Count; i++)
		{
			var bp = worldShipState.Components[i];
			var ws = worldShipState.ComponentStates[i];
			
			var packed_scene = GD.Load<PackedScene>(bp.SceneFilePath);
			var newInstance = packed_scene.Instantiate<Component>();

			newInstance.Position = bp.LocalPosition;
			newInstance.Rotation = bp.LocalRotation;
			
			//Here we load the actual saved data per ships component.
			newInstance.Data.LoadState(ws);

			components.Add(newInstance);
		}

		return components;
    }
    
    public static MapShip MapShipFromComponents(List<Component> components)
    {
		var packed_scene = GD.Load<PackedScene>("uid://npin8qld034a");
		MapShip s = packed_scene.Instantiate<MapShip>();
		components.ForEach(x => s.AddChild(x));
		s.ShipData.Components.AddRange(components);

		return s;
    }
    
    public static MapShip MapShipFromWorldShipState(WorldShipState worldShipState)
    {
		var components = LoadWorldComponentsFromWorldShipState(worldShipState);
		var mapShip = MapShipFromComponents(components);

		return mapShip;
    }
    
    //Load a mapship from a ship file
    public static MapShip MapShipFromShipFile(string shipName)
    {
		var bp = LoadShipFromFile(shipName);
		var ws = ConvertToWorldShipState(bp);
		var ms = MapShipFromWorldShipState(ws);
		ms.ShipData.Components.ForEach(x => x.Visible = false);
		//ms.ShipData.Components.ForEach(x => x.GlobalPosition = new Vector3(1000000, 1000000, 1000000));
		return ms;
    }
    
    public static MapShip MapShipFromShipBlueprint(ShipBlueprint bp)
    {
        var ws = ConvertToWorldShipState(bp);
		var ms = MapShipFromWorldShipState(ws);
		ms.ShipData.Components.ForEach(x => x.Visible = false);
		//ms.ShipData.Components.ForEach(x => x.GlobalPosition = new Vector3(1000000, 1000000, 1000000));
		return ms;
    }
    
	public static List<string> GetFilesInFolder(string folderPath)
    {
        List<string> files = new();

        using var dir = DirAccess.Open(folderPath);
        if (dir == null)
        {
            //GD.PrintErr($"Failed to open directory: {folderPath}");
            return files;
        }

        dir.ListDirBegin();

        while (true)
        {
            string fileName = dir.GetNext();
            if (fileName == "")
                break;

            // Skip special entries
            if (dir.CurrentIsDir())
                continue;

            files.Add($"{folderPath}/{fileName}");
        }

        dir.ListDirEnd();

        return files;
    }
	#endregion
	
	public static double Normalize(double value, double min, double max)
    {
        if (max == min)
        {
            throw new ArgumentException("Max and min cannot be the same value.");
        }
        return (value - min) / (max - min);
    }

	public static IEnumerable<double> GetRangeWithInterval(double start, double end, double interval)
	{
		double current = start;
		while (current <= end)
		{
			yield return current;
			current += interval;
		}
	}


	private static bool _inShipEditor { get; set; } = false;
	private static ShipEditor4 _editorInstance { get; set; } = null;
	public static void ToggleShipEditor()
	{
		_inShipEditor = !_inShipEditor;
		var tree = init.Init.GetTree();
		
		if(_inShipEditor)
		{
			tree.Paused = true;
			tree.Root.GetNode<Node2D>("Overworld").Visible = false;
			tree.Root.GetNode<CanvasLayer>("Overworld/UICanvas").Visible = false;
			tree.Root.GetNode<ParallaxBackground>("Overworld/BehindStuffParalax").Visible = false;
			tree.Root.GetNode<ParallaxBackground>("Overworld/InFrontOfStuffParalax").Visible = false;
			tree.Root.GetNode<ParallaxBackground>("Overworld/ParallaxBackground").Visible = false;

			var editorScene = GD.Load<PackedScene>("uid://cfr0v1088iske");
			var editorInstance = editorScene.Instantiate<ShipEditor4>();
			_editorInstance = editorInstance;
			tree.Root.AddChild(editorInstance);
			editorInstance.ProcessMode = ProcessModeEnum.Always;
		}
		else
		{
		    tree.Paused = false;
			tree.Root.GetNode<Node2D>("Overworld").Visible = true;
			tree.Root.GetNode<CanvasLayer>("Overworld/UICanvas").Visible = true;
			tree.Root.GetNode<ParallaxBackground>("Overworld/BehindStuffParalax").Visible = true;
			tree.Root.GetNode<ParallaxBackground>("Overworld/InFrontOfStuffParalax").Visible = true;
			tree.Root.GetNode<ParallaxBackground>("Overworld/ParallaxBackground").Visible = true;
			
			tree.Root.GetNode<Camera2D>("Overworld/CustomCamera2D").Enabled = true;
			tree.Root.GetNode<Camera2D>("Overworld/CustomCamera2D").MakeCurrent();
			
			_editorInstance.QueueFree();
		}
	}

    public static List<Vector2> GenerateVFormation2D(
        int totalPoints,
        int pointsPerV,
        float horizontalStep,
        float depthStep,
        float vOffset)
    {
        var positions = new List<Vector2>();
        int numVs = Mathf.CeilToInt((float)totalPoints / pointsPerV);

        int pointIndex = 0;

        for (int vIndex = 0; vIndex < numVs; vIndex++)
        {
            int pointsInThisV = Math.Min(pointsPerV, totalPoints - pointIndex);
            float vDepthOffset = -vIndex * vOffset;

            int half = pointsInThisV / 2;

            for (int i = 0; i < pointsInThisV; i++)
            {
                int sideIndex = i - half;

                // Compute V shape
                float y = Math.Abs(sideIndex) * depthStep + vDepthOffset;
                float x = sideIndex * horizontalStep;

                positions.Add(new Vector2(x, y));
                pointIndex++;

                if (pointIndex >= totalPoints)
                    break;
            }
        }

        return positions;
    }

    public static List<PackedScene> LoadAllScenes(string folderPath)
    {
    	List<PackedScene> LoadedScenes = new();
        using var dir = DirAccess.Open(folderPath);
        if (dir == null)
        {
            GD.PrintErr($"Could not open directory: {folderPath}");
            return new();
        }

        dir.ListDirBegin();

        while (true)
        {
            string fileName = dir.GetNext();
            if (string.IsNullOrEmpty(fileName))
                break;

            // Skip navigation directories
            if (dir.CurrentIsDir())
            {
                // Optionally recurse into subfolders:
                // LoadAllScenes($"{folderPath}/{fileName}");
                continue;
            }

            // Check for scene files
            if (fileName.EndsWith(".tscn") || fileName.EndsWith(".scn"))
            {
                string fullPath = $"{folderPath}/{fileName}";
                var scene = ResourceLoader.Load<PackedScene>(fullPath);
                if (scene != null)
                {
                    LoadedScenes.Add(scene);
                    GD.Print($"Loaded scene: {fullPath}");
                }
                else
                {
                    GD.PrintErr($"Failed to load scene: {fullPath}");
                }
            }
        }

        dir.ListDirEnd();
		return LoadedScenes;
    }
    
    public static float GetWorldWidth3D(Camera3D camera, float distance)
	{
		// 1. Get the Viewport's pixel size
		Vector2 viewportPixelSize = camera.GetViewport().GetVisibleRect().Size;
		
		// 2. Calculate Aspect Ratio
		float aspectRatio = viewportPixelSize.X / viewportPixelSize.Y;
		
		// 3. Get the Vertical FOV (default Godot FOV property)
		// Convert degrees to radians: Mathf.DegToRad(fov)
		float fovRad = Mathf.DegToRad(camera.Fov);
		
		// 4. Calculate the visible height in world units at the given distance D
		// World Height = 2 * D * tan(FOV/2)
		float worldHeight = 2.0f * distance * Mathf.Tan(fovRad / 2.0f);
		
		// 5. Calculate the visible width in world units
		float worldWidth = worldHeight * aspectRatio;
		
		return worldWidth;
	}
}

public enum Faction
{
	PLAYER,
	ENEMY
}

public enum RadiationType
{
	SearchRadar,
	FireControlRadar,
	MissileRadar,
	SearchRadarGround,
	FireControlRadarGround,
}

public enum MissileGuidanceType
{
	Radar, //Radar seeking
	Command, //Only needs target to be known
	Radiation, //Radiation seeking
	SacLos, //Needs line of sight
	SemiActive, //Needs radar illumination
}

public enum MapToolMode
{
	None,
	Pen, //Ruler
	Circle,
	Angle
}

public enum AircraftType
{
	None,
	A1,
	A2
}

public enum WorldShipDirectionBehavior
{
    FaceVelocity = 0,
    FaceTargetPosition = 1,
    FaceTargetObject = 2,
    HoldCustomDirection = 3,
}

public static class Extensions
{
	public static float MPixelToMeter(this float pixels) => globals.MapMeterPerPixel * pixels;
	public static float MMeterToPixel(this float meters) => globals.MapPixelPerMeter * meters;
	
	
	public static List<Node> GetAllDescendants(this Node root)
	{
		List<Node> descendants = new List<Node>();
		GetDescendantsRecursive(root, descendants);
		return descendants;
	}
	
		// Recursive function to traverse and collect nodes
	private static void GetDescendantsRecursive(Node node, List<Node> descendants)
	{
		foreach (Node child in node.GetChildren())
		{
			descendants.Add(child);
			GetDescendantsRecursive(child, descendants);
		}
	}
	
	private static void MimicPosRot(this Node2D node, Node2D To)
	{
		node.GlobalPosition = To.GlobalPosition;
		node.GlobalRotation = To.GlobalRotation;
	}
	
	private static void MimicPosRot(this Node3D node, Node3D To)
	{
		node.GlobalPosition = To.GlobalPosition;
		node.GlobalRotation = To.GlobalRotation;
	}
}