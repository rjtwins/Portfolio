using Godot;
using System;

public partial class EditorCamera : Camera3D
{

	[Export] public float Sensitivity { get; set; } = 1f; 
	float _yaw = 0.8f;
	float _pitch = 0.8f;
	float _orbit_distance = 25.0f;

	float max_size = 50;
	float min_size = 1;

	Vector3 _orbit_center = Vector3.Zero;
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
    {

    }

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	private void HandlePan(float delta)
	{
		Sensitivity = this.Size / GetViewport().GetVisibleRect().Size.Y;	
		// Sensitivity = this.Size / (this.max_size / 2); //TODO: fix this:
		float amount = 25 * Sensitivity;
		
		var movement = Vector3.Zero;
		
		// if(Input.IsActionPressed("camera_pan"))
		// {
		// 	var mouseVel = Input.GetLastMouseVelocity();
		// 	movement.X -= mouseVel.X;
		// 	movement.Y -= mouseVel.Y;
		// }
		
        if(Input.IsActionPressed("ui_right"))
        {
			movement.X += amount;
        }
		if(Input.IsActionPressed("ui_left"))
        {
            movement.X -= amount;
        }
		if(Input.IsActionPressed("ui_up"))
        {
            movement.Y += amount;
        }
		if(Input.IsActionPressed("ui_down"))
        {
            movement.Y -= amount;
        }

		if (movement.Length() == 0)
			return;

		movement = movement.Normalized().Rotated(Vector3.Up, _yaw);
		movement = movement * (float)delta;
		_orbit_center += movement;

		var dir = new Vector3(
			Mathf.Sin(_yaw) * Mathf.Cos(_pitch), 
			Mathf.Sin(_pitch), 
			Mathf.Cos(_yaw) * Mathf.Cos(_pitch)
		).Normalized();

		this.Position = _orbit_center + dir * _orbit_distance;
	}
	
	private void HandleZoom(float delta)
	{
	
	    if(Input.IsActionJustPressed("camera_zoom_in"))
	    {
			this.Size = Math.Clamp(this.Size + 1, min_size, max_size);
	    }
	    if(Input.IsActionJustPressed("camera_zoom_out"))
	    {
			this.Size = Math.Clamp(this.Size - 1, min_size, max_size);
	    }
	}
	
	
	public override void _Process(double delta)
    {
		HandlePan((float)delta);
		
		HandleZoom((float)delta);
    }
}
