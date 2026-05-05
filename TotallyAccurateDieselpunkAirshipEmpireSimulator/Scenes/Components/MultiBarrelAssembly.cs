using Godot;
using System;
using System.Linq;

public partial class MultiBarrelAssembly : TurretBarrel
{
	[Export] public Godot.Collections.Array<Node3D> Barrels { get; set; }

	private int _barrelIndex = 0;

    public override Vector3 GetCurrentMuzzlePosition()
    {
		var barrel = Barrels[_barrelIndex];
		return barrel.GetNode<Node3D>("Muzzle").GlobalPosition;
    }


    public override void WasFired()
    {
		MuzzleFlash();
		RecoilBarrel();
		_barrelIndex = (_barrelIndex + 1) % Barrels.Count;
    }

    private void MuzzleFlash()
    {
    	var barrel = Barrels[_barrelIndex];
		barrel.GetChildren().OfType<AnimatedSprite3D>().ToList().ForEach(x => x.Play());
    }


    private void RecoilBarrel()
    {
		var tween = GetTree().CreateTween();
		var barrel = Barrels[_barrelIndex];
		var currentPosition = barrel.Position;
		var recoiledPosition = barrel.Position - new Vector3(0.25f, 0, 0);

		tween.TweenProperty(barrel, "position", recoiledPosition, 0.25f);
		tween.TweenProperty(barrel, "position", currentPosition, 0.25f);
    }


    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
	{
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}
}
