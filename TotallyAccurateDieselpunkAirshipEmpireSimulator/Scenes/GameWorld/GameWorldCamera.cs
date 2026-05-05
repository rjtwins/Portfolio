using Godot;
using System;

public partial class GameWorldCamera : Camera2D
{
	[Export] float ZoomSpeed = 1.2f;
	[Export] float KeyPanSpeed = 5f;
	
	[Export] float MaxZoom = 0.4f;
	[Export] float MinZoom = 1e-2f;
	
	private Vector2 panMoveAmount  => Vector2.One * KeyPanSpeed * 1/Zoom;
	private Vector2 zoomTarget;
	
	private Vector2 dragStartMousePos;
	private Vector2 dragStartCameraPos;
	private bool isDragging = false;
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		zoomTarget = Zoom;
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{					
		zoomTarget = Zoom;
		SimpleZoom(delta);
		SimplePan();
		ClickAndDrag();
		
		GlobalPosition = GlobalPosition.Snapped(Vector2.One);
		
		//GD.Print(GetLocalMousePosition());
	}
	
	private void ClickAndDrag()
	{
		if (!isDragging && Input.IsActionJustPressed("camera_pan"))
		{
			dragStartMousePos = GetViewport().GetMousePosition();
			dragStartCameraPos = Position;
			isDragging = true;
		}

		if (isDragging && Input.IsActionJustReleased("camera_pan"))
		{
			isDragging = false;
		}

		if (isDragging)
		{
			Vector2 moveVector = GetViewport().GetMousePosition() - dragStartMousePos;
			Position = dragStartCameraPos - moveVector * (1 / Zoom.X);
		}
	}
	
	// private void SimpleZoom(double delta)
	// {
	// 	if(Input.IsActionJustPressed("camera_zoom_in"))
	// 		Zoom *= ZoomSpeed;
	// 	if(Input.IsActionJustPressed("camera_zoom_out"))
	// 		Zoom *= 1/ZoomSpeed;
	// }
	
	private void SimpleZoom(double delta)
	{
		if(OverworldMouseFollower.Instance?.IsOverUI() ?? false)
			return;
			
		if (Input.IsActionJustPressed("camera_zoom_in", true) || Input.IsActionJustPressed("camera_zoom_out", true))
		{
			Vector2 currentZoom = Zoom;
			
			if (Input.IsActionJustPressed("camera_zoom_in", true))
				Zoom *= ZoomSpeed;
			if (Input.IsActionJustPressed("camera_zoom_out", true))
				Zoom /= ZoomSpeed;
				
			Zoom = Zoom.Clamp(Vector2.One * MinZoom, Vector2.One * MaxZoom);
			Zoom = Zoom.Snapped(Vector2.One * 0.01f);
			Zoom = Zoom = new Vector2(MathF.Round(Zoom.X, 2), MathF.Round(Zoom.Y, 2));
			var zoomChange = Zoom - currentZoom;
			
			GlobalPosition += (GetLocalMousePosition() * zoomChange) * 1/Zoom;
			
			GD.Print(Zoom);
		}
	}
	
	private void SimplePan()
	{
		if(Input.IsActionPressed("camera_move_right"))
			GlobalPosition += Vector2.Right * panMoveAmount;
		if(Input.IsActionPressed("camera_move_left"))
			GlobalPosition += Vector2.Left * panMoveAmount;
		if(Input.IsActionPressed("camera_move_up"))
			GlobalPosition += Vector2.Up * panMoveAmount;
		if(Input.IsActionPressed("camera_move_down"))
			GlobalPosition += Vector2.Down * panMoveAmount;
	}
}
