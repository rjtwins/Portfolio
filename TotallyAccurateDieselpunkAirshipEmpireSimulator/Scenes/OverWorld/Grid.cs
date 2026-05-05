using Godot;

public partial class Grid : Node2D
{
	[Export]
	public Color Color {get; set;} = new Color("black");
	[Export]
	public int GridSize {get; set;} = 50;
	[Export]
	public Camera2D MapCamera2D {get; set;}
	[Export]
	public int ScreenWidth = 1;
	[Export]
	public int MaxSquares = 50;
	
	private Camera2D _camera;
	private Viewport _viewport;
	private Vector2 _gridSize;
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		_viewport = GetViewport();
		_camera = MapCamera2D;
		_gridSize = Vector2.One * GridSize;
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		QueueRedraw();
	}

	public override void _Draw()
	{
		var size = GetViewportRect().Size * (Vector2.One / _camera.Zoom);
		
		var cam = _camera.GlobalPosition;
		
		// GD.Print(_camera.Zoom);
		
		var minx = (int)((cam.X - size.X) / GridSize) - 1;
		var maxx = (int)((size.X + cam.X) / GridSize) + 1;
		
		var miny = (int)((cam.Y - size.Y) / GridSize) - 1;
		var maxy = (int)((size.Y + cam.Y) / GridSize) + 1;
		
		var nrX = maxx - minx;
		var nrY = maxy - miny;
		
		if(nrX > MaxSquares || nrY > MaxSquares)
			return;
		
		// GD.Print($"-x:{minx}+x:{maxx}-y{miny}+y{maxy}");
		
		var lineWidth = ((float)ScreenWidth) / _camera.Zoom.X;
		
		for (int i = minx; i <= maxx; i++)
		{
			DrawLine(new Vector2(i * GridSize, cam.Y + size.Y + 100), new Vector2(i * GridSize, cam.Y - size.Y - 100), Color, lineWidth);
		}

		for (int i = miny; i <= maxy; i++)
		{
			DrawLine(new Vector2(cam.X + size.X + 100, i * GridSize), new Vector2(cam.X - size.X - 100, i * GridSize), Color, lineWidth);
		}
	}
}
