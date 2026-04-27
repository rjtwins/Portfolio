using Godot;
using System;

public partial class OverworldSpeedControl : Node
{
	private bool _paused = false;
	private bool _realtime = false;
	
	public delegate void OnGameSpeedChanged(double newValue);
	public static event OnGameSpeedChanged GameSpeedChanged;
	
	public static double GameSpeed 
	{
	    get 
	    {
	        return Engine.TimeScale;
	    }
	    set
	    {
			if(value == Engine.TimeScale)
				return;
				
	        Engine.TimeScale = value;
	        GameSpeedChanged?.Invoke(value);
	    }
	}
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		
	}

	public override void _UnhandledKeyInput(InputEvent @event)
	{
		
		if (!(@event is InputEventKey keyInput))
		{
			Godot.Engine.TimeScale = 1;
			return;
		}
		
		//Toggle on pause key
		if(keyInput.Keycode == Key.Pause && !keyInput.IsPressed())
		{
			_realtime = false;
			
			_paused = !_paused;
			GetTree().Paused = _paused;
			GameSpeed = _paused ? 0 : 1;
		}
		
		//Toggle on realtime/slomo key
		if(keyInput.Keycode == Key.Shift && !keyInput.IsPressed())
		{
			_paused = false;
			GetTree().Paused = false;

			_realtime = !_realtime;
			GameSpeed = _realtime ? 0.01f : 1f;

			return;
		}
		
		if (!keyInput.IsPressed())
		{
			GameSpeed = 1;
			return;
		}
					
		if (keyInput.Keycode == Key.Space)
		{
			GameSpeed = 60;
		    return;
		}
	}
}
