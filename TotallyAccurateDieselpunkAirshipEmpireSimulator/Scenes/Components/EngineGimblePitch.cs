using Godot;
using System;

public partial class EngineGimblePitch : Node3D
{
	[Export] EngineComponent EngineComponent { get; set; }
	[Export] public float CurrentPitch { get; set; } = 0f;
	[Export] public float MaxPitch { get; set; } = MathF.PI;
	[Export] public float MinPitch { get; set; } = -MathF.PI;
	
	public override void _Ready()
	{
	}

	public override void _Process(double delta)
    {
		RotatePitchOnly(delta);
    }
    
	public void RotatePitchOnly(double delta)
	{
		// 1. Convert the world direction to the node's local coordinate system.
		var worldDirection = EngineComponent.DesiredThrustDirection.Normalized();
		var localDirection = GlobalTransform.Basis.Inverse() * worldDirection;

		// 2. We only care about the Y rotation (pitch). 
		// We want to find the angle difference between the current local forward (Vector3.Back) 
		// and the direction we want to face, but ONLY on the X/Z plane (pitch).
		
		// Flatten the vectors onto the XZ plane to ignore yaw (Y-axis rotation)
		// The object's current forward in its local space is Vector3.Back or -Vector3.Z
		Vector3 currentLocalForwardXZ = Vector3.Back.Normalized(); 
		// The desired direction, clamped to XZ plane, using the original Z component for directionality
		Vector3 desiredLocalForwardXZ = new Vector3(localDirection.X, 0, localDirection.Z).Normalized(); 

		// Calculate the signed angle between the two on the XZ plane.
		// The axis to rotate around to get from current to desired is the local Y axis (pitch axis).
		// The 'Vector3.Up' (or 'Vector3.Y') argument specifies the axis around which the rotation occurs.
		float angleToRotate = currentLocalForwardXZ.SignedAngleTo(desiredLocalForwardXZ, Vector3.Up); 

		// 3. Apply the rotation smoothly using a delta movement cap
		float maxRotationDelta = (float)delta * 2.0f; // Speed of rotation in radians/sec
		float actualRotationDelta = Mathf.Clamp(angleToRotate, -maxRotationDelta, maxRotationDelta);

		float potentialNewPitch = CurrentPitch + actualRotationDelta;
        float clampedNewPitch = Mathf.Clamp(potentialNewPitch, MinPitch, MaxPitch);

		float finalDelta = clampedNewPitch - CurrentPitch;

		RotateY(finalDelta); 
		CurrentPitch += finalDelta;
	}
}
