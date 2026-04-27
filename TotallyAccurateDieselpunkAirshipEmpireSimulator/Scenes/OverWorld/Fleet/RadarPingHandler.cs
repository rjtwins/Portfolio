using System;
using System.Linq;
using Godot;

public partial class RadarPingHandler : Node2D
{
	[Export] private PackedScene radarPingScene;
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
    {
		OverworldSpeedControl.GameSpeedChanged += GameSpeedChanged;
        
        TreeExiting += () => 
        {
            OverworldSpeedControl.GameSpeedChanged -= GameSpeedChanged;
        };
    }


    private void GameSpeedChanged(double newValue)
    {
        this.GetChildren().ToList().ForEach(x => x.QueueFree());
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta)
    {
        
    }
	
	public void ShowPing(Vector2 point)
	{
	    var newPing = radarPingScene.Instantiate<RadarPing>();
	    newPing.GlobalPosition = point;
	    newPing.TopLevel = true;
	    AddChild(newPing);
	}
}
