using Godot;

public partial class SoftwareCurser : Sprite2D
{
	[Export]
	public Texture2D EmptyCurser {get; set; } = null;
	
	[Export]
	public Area2D ShadedArea {get; set;} = null;
	
	[Export]
	public Area2D OwnArea {get; set;} = null;
	
	[Export]
	public Control MapOverlay {get; set;} = null;
	
	private bool _insideShadedArea = false;
	


	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		//Input.SetCustomMouseCursor(EmptyCurser, Input.CursorShape.Arrow);
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		//GlobalPosition = GetGlobalMousePosition();
		if(ShadedArea.OverlapsArea(OwnArea))
		{
			ZIndex = -1;
			//TopLevel = false;
			//GD.Print("In Port");
			_insideShadedArea = true;
			MapOverlay.MouseFilter = Control.MouseFilterEnum.Ignore;
			GetViewport()?.GetCamera2D()?.Set("zoom_factor", 1.25);

		}
		else
		{
			ZIndex = 99;
			//TopLevel = true;
			//GD.Print("Out Port");
			_insideShadedArea = false;
			MapOverlay.MouseFilter = Control.MouseFilterEnum.Stop;
			GetViewport()?.GetCamera2D()?.Set("zoom_factor", 1);
		}

	}
}
