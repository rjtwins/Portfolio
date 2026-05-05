using Godot;

public partial class MapShipModel : Node2D
{
	[Export] public float DayFlare = 0;
	[Export] public float NightFlare = 1;
	[Export] public Sprite2D Shadow;
	[Export] public Line2D Glow;
	[Export] public bool Landed {get; set;} = true;
	[Export] public bool TakingOff {get; set;} = false;
	[Export] public bool Landing {get; set;} = false;
	
	private Fleet _fleet;
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		_fleet = Owner as Fleet;
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		// if(Landed && !TakingOff)
		// {
		// 	(GetNode("Shadow") as Node2D).Hide();
		// 	(GetNode("Sprite2D") as Node2D).Hide();
		// 	//(GetNode("Trail") as GpuParticles2D).Emitting = false;
		// 	return;
		// }
		
		(GetNode("Shadow") as Node2D).Show();
		(GetNode("Sprite2D") as Node2D).Show();
		//(GetNode("Trail") as GpuParticles2D).Emitting = true;
		
		var camera = GetViewport().GetCamera2D();
		if (camera == null)
			return;
			
		var zoom = camera.Zoom.X;
		var ratio = zoom / 5;

		//Modulate = new Color(1, 1, 1 ,1) * ratio;
		
		if(!Landed && !TakingOff)
		{
			ratio = (1 - globals.CalculateLightingLevel()) * NightFlare * 1000;
			Glow.DefaultColor = new Color(29 * ratio, 9 * ratio, 0, 1);
			//GD.Print(Glow.DefaultColor);
		}     
	}
	
	public void TakeOff()
	{			
		TakingOff = true;
		
		var tween = CreateTween();
		tween.Parallel();
		tween.TweenProperty(Shadow, "offset", new Vector2(-80, 80), 25);
		tween.Parallel();
		// tween.TweenProperty(Shadow, "scale", new Vector2(0.04f, 0.04f), 30);
		// tween.Parallel();
		
		var ratio = (1 - globals.CalculateLightingLevel()) * NightFlare;
		
		tween.TweenProperty(Glow, "default_color:a", 1, 1);
		tween.Parallel();
		tween.TweenProperty(Glow, "default_color:r", 500 * ratio , 5).SetTrans(Tween.TransitionType.Cubic).SetEase(Tween.EaseType.Out);;
		tween.Parallel();
		tween.TweenProperty(Glow, "default_color:g", 180 * ratio, 5).SetTrans(Tween.TransitionType.Cubic).SetEase(Tween.EaseType.Out);
		
		tween.TweenProperty(Glow, "default_color:r", 29 * ratio, 10).SetEase(Tween.EaseType.In);
		tween.Parallel();
		tween.TweenProperty(Glow, "default_color:g", 9 * ratio, 10).SetEase(Tween.EaseType.In);
		tween.Finished += () => 
		{
		    TakingOff = false; 
		};
		
		Landed = false;
	}
	
	public void Land()
	{
		Landing = true;
		var tween = CreateTween();
		tween.Parallel();
		tween.TweenProperty(Shadow, "offset", new Vector2(0, 0), 8);
		tween.Parallel();
		
		tween.TweenProperty(Glow, "default_color:a", 0, 5);
		
		Landing = false; 
		Landed = true;
	}
}
