using Godot;
using System;
using System.Collections.ObjectModel;
using System.Linq;

public partial class WorldShip : RigidBody3D, IGameWorldSelectable, IGameWorldOrderable
{
	public static ObservableCollection<WorldShip> Active { get; set; } = new();
	public static ObservableCollection<WorldShip> Selected { get; set; } = new();
	public string Description { get; set; }

	[Export] public Vector3 TargetPosition 
	{
		get => ShipData.TargetPosition;
		set => ShipData.TargetPosition = value;
	}
	
	[Export] public Vector3 TargetDirection { get; set; }
	
	[Export] public Node3D TargetObject 
	{
		get => ShipData.TargetObject;
		set => ShipData.TargetObject = value;
	}
	
	[Export] public ShipData ShipData { get; set; }
	[Export] public ShipPid PID { get; set; }
	[Export] public ShipTuner ShipTuner { get; set; }

	[Export] public float MoveP { get; set; } = 0.25f;
	[Export] public float MoveD { get; set; } = -0.25f;
	
	[Export] public bool UISelected { get; set; }

	[Export] public float RotationSpeed { get; set; } = 0.1f;

	public bool InFormation => Formation != null;
	public WorldFormation Formation { get; set; }

	private bool _simulated = false;
	public Vector3 CustomDirectionToHold { get; set; } = Vector3.Zero;
	public WorldShipDirectionBehavior DirectionBehavior { get; set; } = WorldShipDirectionBehavior.FaceVelocity;
	public bool Broadside { get; set; }
	
	
	// Called when the node enters the scene tree for the first time.	
	public override void _Ready()
    {
		this.AddToGroup("UISelectable");
		this.TreeEntered += () => Active.Add(this);
		this.TreeExited += () => Active.Remove(this);
		Active.Add(this);
    }

    public void SetTargetObject(Node3D targetObject)
    {
		TargetObject = targetObject;
    }
    
    public void TuneShipPid()
    {
		if(ShipData.Thrust != 0 && ShipData.Weight != 1)
		{
			//Thrusters only need to work on the the actual mass of the ship.
			ShipTuner.RunSimulation(ShipData.Weight - ShipData.Lift, 100, ShipData.Thrust * -1, ShipData.Thrust);
			_simulated = true;
		}
    }
    
	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
    {
		
		if (ShipData.IsWrecked)
		{
			TargetObject = null;
			return;
		}
		
		ShipData.Update();
		UpdateTargetDirection();

		if (TargetObject is WorldShip worldShip && worldShip.ShipData.IsWrecked)
			TargetObject = null;
    }
    
    private void UpdateTargetDirection()
    {
		switch (DirectionBehavior)
		{
			case WorldShipDirectionBehavior.FaceVelocity:
				if (LinearVelocity.Length() > 1) 
					TargetDirection = LinearVelocity.Normalized() * new Vector3(1, 0, 1);
				break;
			case WorldShipDirectionBehavior.FaceTargetPosition:
				if(TargetPosition != Vector3.Zero && TargetPosition.DistanceTo(GlobalPosition) > 5)
					TargetDirection = GlobalPosition.DirectionTo(TargetPosition) * new Vector3(1, 0, 1);
				break;
			case WorldShipDirectionBehavior.FaceTargetObject:
				if(TargetObject != null)
					TargetDirection = GlobalPosition.DirectionTo(TargetObject.GlobalPosition) * new Vector3(1, 0, 1);
				break;
			default:
				//Custom direction:
				if(CustomDirectionToHold.Length() > 0)
					TargetDirection = CustomDirectionToHold;
				break;
		}
		
		if(Broadside)
		{
			TargetDirection = TargetDirection.Rotated(Vector3.Up, Mathf.Pi / 2f);
		}
    }
    
	public override void _PhysicsProcess(double delta)
	{
		if (Freeze)
			return;

		if (ShipData.IsWrecked)
			return;

		Mass = MathF.Max(ShipData.Weight, 1);

		var result = GetRealizedThrustAndDirection(delta);
		
		Vector3 appliedForce = result.desiredThrustDir * result.desiredThrust + Vector3.Up * ShipData.Lift * 9.80665f;
		ApplyCentralForce(appliedForce);
		
		var maxThrust = ShipData.Thrust * 9.80665f; //kg to N
		var thrustFraction = result.desiredThrust / maxThrust;

		//DebugDraw3D.DrawRay(GlobalPosition, result.desiredThrustDir, 10, Colors.Red, (float)delta);
		
		ShipData.Components
			.Select(x => x.Data)
			.OfType<EngineComponent>()
			.ToList().ForEach(x =>
			{
				x.PowerLevel = thrustFraction;
				x.DesiredThrustDirection = result.desiredThrustDir;
			}); //UpdateThrusterPowerLevel(result.desiredThrustDir, result.desiredThrust);

		HandleRotation(delta);
	}
	
