using Godot;
using System;

public partial class Targeter : Control
{
	[Export]
	public TextureRect VerticalArm {get; set;}
	[Export]
	public TextureRect HorizontalArm {get; set;}
	[Export]
	public TextureRect TargetReticle {get;set;}
	[Export]
	public float Velocity {get;set;} = 500f;
	
	[Export]
	public Node2D TopLeft { get; set; }
	[Export]
	public Node2D BottomRight { get; set; }
	
	//Right-Down
	private Vector2 _upperBound;
	//Left-Up
	private Vector2 _lowerBound;
	
	
	private Vector2 _target;
	private Vector2 _reticleCenterPos => TargetReticle.Position + new Vector2(TargetReticle.PivotOffset.X, TargetReticle.PivotOffset.Y);
	private Fleet _selectedFleet;
	
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		//Init target and bounds:
		_upperBound = _reticleCenterPos;
			
		_lowerBound = TopLeft.Position + new Vector2(
			VerticalArm.Size.X + VerticalArm.Size.X / 2,
			HorizontalArm.Size.Y + HorizontalArm.Size.Y / 2
			);
			
		_target = _upperBound;
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		return;
		
		if(_selectedFleet == null)
			SetTarget(_upperBound);
		else
		{
			var fleetPos = GetFleetScreenPos(_selectedFleet);
			SetTarget(fleetPos);
		}
		
		var moved = (float)(Velocity * delta);
		moved = MathF.Min(moved, _reticleCenterPos.DistanceTo(_target));
		var direction = _reticleCenterPos.DirectionTo(_target);
		var displacement = direction * moved;
		TargetReticle.Position += displacement;
		HorizontalArm.Position += new Vector2(0, displacement.Y);
		VerticalArm.Position += new Vector2(displacement.X, 0);
	}
	
	public void FleetSelected(Fleet fleet)
	{
		_selectedFleet = fleet;
	}
	
	private Vector2 GetFleetScreenPos(Fleet fleet)
	{
		Camera2D camera = GetViewport().GetCamera2D();

		var fleetGlobalPos = fleet.GlobalPosition;
		var cameraOffset = camera.Offset;
		var fleetCameraPos = fleetGlobalPos - cameraOffset;
		var zoom = camera.Zoom;
		var screenOffset = fleetCameraPos * zoom;
		screenOffset += this.Size / 2;
		
		return screenOffset;
	}
	
	public void FleetUnselected()
	{
		_selectedFleet = null;
	}
	
	public void SetTarget(Vector2 target)
	{
		_target = target.Clamp(_lowerBound, _upperBound);;
	}
}
