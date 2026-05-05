using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public partial class RWROverlay : Node2D
{
	[Export]
	Texture2D FCTexture {get; set;}
	[Export]
	Texture2D SRTexture {get; set;}
	[Export]
	Texture2D MTexture {get; set;}
	[Export]
	Texture2D AirBorneTexture {get; set;}
	[Export]
	Texture2D NewTexture {get; set;}
	[Export]
	Texture2D TrackTexture {get; set;}
	[Export]
	public Timer RefreshTimer {get; set;}
	
	public Dictionary<Guid, RWRContact> Contacts = new();
	List<Node2D> _iconNodes = new();
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		RefreshTimer.Timeout += Refresh;
		TreeExiting += () => 
		{
		    RefreshTimer.Timeout -= Refresh;
		};
	}

	private void Refresh()
	{
		_iconNodes.ForEach(x => 
		{
			RemoveChild(x);
			x.QueueFree();
		});
		
		_iconNodes.Clear();
		
		Contacts.ToList().ForEach(x =>
		{
			x.Value.Age += 1 * (float)Engine.TimeScale * 20;
			if(x.Value.Age > 119)
				return;
				
			var alpha = x.Value.Age / 120;
			var dir = Mathf.RadToDeg(GlobalPosition.DirectionTo(x.Value.Point).Angle());
			dir = dir < 0 ? dir + 360 : dir;
			var sector = (int)Math.Round(dir / 30);
			dir = sector * 30;
			var iconNode = GenerateIcon(alpha, x.Value.RadiationType);
			
			var pos = globals.CalculateVector(Mathf.DegToRad(dir), 100);
			AddChild(iconNode);
			iconNode.Position = pos;
			_iconNodes.Add(iconNode);
		});
	}
	
	private Node2D GenerateIcon(float alpha, RadiationType type)
	{
		var master = new Node2D();
		
		switch (type)
		{
			case RadiationType.SearchRadar:
				var abi = new TextureRect();
				abi.Texture = AirBorneTexture;
				var sri = new TextureRect();
				sri.Texture = SRTexture;
				abi.ExpandMode = TextureRect.ExpandModeEnum.IgnoreSize;
				sri.ExpandMode = TextureRect.ExpandModeEnum.IgnoreSize;
				abi.Size = Vector2.One * 64;
				sri.Size = Vector2.One * 64;
				master.AddChild(abi);
				master.AddChild(sri);
				abi.Position = Vector2.One * -32;
				sri.Position = Vector2.One * -32;
				break;
			default:
			break;
		}
		
		master.Modulate = new Color(1, 2, 1, 1);
		
		return master;
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		var camera = GetViewport().GetCamera2D();
		if (camera == null)
			return;
			
		Scale = Vector2.One * (Vector2.One / camera.Zoom);
	}
	
	public void ReceiveRadiation(Vector2 point, RadiationType type, Guid sourceId)
	{
		Contacts[sourceId] = new RWRContact()
        {
            Point = point,
            RadiationType = type,
            Age = 0f
        };
	}
	
}
public class RWRContact
{
	public Vector2 Point;
	public RadiationType RadiationType;
	public float Age;
}
