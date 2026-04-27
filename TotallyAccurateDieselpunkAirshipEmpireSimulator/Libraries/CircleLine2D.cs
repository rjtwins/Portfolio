using Godot;
public partial class CircleLine2D : Line2D
{
	[Export]
	public int Segments {get; set;} = 32;
	[Export]
	public float Radius {get; set;} = 50.0f;
	[Export]
	public int ScreenWidth {get; set;} = 2;
	[Export]
	public bool MaintainPixelWidth {get; set;} = true;
	[Export]
	public bool MaintainSize {get; set;}= false;
	
	private Camera2D _camera;
	
	[Export]
	public float WorldRadius {get; set;} = 10f;

	public override void _Ready()
	{
		Closed = true;
		Redraw();
	}
	
	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		UpdateLineWidth();
	}
	
	public void Redraw()
	{
		var temp = new Vector2[Segments];
		for (int i = 1; i < Segments + 1; i++)
		{
			float angle = (float)i / (float)Segments * Mathf.Tau;
			var x = Mathf.Cos(angle);
			var y = Mathf.Sin(angle);
			var vec = new Vector2(x, y);
			vec *= Radius;
			temp[i-1] = vec;
		}
		ClearPoints();
		Points = temp;
	}
	
	private void UpdateLineWidth()
	{
		_camera = GetViewport().GetCamera2D();
		if(_camera == null)
			return;
			
		// Adjust the line width based on the camera's zoom level
		if(MaintainPixelWidth)
			Width = ((float)ScreenWidth) / _camera.Zoom.X;
			
		if(MaintainSize)
			Radius = WorldRadius / _camera.Zoom.X;
		
		Redraw();
	}
}
