using Godot;
using System;

public partial class Turret : Node3D
{
	[Export] public Node3D Target { get; set; }
	[Export] public Vector3 PreviousTargetLocation { get; set; }
	[Export] public Vector3 PreviousOwnLocation { get; set; }

	[Export] public Vector3 TargetVelocity { get; set; }
	[Export] public Vector3 OwnVelocity { get; set; }

	[Export] public float AngularVelocity { get; set; } = 0.1f;
	[Export] public bool OnFireAngle { get; set; } = false;
	[Export] public float ProjectileSpeed = 1000f;
	[Export] public int Magazine = 50;
	[Export] public int MagazineCap = 50;
	[Export] public float FireRate = 0.03f;
	[Export] public float MagazineReloadTime = 10f;
	[Export] public Timer FireRateTimer { get; set; }
	[Export] public Timer MagazineReloadTimer { get; set; }
	[Export] public PackedScene BulletScene { get; set; }
	[Export] public TurretBarrel BarrelAssembly { get; set; }

	[Export] public DebugTargetBall DebugTargetBall { get; set; }

	private RandomNumberGenerator rng;

	[Export] public float Elevation { get; set; } // = 30 * MathF.PI/180;
	[Export] public WorldShip Ship { get; set; }

	[Export] public Vector3 GravityVector = new Vector3(0, -9.81f, 0);//new Vector3(0, -9.8f, 0);
																	  //[Export] public ComponentBase Component {get; set;}

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		
		if(FireRate > 0)
			FireRateTimer.WaitTime = FireRate;
			
		if(MagazineReloadTime > 0)
			MagazineReloadTimer.WaitTime = MagazineReloadTime;
			
		MagazineReloadTimer.Timeout += ReloadMagazine;


		FireRateTimer.OneShot = true;
		MagazineReloadTimer.OneShot = true;
		FireRateTimer.Autostart = false;
		MagazineReloadTimer.Autostart = false;

