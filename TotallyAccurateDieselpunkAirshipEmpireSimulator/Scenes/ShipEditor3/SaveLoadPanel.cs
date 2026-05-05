using System;
using Godot;

public partial class SaveLoadPanel : Control
{
	[Export] Button saveButton;
	[Export] Button loadButton;
	[Export] Button deleteButton;
	[Export] public SavedShipList shipList;
	[Export] ConfirmationDialog confirmationDialog;
	[Export] LineEdit shipNameEdit;
	[Export] FileDialog fileDialog;

	public override void _Ready()
	{
		base._Ready();
		saveButton.Pressed += Save;
		loadButton.Pressed += Load;
		deleteButton.Pressed += Delete;
		
		VisibilityChanged += Update;
		shipNameEdit.TextChanged += ShipNameUpdated;
			
		fileDialog.FileSelected += (string path) =>
		{
			//ShipEditor3.Instance.Save(true, "", path);
			shipList?.Update();
		};
	}

    private void ShipNameUpdated(string newText)
	{
		//ShipEditor3.Instance.EditorShip.ShipName = newText;
	}

	public override void _Input(InputEvent @event)
	{
		if(Input.IsActionJustReleased("ui_cancel"))
		{
			Visible = false;
		}
		if(Input.IsActionJustReleased("ui_accept"))
		{
			Load();
		}
	}

	private void Update()
	{
		//shipNameEdit.Text = ShipEditor3.Instance.EditorShip.ShipName;
	}

	private void Load()
	{
		var shipName = shipList.GetSelectedShipName();
		//ShipEditor3.Instance.Load(shipName);
	}
	
	private void Delete()
	{
		var shipName = shipList.GetSelectedShipName();
		//ShipEditor3.Instance.DeleteSavedShip(shipName);
		shipList.Update();
	}

	private void Save()
	{	
		// if(Input.IsKeyPressed(Key.Ctrl))
		// {
		// 	fileDialog.Popup();
		// 	return;
		// }
		
		// shipList.Update();
		
		// var currentShips = shipList.GetAllShipNames();
		
		// if(currentShips.Contains(ShipEditor3.Instance.EditorShip.ShipName))
		// {
		// 	confirmationDialog.DialogText = "Overwrite " + ShipEditor3.Instance.EditorShip.ShipName + "?";
		// 	confirmationDialog.Confirmed += () => 
		// 	{
		// 		ShipEditor3.Instance.Save(true, ShipEditor3.Instance.EditorShip.ShipName);
		// 		shipList.Update();
		// 	};
			
		// 	confirmationDialog.Popup();

		// 	return;
		// }
		
		// ShipEditor3.Instance.Save();
		// shipList.Update();
	}
}
