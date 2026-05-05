using Godot;
using System;

public partial class RotatingBarrel : TurretBarrel
{
	[Export] public int NrBarrels { get; set; }
	[Export] public float FireRate { get; set; } //Sec per shot
                                                 // Called when the node enters the scene tree for the first time.
	private Tween _tween { get; set; }
	private float _rotationPerShot => (Mathf.Pi * 2f) / NrBarrels;
	private float _rotationPerSec => (Mathf.Pi * 2f) * ((1 / FireRate) / NrBarrels);

	private bool isBeingFired = false;
	private Timer beingFiredTimer;
	
    public override void _Ready()
    {
        base._Ready();
		//GD.Print($"Rotation per shot {_rotationPerShot}");

		beingFiredTimer = new Timer();
		beingFiredTimer.WaitTime = FireRate * 2;
		beingFiredTimer.Autostart = false;
		beingFiredTimer.OneShot = true;
		beingFiredTimer.Timeout += () => isBeingFired = false;
		beingFiredTimer.IgnoreTimeScale = true; 
		
		this.AddChild(beingFiredTimer);
    }


    public override void WasFired()
    {
		// _tween = GetTree().CreateTween();
		// _tween.TweenProperty(Barrel, "rotation", Barrel.Rotation.Rotated(new Vector3(1, 0, 0), _rotationPerShot), FireRate);

		isBeingFired = true;
		beingFiredTimer.Start(FireRate * 2);
    }

    public override void _Process(double delta)
    {
        base._Process(delta);
        
        if(isBeingFired)
        {
			Barrel.RotateX(_rotationPerSec * (float)delta);
        }
    }

}