	private void HandleRotation(double delta)
	{		
		//DebugDraw3D.DrawRay(GlobalPosition, TargetDirection, 10, Colors.Green, (float)delta);

		if (TargetDirection == Vector3.Zero)
            return;
            
		Vector3 currentDir = -GlobalTransform.Basis.Z;
		Vector3 rotationAxis = currentDir.Cross(TargetDirection);
		float angleDiff = Mathf.Acos(Mathf.Clamp(currentDir.Dot(TargetDirection), -1.0f, 1.0f));
		
        // If already aligned, stop rotating
        if (angleDiff < 0.001f || rotationAxis.LengthSquared() < 0.0001f)
        {
            AngularVelocity = Vector3.Zero;
            return;
        }
        
        rotationAxis = rotationAxis.Normalized();
        
       // Compute desired angular velocity vector
        float maxAngleStep = RotationSpeed * (float)delta;
        float appliedAngle = Mathf.Min(angleDiff, maxAngleStep);

        // Convert to angular velocity in world space (rad/sec)
        Vector3 desiredAngularVelocity = rotationAxis * (appliedAngle / (float)delta);

        AngularVelocity = desiredAngularVelocity;
		//GD.Print($"Weight (kg): {this.Mass} Force (kg) {appliedThrust / 9.80665f}");
	}
	
	private (float desiredThrust, Vector3 desiredThrustDir) GetRealizedThrustAndDirection(double delta)
	{
		var hor = GetDesiredHorizontalThrust(delta);
		var vert = GetDesiredVerticalThrust(delta);
		var desiredUp = vert.desired;
		var mandatoryUp = vert.mandatory;
		var maxThrust = ShipData.Thrust * 9.80665f;
		
		float remaining = maxThrust - mandatoryUp;
		
		Vector3 optionalVector = new Vector3(hor.X, desiredUp, hor.Z);
		Vector3 optionalDir = optionalVector.Normalized();
		
		float maxAngle = 0.5f * Mathf.Pi;
		float angle = Mathf.Acos(Vector3.Up.Dot(optionalDir));
		
		if (angle > maxAngle)
		{
			float t = maxAngle / angle;
			optionalDir = Vector3.Up.Lerp(optionalDir, t).Normalized();
		}
		
		Vector3 thrust =
			Vector3.Up * mandatoryUp +
			optionalDir * remaining;
			
		float realizedThrust = thrust.Length();
		Vector3 realizedDir  = thrust.Normalized();
		
		return (realizedThrust, realizedDir);
	}
	
	private (float desired, float mandatory) GetDesiredVerticalThrust(double delta)
	{
	    var verticalSpeed = LinearVelocity.Y;

		var correction = PID.DoPid(GlobalPosition.Y, ShipData.TargetPosition.Y, verticalSpeed, delta, -1, 1);
		correction = Mathf.Clamp(correction, -1, 1);
		
		var thrust = ShipData.Thrust * 9.80665f * correction; //kg to N
		var mandatory = Mathf.Min(ShipData.Thrust * 9.80665f, (Mass - ShipData.Lift) * 9.80665f);
		
		return (thrust, mandatory);
	}
	
	private Vector3 GetDesiredHorizontalThrust(double delta)
	{
	    Vector3 posError = ShipData.TargetPosition - GlobalPosition;
		posError.Y = 0;
		Vector3 vel = LinearVelocity;
		Vector3 velError = -vel;
		velError.Y = 0;
		
		Vector3 horizontalAcc =
			posError * MoveP +
			velError * MoveD;

		return horizontalAcc * Mass;
	}

    public bool IsInSelectionBox(Rect2 box)
    {
		Camera3D camera = GameWorldTest.InMapMode ? OrthogonalCamera3d.Instance: OrbitalCamera.Instance;
		var canvasPos = camera.UnprojectPosition(GlobalPosition);
		return box.HasPoint(canvasPos);
    }

    public void Select()
    {
		if (InFormation)
			return;
		
		this.UISelected = true;
		AddToGroup("UISelected");
		Selected.Add(this);
    }

    public void Deselect()
    {
		this.UISelected = false;
		RemoveFromGroup("UISelected");
		Selected.Remove(this);
    }

    public void MoveToPosition(Vector3 position)
    {
		TargetPosition = position;
		TargetDirection = GlobalPosition.DirectionTo(TargetPosition) * new Vector3(1, 0, 1);
    }

    public void HoldPosition()
    {
		TargetPosition = GlobalPosition;
    }
    
    public float GetHeight()
    {
		return GlobalPosition.Y;
    }
    
    public bool GetFormation(out WorldFormation formation)
    {
		formation = WorldFormation.Active.FirstOrDefault(x => x.WorldShips.Contains(this));
		return formation != null;
    }
}
