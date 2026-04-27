using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public partial class MissilePanel : Control
{
	[Export] ItemList MissileList { get; set; }
	[Export] ItemList StrikeQueue { get; set; }
	[Export] ItemList LauncherList { get; set; }
	[Export] ItemList MissileInfo { get; set; }
	
	[Export] Button AddButton { get; set; }
	[Export] Button RemoveButton { get; set; }
	[Export] Button LaunchButton { get; set; }
	[Export] Button CloseButton { get; set; }

	private int _selectedMissileIndex => MissileList.GetSelectedItems().FirstOrDefault();
	private string _selectedMissileType => AllMissileTypes[_selectedMissileIndex];

	private List<ShipData> _launchers = new List<ShipData>();
	private int _selectedLauncherIndex => LauncherList.GetSelectedItems().FirstOrDefault();
	private ShipData _selectedLauncher => _launchers[_selectedLauncherIndex];

	private bool _isFiringSalvo => _salvoCount > 0;
	private int _salvoCount { get; set; } = 0;
	
	private FleetInfo _fleetInfo;
	private Dictionary<ShipData, List<MissileData>> StoresPerShip = new Dictionary<ShipData, List<MissileData>>();
	private List<MissileData> AllStores => StoresPerShip.Values.SelectMany(x => x.ToList()).ToList();
	private List<string> AllMissileTypes => AllStores.Select(x => x.MissileIdentifier).Distinct().ToList();
	private List<(string, ShipData)> MissileQueue = new();
		
	public void Setup(FleetInfo fleetInfo)
	{
		_fleetInfo = fleetInfo;
		UpdateStores();
		UpdateUI();
	}

    private void UpdateUI()
    {
    	// if(!MissileList.IsAnythingSelected())
    	// {
		// 	LauncherList.Clear();
		// 	MissileInfo.Clear();
    	// }
    
		MissileList.Clear();
		
		AllMissileTypes.ToList().ForEach(x =>
		{
			var nr = AllStores.Count(y => y.MissileIdentifier == x);
			MissileList.AddItem($"#{nr} - {x}");
		});
		
		// if(MissileList.IsAnythingSelected())
		// {
		// 	//Fill possible launchers:
		// 	_launchers = StoresPerShip
		// 		.Where(x => x.Value
		// 			.Select(y => y.MissileIdentifier)
		// 			.Contains(_selectedMissileType))
		// 		.Select(x => x.Key)
		// 		.ToList();

		// 	_launchers.ForEach(x =>
		// 	{
		// 		LauncherList.AddItem(x.Name);
		// 	});

		// 	//MissileInfo:
		// 	var missileData = AllStores.FirstOrDefault(x => x.MissileIdentifier == _selectedMissileType);
		// 	MissileInfo.AddItem($"ID: {missileData.MissileIdentifier}");
		// 	MissileInfo.AddItem($"Range: {missileData.FlyTime}");
		// 	MissileInfo.AddItem($"Guidance: {missileData.MissileType}");
		// };

		StrikeQueue.Clear();

		MissileQueue.ForEach(x =>
		{
			StrikeQueue.AddItem($"{x.Item1} -> {x.Item2}");
		});
    }

    private void UpdateStores()
	{
	    StoresPerShip = _fleetInfo.Ships.ToDictionary(x => x.ShipData, x => x.ShipData.GetMissileStores());
	}
	
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
    {
		AddButton.Pressed += AddButtonPressed;
		RemoveButton.Pressed += RemoveButtonPressed;
		LaunchButton.Pressed += LaunchButtonPressed;
		CloseButton.Pressed += ClosedButtonPressed;
		MissileList.ItemSelected += MissileListItemSelected;
		this.Hidden += PanelHidden;
    }
    
	// Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta)
    {
        if(MissileQueue.Count == 0 || _isFiringSalvo)
        {
			LaunchButton.Disabled = true;
        }
        else
        {
			LaunchButton.Disabled = false;
        }

		HandleClickLaunch();
    }
    
    private void HandleClickLaunch()
    {
		if (!Input.IsActionJustReleased("right_click") || !globals.ReadyToLaunchMissile)
			return;
			
		var mouseFollower = OverworldMouseFollower.Instance;
		if (mouseFollower.IsOverUI())
			return;
		
		var targetLocation = OverworldMouseFollower.Instance.GlobalPosition;
		if(mouseFollower.HasFleetOrSettlementUnderMouse())
		{
			//Give the missile time to activate when we've clicked on an entity.
			//TODO: use missile tracking logic here
			targetLocation = mouseFollower.GetFleetOrSettlementUnderMouse().GlobalPosition.MoveToward(_fleetInfo.GetParent<Node2D>().GlobalPosition, 100);
		}

		_salvoCount = MissileQueue.Count();

		var timeIndex = 0.1f;
		
		MissileQueue.ToList().ForEach(x =>
		{
			MissileQueue.Remove(x);				
			FireMissileInSalve(timeIndex, globals.MissileDictionary[x.Item1], targetLocation);
			timeIndex += 5;
		});

		globals.ReadyToLaunchMissile = false;

		UpdateStores();
		UpdateUI();
    }
    
    private void FireMissileInSalve(float timeIndex, MissileData d, Vector2 target)
    {
    	//TODO: Set active point
		var timer = new Godot.Timer();
		timer.Timeout += () =>
		{
			var packed_scene = GD.Load<PackedScene>("uid://ccigvtdo2qfu3");
			MapMissile m = packed_scene.Instantiate<MapMissile>();
			m.MissileData = d;
			OverWorld.Instance.AddChild(m);
			m.GlobalPosition = _fleetInfo.GetParent<Node2D>().GlobalPosition;
			m.LookAt(target);
			timer.QueueFree();

			_salvoCount -= 1;
			UpdateUI();
		};
		timer.OneShot = true;
		timer.WaitTime = timeIndex;
		AddChild(timer);
		timer.Start();
    }

    private void PanelHidden()
    {
		globals.ReadyToLaunchMissile = false;
    }


    private void MissileListItemSelected(long index)
    {
		MissileInfo.Clear();
		LauncherList.Clear();
		
		if(MissileList.IsAnythingSelected())
		{
			//Fill possible launchers:
			_launchers = StoresPerShip
				.Where(x => x.Value
					.Select(y => y.MissileIdentifier)
					.Contains(_selectedMissileType))
				.Select(x => x.Key)
				.ToList();

			_launchers.ForEach(x =>
			{
				LauncherList.AddItem(x.Name);
			});

			//MissileInfo:
			var missileData = AllStores.FirstOrDefault(x => x.MissileIdentifier == _selectedMissileType);
			MissileInfo.AddItem($"ID: {missileData.MissileIdentifier}", selectable: false);
			MissileInfo.AddItem($"Range: {missileData.FlyTime}", selectable: false);
			MissileInfo.AddItem($"Guidance: {missileData.MissileType}", selectable: false);
		};
    }


    private void ClosedButtonPressed()
    {
		MissileQueue.ForEach(x =>
		{
			x.Item2.AddMissileToStore(x.Item1);
		});

		MissileQueue.Clear();
		
		this.Hide();
    }


    private void LaunchButtonPressed()
    {
		if (MissileQueue.Count == 0)
			return;

		globals.ReadyToLaunchMissile = true;
    }

    private void RemoveButtonPressed()
    {
		globals.ReadyToLaunchMissile = false;

		if (!StrikeQueue.IsAnythingSelected())
			return;

		var selectedIndex = StrikeQueue.GetSelectedItems().First();
		var selectedItem = MissileQueue[selectedIndex];
		selectedItem.Item2.AddMissileToStore(selectedItem.Item1);

		StrikeQueue.RemoveItem(selectedIndex);
		MissileQueue.Remove(selectedItem);
		
		UpdateUI();
		
		if(StrikeQueue.ItemCount > 0)
			StrikeQueue.Select(StrikeQueue.ItemCount - 1);
    }

    private void AddButtonPressed()
    {
    	globals.ReadyToLaunchMissile = false;
    	
		if (!MissileList.IsAnythingSelected())
			return;
		
		//If no ship is selected, select the first available ship.
		var shipData = LauncherList.IsAnythingSelected() ? _selectedLauncher : null;
		shipData = shipData == null ? StoresPerShip.FirstOrDefault(x => x.Value.Select(y => y.MissileIdentifier).Contains(_selectedMissileType)).Key : shipData;

		shipData.RemoveMissileFromStore(_selectedMissileType);

		MissileQueue.Add((_selectedMissileType, shipData));

		var selectedIndex = _selectedMissileIndex;
		
		UpdateStores();
		UpdateUI();

		if (MissileList.ItemCount > 0)
			MissileList.Select(selectedIndex);
    }
}
