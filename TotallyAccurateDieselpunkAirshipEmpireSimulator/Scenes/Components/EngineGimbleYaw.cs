using System;
using Godot;

public partial class EngineGimbleYaw : Node3D
{
	[Export] EngineComponent EngineComponent { get; set; }

	[Export] public float CurrentYaw { get; set; } = 0f;
	[Export] public float MaxYaw { get; set; } = MathF.PI;
	[Export] public float MinYaw { get; set; } = -MathF.PI;
	
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
    {
        
    }

	public override void _Process(double delta)
    {
		RotateYawOnly(delta);
    }
    
	public void RotateYawOnly(double delta)
	{
		var worldDirection = EngineComponent.DesiredThrustDirection.Normalized();
		
		// 1. Convert the world direction to the node's local coordinate system.
		var localDirection = GlobalTransform.Basis.Inverse() * worldDirection;

		// 2. We only care about the X rotation (yaw). 
		// We want to find the angle difference between the current local forward (Vector3.Back or -Vector3.Z) 
		// and the direction we want to face, but ONLY on the Y/Z plane (yaw).
		
		// Flatten the vectors onto the YZ plane to ignore pitch (Y-axis rotation)
		Vector3 currentLocalForwardYZ = Vector3.Back.Normalized(); // The object's current forward in its local space
		Vector3 desiredLocalForwardYZ = new Vector3(0, localDirection.Y, localDirection.Z).Normalized(); // The desired direction, clamped to YZ plane

		// Calculate the signed angle between the two on the YZ plane.
		// The axis to rotate around to get from current to desired is the local X axis.
		float angleToRotate = currentLocalForwardYZ.SignedAngleTo(desiredLocalForwardYZ, Vector3.Right); 

		// 3. Apply the rotation smoothly using Slerp or a simple delta movement cap
		// Let's smooth the rotation
		float maxRotationDelta = (float)delta * 2.0f; // Speed of rotation in radians/sec
		float actualRotationDelta = Mathf.Clamp(angleToRotate, -maxRotationDelta, maxRotationDelta);

		float potentialNewYaw = CurrentYaw + actualRotationDelta;
        float clampedNewYaw = Mathf.Clamp(potentialNewYaw, MinYaw, MaxYaw);

		float finalDelta = clampedNewYaw - CurrentYaw;

		// 4. Apply the rotation locally around the X axis (yaw)
		// This is the key: use RotateX() which applies rotation in local space
		RotateX(finalDelta);

		CurrentYaw += finalDelta;
	}
}
