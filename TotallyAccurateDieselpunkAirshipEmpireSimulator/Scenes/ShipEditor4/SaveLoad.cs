using Godot;
using System;
using System.Linq;

public partial class SaveLoad : Control
{
	[Export] public LineEdit LineEdit { get; set; }
	[Export] public ShipEditor4 ShipEditor4 { get; set; }
	[Export] public ItemList ShipList { get; set; }
	
	public void _on_close_pressed()
	{
		Close();
	}
	
	public void _on_save_pressed()
	{
		ShipEditor4._on_button_save_pressed(LineEdit.Text);
		UpdateUI();
	}
	
	public void _on_load_pressed()
	{
		ShipEditor4._on_button_load_pressed(LineEdit.Text);
		Close();
	}
	
	public void _on_item_list_item_selected(int index)
	{
	    var shipFiles = ShipEditor4.GetSavedShips();
		var shipFile = shipFiles[index];

		LineEdit.Text = shipFile.Split(@"/").Last().Split(".").First();
	}
	
	public void Close()
	{
		this.GetParent<ColorRect>().Visible = false;
	}
	
	public void Open()
	{
		this.GetParent<ColorRect>().Visible = true;
		UpdateUI();
	}


	private void UpdateUI()
	{
		var shipFiles = ShipEditor4.GetSavedShips();

		//Clear
		ShipList.Clear();

		shipFiles.ForEach(x =>
		{
			var label = new Label();
			ShipList.AddItem(x);
		});		
	}

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
    {
        
    }

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}
}
