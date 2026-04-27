using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using Godot;

public partial class OrbitalCamera : Camera3D
{
    public static OrbitalCamera Instance;

    // =========================
    // Camera movement settings
    // =========================
    [ExportCategory("Camera movement")]
    [Export] public float CameraSpeed = 20.0f;
    [Export] public float CameraZoomSpeed = 20.0f;
    [Export] public float CameraZoomMin = 10.0f;
    [Export] public float CameraZoomMax = 50.0f;

    // =========================
    // Edge scrolling settings
    // =========================
    [ExportCategory("Edge scrolling")]
    [Export] public float EdgeScrollMargin = 20.0f;
    [Export] public float EdgeScrollSpeed = 15.0f;

    // =========================
    // Rotation (MMB) settings
    // =========================
    [ExportCategory("Rotation")]
    [Export] public float YawSensitivity = 0.50f;
    [Export] public float PitchSensitivity = 0.18f;
    [Export] public float MaxStepDeg = 3.0f;
    [Export] public float PitchMinDeg = 10.0f;
    [Export] public float PitchMaxDeg = 80.0f;
    [Export] public bool CaptureMouseOnMMB = false;
    [Export] public bool RotatingEnabled = true;

    // =========================
    // Runtime state
    // =========================
    [Export] public Vector3 OrbitCenter = Vector3.Zero;
    [Export] public float OrbitDistance = 25.0f;

    [Export] public float CurrentHeight = 20.0f;
    [Export] public float OrbitRadius = 20.0f;

    private bool _isMMBRotating = false;
    [Export] public float Yaw = 0.0f;
    [Export] public float Pitch = 0.8f; // radians

    private List<Node3D> trackedObjects = new();

    public override void _Ready()
    {
        float pmin = Mathf.DegToRad(PitchMinDeg);
        float pmax = Mathf.DegToRad(PitchMaxDeg);
        Pitch = Mathf.Clamp(Pitch, pmin, pmax);
        UpdateCameraPosition();

        Instance = this;
    }

    public override void _Process(double delta)
    {
        Vector3 movement = Vector3.Zero;

        // Keyboard movement
        if (Input.IsActionPressed("ui_right"))
            movement.X += 1;
        if (Input.IsActionPressed("ui_left"))
            movement.X -= 1;
        if (Input.IsActionPressed("ui_up"))
            movement.Z -= 1;
        if (Input.IsActionPressed("ui_down"))
            movement.Z += 1;

        // Shift boost
        float speedMultiplier = Input.IsActionPressed("ui_shift") ? 5.0f : 1.0f;

        if (movement.Length() > 0.0f && !GameWorldTest.MouseInUI)
        {
            trackedObjects.Clear();
            movement = movement.Normalized().Rotated(Vector3.Up, Yaw);
            OrbitCenter += movement * CameraSpeed * speedMultiplier * (float)delta;
            UpdateCameraPosition();
        }
        
        if(trackedObjects.Count > 0)
        {
            var x = trackedObjects.Select(x => x.GlobalPosition).Average(x => x.X);
            var y = trackedObjects.Select(x => x.GlobalPosition).Average(x => x.Y);
            var z = trackedObjects.Select(x => x.GlobalPosition).Average(x => x.Z);
            OrbitCenter = new Vector3(x, y, z);
            UpdateCameraPosition();
        }
    }

    public override void _UnhandledInput(InputEvent @event)
    {
        if (GameWorldTest.MouseInUI)
            return;
    
		float dt = (float)GetProcessDeltaTime();
		
		if (@event is InputEventMouseButton mb)
		{

			if (mb.ButtonIndex == MouseButton.WheelUp)
			{
                if (Input.IsActionPressed("ui_shift"))
                    OrbitCenter.Y = Math.Max(OrbitCenter.Y - CameraZoomSpeed * dt, 0);
                else
                    OrbitDistance = Mathf.Max(CameraZoomMin, OrbitDistance - CameraZoomSpeed * dt);
				UpdateCameraPosition();
			}
			else if (mb.ButtonIndex == MouseButton.WheelDown)
			{
                if (Input.IsActionPressed("ui_shift"))
                    OrbitCenter.Y = Math.Min(OrbitCenter.Y + CameraZoomSpeed * dt, 500);
                else
				    OrbitDistance = Mathf.Min(CameraZoomMax, OrbitDistance + CameraZoomSpeed * dt);
				    
				UpdateCameraPosition();
			}

			if (mb.ButtonIndex == MouseButton.Middle)
			{
				_isMMBRotating = mb.Pressed;

				if (CaptureMouseOnMMB)
				{
					Input.MouseMode = mb.Pressed
						? Input.MouseModeEnum.Captured
						: Input.MouseModeEnum.Visible;
				}
			}
		}
		else if (@event is InputEventMouseMotion mm && _isMMBRotating)
		{
			Vector2 vp = GetViewport().GetVisibleRect().Size;
			float vmin = Mathf.Min(vp.X, vp.Y);

			float sixtyFps = 60.0f * dt;

			float dx = (mm.Relative.X / vmin) * YawSensitivity * Mathf.Tau * sixtyFps;
			float dy = (mm.Relative.Y / vmin) * PitchSensitivity * Mathf.Tau * sixtyFps;

			float maxStep = Mathf.DegToRad(MaxStepDeg);
			dx = Mathf.Clamp(dx, -maxStep, maxStep);
			dy = Mathf.Clamp(dy, -maxStep, maxStep);

			Yaw -= dx;
			Pitch += RotatingEnabled ? dy : 0;

			float pmin = Mathf.DegToRad(PitchMinDeg);
			float pmax = Mathf.DegToRad(PitchMaxDeg);
			Pitch = Mathf.Clamp(Pitch, pmin, pmax);

			UpdateCameraPosition();
		}
    }

    // =========================
    // Helpers
    // =========================
    public void UpdateCameraPosition()
    {
        Vector3 dir = new Vector3(
            Mathf.Sin(Yaw) * Mathf.Cos(Pitch),
            Mathf.Sin(Pitch),
            Mathf.Cos(Yaw) * Mathf.Cos(Pitch)
        ).Normalized();

        GlobalPosition = OrbitCenter + dir * OrbitDistance;
        LookAt(OrbitCenter, Vector3.Up);

        CurrentHeight = OrbitDistance * Mathf.Sin(Pitch);
        OrbitRadius = OrbitDistance * Mathf.Cos(Pitch);
    }

    internal void TrackObjects(IEnumerable<Node3D> selectedNodes)
    {
        trackedObjects.Clear();
        trackedObjects.AddRange(selectedNodes);
    }

}
