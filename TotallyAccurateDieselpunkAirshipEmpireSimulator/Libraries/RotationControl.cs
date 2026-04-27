using Godot;

public partial class RotationControl : TextureRect
{
	[Signal]
	public delegate void OnRotatedEventHandler(float NewRotationDegrees);
	
	
	[Export]
	Timer TimeoutTimer { get; set; }
	
	[Export]
	public float MinRotation { get; set; } = 0f;
	
	[Export]
	public float MaxRotation { get; set; } = 360f;
	
	[Export]
	public float RotationPerClick { get; set; } = 5f;
	
	[Export]
	public float ClickTimeout = 0.1f;
	
	private bool _mouseOver = false;
	private bool _onTimeout = false;
	
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		MouseEntered += () => _mouseOver = true;
		MouseExited += () => _mouseOver = false;
		PivotOffset = Size / 2;
		TimeoutTimer.Timeout += () => _onTimeout = false;
		TimeoutTimer.WaitTime = ClickTimeout;
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	// public override void _Process(double delta)
	// {
		
	// }

	//I got a bit lazy here and duplicated a lot of code.
	public override void _GuiInput(InputEvent @event)
	{	
		if(_onTimeout)
			return;
			
		if(@event is InputEventMouseButton mouseButton)
			if(mouseButton.ButtonIndex == MouseButton.WheelUp)
			{
				RotationDegrees -= RotationPerClick;
				RotationDegrees = Mathf.Clamp(RotationDegrees, MinRotation, MaxRotation);
				_onTimeout = true;
				TimeoutTimer.Start();
				EmitSignal("OnRotated", RotationDegrees);
			}
			else if (mouseButton.ButtonIndex == MouseButton.WheelDown)
			{
				RotationDegrees += RotationPerClick;
				RotationDegrees = Mathf.Clamp(RotationDegrees, MinRotation, MaxRotation);
				_onTimeout = true;
				TimeoutTimer.Start();
				EmitSignal("OnRotated", RotationDegrees);
			}
	}
}
