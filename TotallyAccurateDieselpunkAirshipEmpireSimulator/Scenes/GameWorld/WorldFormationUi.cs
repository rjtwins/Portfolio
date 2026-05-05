using Godot;
using System;
using System.Linq;

public partial class WorldFormationUi : Control
{
	[Export] WorldFormation WorldFormation { get; set; }
	[Export] Line2D SelectionIndicator { get; set; }
	[Export] Line2D MovementIndicator { get; set; }
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
    {

    }

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
    {
		Camera3D camera = GameWorldTest.InMapMode ? OrthogonalCamera3d.Instance : OrbitalCamera.Instance;
		Visible = WorldFormation.UISelected && !camera.IsPositionBehind(WorldFormation.GlobalPosition); //&& !GameWorldTest.InMapMode
		SelectionIndicator.Visible = Visible;
		MovementIndicator.Visible = Visible;
		
		if (!Visible)
			return;

		GlobalPosition = camera.UnprojectPosition(WorldFormation.GlobalPosition);
		
		int mod = GameWorldTest.InMapMode ? 5 : 50;
		var minX = WorldFormation.WorldShips.Min(x => camera.UnprojectPosition(x.GlobalPosition).X) - mod;
		var maxX = WorldFormation.WorldShips.Max(x => camera.UnprojectPosition(x.GlobalPosition).X) + mod;;

		var minY = WorldFormation.WorldShips.Min(x => camera.UnprojectPosition(x.GlobalPosition).Y) - mod;;
		var maxY = WorldFormation.WorldShips.Max(x => camera.UnprojectPosition(x.GlobalPosition).Y) + mod;;

		SelectionIndicator.ClearPoints();

		SelectionIndicator.AddPoint(new Vector2(minX, maxY)); //Top left
		SelectionIndicator.AddPoint(new Vector2(maxX, maxY)); //Top right
		SelectionIndicator.AddPoint(new Vector2(maxX, minY)); //bottom right
		SelectionIndicator.AddPoint(new Vector2(minX, minY)); //bottom left

		var moveToPosition = WorldFormation.Anchor.TargetPosition;
		if (camera.IsPositionBehind(moveToPosition))
		{
		    MovementIndicator.Visible = false;
			return;
		}
			
		var centerX = minX + ((maxX - minX) / 2);
		var centerY = minY + ((maxY - minY) / 2);

		var center = new Vector2(centerX, centerY);

		MovementIndicator.ClearPoints();
		MovementIndicator.AddPoint(center);
		MovementIndicator.AddPoint(camera.UnprojectPosition(moveToPosition));
		MovementIndicator.AddPoint(camera.UnprojectPosition(moveToPosition * new Vector3(1, 0 ,1)));
    }
}
