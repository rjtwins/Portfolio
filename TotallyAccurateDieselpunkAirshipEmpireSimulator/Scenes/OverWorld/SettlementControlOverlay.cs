using Godot;

public partial class SettlementControlOverlay : Control
{
	[Export]
	public AnimationPlayer AnimationPlayer {get; set;}
	
	private bool _isDown = false;
	
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		AnimationPlayer.AnimationFinished += AnimationFinished;
	}

	private void AnimationFinished(StringName animName)
	{
		AnimationPlayer.Stop(true);
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		
	}
	
	public void MovePanelDown()
	{
		//If playing and we are moving down
		if(AnimationPlayer.IsPlaying() && AnimationPlayer.GetPlayingSpeed() > 0)
			return;
		else if(AnimationPlayer.IsPlaying() && AnimationPlayer.GetPlayingSpeed() < 0 && AnimationPlayer.CurrentAnimationPosition == AnimationPlayer.CurrentAnimationLength)
			AnimationPlayer.Stop(true);
		else if(AnimationPlayer.IsPlaying() && AnimationPlayer.GetPlayingSpeed() < 0)
			AnimationPlayer.Play(customSpeed: 1);
		else
			AnimationPlayer.Play("Swing");
	}
	
	public void MovePanelUp()
	{
		//If playing and we are moving up
		if(AnimationPlayer.IsPlaying() && AnimationPlayer.GetPlayingSpeed() < 0)
			return;
		else if(AnimationPlayer.IsPlaying() && AnimationPlayer.GetPlayingSpeed() < 0 && AnimationPlayer.CurrentAnimationPosition == 0)
			AnimationPlayer.Stop(true);
		//If playing and we are moving down
		else if(AnimationPlayer.IsPlaying() && AnimationPlayer.GetPlayingSpeed() > 0)
			AnimationPlayer.Play(customSpeed: -1);
		else
			AnimationPlayer.PlayBackwards("Swing");
	}
	
	public void SettlementSelected(Settlement s)
	{
		MovePanelDown();
	}
	
	public void SettlementUnselected()
	{
		MovePanelUp();
	}
}
