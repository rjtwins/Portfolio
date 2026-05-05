using Godot;

public partial class MapToolManger : Node2D
{
	[Export] public PackedScene PenLineScene {get; set;}
	[Export] public PackedScene PenCircle {get; set;}
	[Export] public PackedScene Protractor {get; set;}

	private PenLine penLine;
	private PenCircle penCircle;
	private Node2D protractor;
	private Node2D globalProtractor;
	private Vector2 MouseStartPos = Vector2.Zero;
	private bool DrawingInProgress = false;
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		MouseStartPos = GetGlobalMousePosition();
	}
	
	public void MapToolModeChanged()
	{
		if(globals.MapToolMode == MapToolMode.None)
			return;
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		//GD.Print(GetGlobalMousePosition());
		if(Input.IsActionJustReleased("right_click"))
			globals.MapToolMode = MapToolMode.None;
		
		if(globals.MapToolMode == MapToolMode.None)
			return;
			
		if(Input.IsActionJustPressed("click"))
		{
			MouseStartPos = GetGlobalMousePosition();
			DrawingInProgress = true;
			
			switch (globals.MapToolMode)
			{
				case MapToolMode.Pen:
					penLine = PenLineScene.Instantiate<PenLine>();
					AddChild(penLine);
					penLine.GlobalPosition = Vector2.Zero;
					penLine.Points = new Vector2[] { MouseStartPos };
					break;
				case MapToolMode.Circle:
					penCircle = PenCircle.Instantiate<PenCircle>();
					AddChild(penCircle);
					penCircle.GlobalPosition = MouseStartPos;
					penCircle.StartPoint = MouseStartPos;
					break;
				case MapToolMode.Angle:
					if(globalProtractor == null)
					{
						globalProtractor = Protractor.Instantiate<Node2D>();
						AddChild(globalProtractor);
					}
					protractor = globalProtractor;
					break;
				default:
					break;
			}
		}
		
		if(Input.IsActionJustReleased("click") && DrawingInProgress)
		{
			DrawingInProgress = false;
			
			penLine?.SetPlaced();
			penLine = null;
			
			penCircle?.SetPlaced();
			penCircle = null;
			
			protractor = null;
		}
		
		if(Input.IsActionJustPressed("protractor_scale_up", true) && globals.MapToolMode == MapToolMode.Angle && protractor != null)
			protractor.Scale *= 1.1f;
			
		if(Input.IsActionJustPressed("protractor_scale_down", true) && globals.MapToolMode == MapToolMode.Angle && protractor != null)
			protractor.Scale *= 0.9f;
			
		if(DrawingInProgress)
		{
			switch (globals.MapToolMode)
			{
				case MapToolMode.None:
					break;
				case MapToolMode.Pen:
					penLine.Points = new Vector2[] { MouseStartPos, GetGlobalMousePosition()};
					break;
				case MapToolMode.Circle:
					penCircle.EndPoint = GetGlobalMousePosition();
					break;
				case MapToolMode.Angle:
					protractor.GlobalPosition = GetGlobalMousePosition();
					break;
				default:
					break;
			}
		}
	}
}