		rng = new RandomNumberGenerator();
		rng.Randomize();
	}

	private void ReloadMagazine()
	{
		Magazine = MagazineCap;
	}

    public override void _PhysicsProcess(double delta)
    {
		if (Target == null)
			return;

		var targetPosition = Target.GlobalPosition;
		TargetVelocity = (targetPosition - PreviousTargetLocation) / (float)delta;
		PreviousTargetLocation = targetPosition;

		OwnVelocity = (GlobalPosition - PreviousOwnLocation) / (float)delta;
		PreviousOwnLocation = GlobalPosition;

		// var targetPositionWithNoise = Target.GlobalPosition + Vector3.One * rng.RandfRange(-1, 1);
		// var TargetVelocityWithNoise = TargetVelocity + Vector3.One * rng.RandfRange(-1, 1);

		// if (!GetInterceptWithGravity(GlobalPosition, OwnVelocity, targetPositionWithNoise, TargetVelocityWithNoise, ProjectileSpeed, GravityVector, out Vector3 aimDir, out Vector3 interceptPoint, out float timeToHit))
		// 	return;

		var horizontalProjectileSpeed = ProjectileSpeed * MathF.Cos(Elevation);
		bool interceptPossible = GetIntercept(GlobalPosition, targetPosition, OwnVelocity, TargetVelocity, horizontalProjectileSpeed, out Vector3 interceptPoint);
		bool elevationPossible = GetElevation(GlobalPosition, interceptPoint, ProjectileSpeed, out float elevation);
		Elevation = elevation;

		// Where do we aim at x,z:
		Vector3 horizontalDir = interceptPoint - GlobalPosition;
		horizontalDir.Y = 0;
		horizontalDir = horizontalDir.Normalized();
		Vector3 fullAimDir = horizontalDir.Rotated(Vector3.Back, Elevation);

		// GD.Print($"{interceptPossible} {elevationPossible} {Mathf.RadToDeg(Elevation)}");
		// DebugTargetBall.GlobalPosition = interceptPoint;
		// DebugTargetBall.SetDebugText($"{timeToHit} sec");

		//DebugDraw3D.DrawRay(GlobalPosition, fullAimDir, 10000, Colors.Green, (float)delta);
		//DebugDraw3D.DrawRay(GlobalPosition, horizontalDir, 100, Colors.Blue);
		//DebugDraw3D.DrawArrow(GlobalPosition, interceptPoint, Colors.Red);

		if (!elevationPossible || !interceptPossible)
			return;
		
		RotateToTargetAngle(fullAimDir, delta);

		if (!OnFireAngle)
			return;

		Fire(fullAimDir);
    }


	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{

	}

	private void Fire(Vector3 aimDir)
	{
		if (MagazineReloadTime != 0 && Magazine <= 0)
			return;

		if (FireRate != 0 && !FireRateTimer.IsStopped())
			return;

		if (MagazineReloadTime != 0 && !MagazineReloadTimer.IsStopped())
			return;

		var bullet = BulletScene.Instantiate<BulletBase>();
		//Don't collide with on ship.
		//shipBulletCollisionAreas.ForEach(x => bullet.AddCollisionExceptionWith(x));

		GetTree().Root.AddChild(bullet);

		var muzzlePosition = BarrelAssembly.GetCurrentMuzzlePosition();

		var projectileSpeedWithNoise = ProjectileSpeed + ProjectileSpeed * rng.RandfRange(-.001f, 0.01f);
		bullet.GlobalPosition = muzzlePosition;
		bullet.GlobalRotation = muzzlePosition;
		bullet.LinearVelocity = aimDir * projectileSpeedWithNoise + OwnVelocity;

		BarrelAssembly.WasFired();

		Magazine -= 1;
		
		if(FireRate > 0)
			FireRateTimer.Start();

		if (MagazineReloadTime > 0 && Magazine == 0)
			MagazineReloadTimer.Start();
	}

	private void RotateToTargetAngle(Vector3 targetDir, double deltaTime)
	{
		// Normalize the direction to be safe
		targetDir = targetDir.Normalized();

		// Convert target direction to local space of the node
		// so we can measure rotation differences in node's local axes
		Vector3 localDirYaw = GlobalTransform.Basis.Inverse() * targetDir;
		Vector3 localDirPitch = BarrelAssembly.GlobalTransform.Basis.Inverse() * targetDir;

		// In local space, yaw is rotation around Y, pitch is rotation around X.
		// Local forward is -Z, so we use atan2 accordingly.
		var yawDelta = Mathf.Atan2(localDirYaw.X, -localDirYaw.Z); // left/right
		var pitchDelta = Mathf.Atan2(localDirPitch.Y, Mathf.Sqrt(localDirPitch.X * localDirPitch.X + localDirPitch.Z * localDirPitch.Z)); // up/down

		//GD.Print($"{localDirYaw}, {localDirPitch}");

		var yawSign = Math.Sign(yawDelta);
		var pitchSign = Math.Sign(pitchDelta);

		//Don't overshoot
		var preciseYaw = MathF.Min(Math.Abs(yawDelta), AngularVelocity * (float)deltaTime) * yawSign;
		var precisePitch = MathF.Min(Math.Abs(pitchDelta), AngularVelocity * (float)deltaTime) * pitchSign;

		//GD.Print($"dyaw {Math.Abs(yawDelta) - MathF.PI}");

		if (
			(MathF.Abs(yawDelta) < 0.1f || MathF.Abs(MathF.Abs(yawDelta) - MathF.PI) < 0.1f) && 
			(MathF.Abs(pitchDelta) < 0.1f || Math.Abs(MathF.Abs(pitchDelta) - MathF.PI) < 0.1f)
			)
		{
			OnFireAngle = true;
		}
		else
		{
			OnFireAngle = false;
		}

		Rotation = Rotation + new Vector3(0, preciseYaw, 0);
		BarrelAssembly.Rotation = BarrelAssembly.Rotation + new Vector3(0, 0, precisePitch);
	}
	
	public static bool GetInterceptWithGravity(
		Vector3 shooterPos,
		Vector3 shooterVel,
		Vector3 targetPos,
		Vector3 targetVel,
		float projectileSpeed,
		Vector3 gravity,
		out Vector3 aimDir,
		out Vector3 interceptPoint,
		out float timeToHit)
	{
		aimDir = Vector3.Zero;
		interceptPoint = Vector3.Zero;
		timeToHit = 0f;

		// Relative motion
		Vector3 relPos = targetPos - shooterPos;
		Vector3 relVel = targetVel - shooterVel;

		float bestError = float.MaxValue;
		float bestTime = -1f;
		Vector3 bestDir = Vector3.Zero;
		
		float distance = relPos.Length();
		float flatTime = distance / projectileSpeed;

		//Early return check
		if (flatTime < 0.1f)
		{
		    float t = Mathf.Max(0.02f, flatTime);
		    Vector3 predicted = targetPos + relVel * t;
		    Vector3 neededVel = (predicted - shooterPos - 0.5f * gravity * t * t) / t;
		    aimDir = neededVel.Normalized();
		    timeToHit = t;
		    interceptPoint = predicted;
		    return true;
		}
		
		// Scan over possible intercept times
		float gravityMag = gravity.Length();
		float gravityFactor = Mathf.Clamp(30f / gravityMag, 0.5f, 3f);
		
		float tMin = flatTime * 0.5f;
		float tMax = flatTime * 2 * gravityFactor;
		float step = Mathf.Clamp(flatTime / 40f, 0.01f, 0.1f);

		for (float t = tMin; t < tMax; t += step)
		{
			// Predict target position at this time
			Vector3 futureTarget = targetPos + relVel * t;

			// Required projectile displacement accounting for gravity
			Vector3 neededVel = (futureTarget - shooterPos - 0.5f * gravity * t * t) / t;

			float speedError = Mathf.Abs(neededVel.Length() - projectileSpeed);
			if (speedError < bestError || (Mathf.Abs(speedError - bestError) < 0.01f && t < bestTime))
			{
				bestError = speedError;
				bestTime = t;
				bestDir = neededVel.Normalized();
			}
		}

		if (bestTime < 0f || bestError > projectileSpeed * 0.05f)
			return false; // No reasonable solution found

		timeToHit = bestTime;
		aimDir = bestDir;
		interceptPoint = targetPos + relVel * bestTime;

		return true;
	}
	
	private static bool GetIntercept(Vector3 shooterPos, Vector3 targetPos, Vector3 shooterVelocity, Vector3 targetVelocity, float bulletHorizontalVelocity, out Vector3 intercept)
	{
		intercept = Vector3.Zero;
		
		targetVelocity = targetVelocity - shooterVelocity;
		
		float a = bulletHorizontalVelocity * bulletHorizontalVelocity - targetVelocity.Dot(targetVelocity);
		float b = 2 * targetVelocity.Dot(targetPos - shooterPos);
		float c = (targetPos - shooterPos).Dot(targetPos - shooterPos);

		if (bulletHorizontalVelocity <= targetVelocity.Length())
			return false;

		float time = (b + MathF.Sqrt(b * b + 4 * a * c)) / (2 * a);
		intercept = targetPos + time * targetVelocity;
		return true;
	}
	
	private static bool GetElevation(Vector3 shooterPos, Vector3 targetPos, float projectileSpeed, out float elevationAngle)
	{
		elevationAngle = 0f;
		float v0 = projectileSpeed;
		// float tempX = shooterPos.X - targetPos.X;
		// float tempZ = shooterPos.Z - targetPos.Z;
		// float x = MathF.Sqrt(tempX * tempX + tempZ * tempZ);

		var d = shooterPos.DistanceTo(targetPos);
		
		//In this case the target is either directly above
		//or below. Here I assume above, but more thorough
		//code would check and possibly set elevationAngle
		//to PI/2
		if(d == 0)
        {
			elevationAngle = Mathf.Pi * 0.5f;
			return false;
        }

		float y = targetPos.Y - shooterPos.Y;
		float g = 9.81f;
		
		if(Mathf.Pow(v0, 4) < g*(g*d*d+2*y*v0*v0))
		{
			return false;
		}

		float angle1 = MathF.Atan(
			(v0 * v0 + MathF.Sqrt(v0 * v0 * v0 * v0 - g * (g * d * d + 2 * y * v0 * v0))) 
			/ (g * d)
		);

		float angle2 = MathF.Atan(
			(v0 * v0 - MathF.Sqrt(v0 * v0 * v0 * v0 - g * (g * d * d + 2 * y * v0 * v0))) 
			/ (g * d)
		);

		elevationAngle = angle1;
		if (Math.Abs(angle2) < Math.Abs(angle1))
			elevationAngle = angle2;

		return true;
	}
	
	public void EngageTarget(Node3D target)
	{
		Target = target;
	}
}
