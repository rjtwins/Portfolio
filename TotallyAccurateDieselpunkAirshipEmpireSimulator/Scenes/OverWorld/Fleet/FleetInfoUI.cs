using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Godot;

public partial class FleetInfoUI : Control
{
	private Fleet _fleet;
	private Vector2 _initScale;
	private Camera2D _camera;
	private bool _mouseOnFleet = false;
	
	private CollisionPolygon2D _uiCollisionPolygon {get;set;}
	
	[Export] public Line2D PointerLine {get; set;}
	
	[Export] public Control UIContainer {get; set;}
	
	[Export] public Label NameLabel {get; set;}
	[Export] public Label SpeedLabel {get; set;}
	[Export] public Label DirectionLabel {get; set;}
	[Export] public Label RangeLabel {get; set;}
	[Export] public Label ETALabel {get; set;}
	
	[Export] CircleLine2D FuelRangeTotal;
	[Export] CircleLine2D FuelRangeReturn;
	
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		_initScale = Scale;
		
		Visible = false;
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		//Get fleet if null
		if(_fleet == null)
			_fleet = Owner as Fleet;
		
		if(_fleet.Faction != Faction.PLAYER)
        {
            Visible = false;
			FuelRangeReturn.Hide();
			FuelRangeTotal.Hide();
            return;
        }
		
		_camera = GetViewport().GetCamera2D();
		
		if (_camera == null)
			return;
		
		//Scale
		Scale = _initScale * (Vector2.One / _camera.Zoom);
		
		if(_fleet.IsSelected || _mouseOnFleet)
			Visible = true;
		else
			Visible = false;
		
		
		if(Visible)
		{
			//TODO: Handle colliding ui elements.
		}
		
		if(Visible)
		{
			//A bit crude but it will work.
			var options = new List<Vector2>() 
			{
				UIContainer.Position,
				UIContainer.Position + UIContainer.Size,
				new Vector2(UIContainer.Position.X + UIContainer.Size.X, UIContainer.Position.Y),
				new Vector2(UIContainer.Position.X, UIContainer.Position.Y + UIContainer.Size.Y),
			};
			var uiPos = options.OrderBy(x => x.DistanceTo(PointerLine.Points[0])).First();	
			PointerLine.SetPointPosition(1, uiPos);
			
			UpdateUI();
		}else
		{
			FuelRangeReturn.Hide();
			FuelRangeTotal.Hide();
		}
	}

	private void UpdateUI()
	{
		NameLabel.Text = _fleet.Name.ToString();
		SpeedLabel.Text = $"Speed: {Mathf.Round(_fleet.FleetInfo.SpeedKPH)} Km/h";
		DirectionLabel.Text = $"Heading: {Math.Round(_fleet.CompasRotation, 1)}°";
		RangeLabel.Text = $"Range: {Math.Round(_fleet.FleetInfo.RangeKM, 1)} km";
		
		float eta = _fleet.GetEta();
		string etaString = "N/A";
		
		if(eta != -1)
		{
			var timeSpan = TimeSpan.FromSeconds(MathF.Round(eta, 1));
			etaString = $"{Math.Round(timeSpan.TotalHours,1)}HR";
		}
		
		ETALabel.Text = $"ETA: {etaString}s";
		
		FuelRangeReturn.Show();
		FuelRangeTotal.Show();
		
		FuelRangeReturn.Radius = _fleet.FleetInfo.RangePX / 2;
		FuelRangeTotal.Radius = _fleet.FleetInfo.RangePX;
	}

	public void _on_mouse_detector_mouse_entered()
	{
		_mouseOnFleet = true;
	}
	
	public void _on_mouse_detector_mouse_exited()
	{
		_mouseOnFleet = false;
	}
}
