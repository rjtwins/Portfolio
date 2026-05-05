using Godot;
using System.Collections.Generic;
using System.IO;
using System.Linq;

public partial class SavedShipList : ScrollContainer
{
	ButtonGroup buttonGroup;
	VBoxContainer subContainer => GetChild(0) as VBoxContainer;
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		buttonGroup = new();
		Update();
	}
	
	public void Update()
	{
		subContainer.GetChildren().ToList().ForEach(x => x.QueueFree());
		
		var dir = DirAccess.Open("user://ships");
		if(dir == null)
		{
			DirAccess.Open("user://").MakeDir("ships");
			dir = DirAccess.Open("user://ships");
		}
		
		if(dir == null)
			throw new FileLoadException();
			
		var files = dir.GetFiles().Where(x => x.EndsWith("_ship.tscn")).ToList();
		
		files.ForEach(x => 
		{
			var button = new Button();
			button.ButtonGroup = buttonGroup;
			button.Text = x.Replace("_ship.tscn", "");
			button.ToggleMode = true;
			subContainer.AddChild(button);
		});
	}

	public string GetSelectedShipName()
	{
		var selectedButton = subContainer.GetChildren().OfType<Button>().Where(x => x.ButtonPressed).FirstOrDefault();
		return selectedButton?.Text ?? string.Empty;
	}
	
	public List<string> GetAllShipNames()
	{
		return subContainer.GetChildren().OfType<Button>().Select(x => x.Text).ToList();
	}
}
