using Godot;

public partial class Shockwave : ColorRect
{
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		Material = Material.Duplicate(true) as ShaderMaterial;
		var ShaderMaterial = Material as ShaderMaterial;
		//ShaderMaterial.SetShaderParameter("global_position", (GetParent() as Node2D).GlobalPosition);
		
		GetTree().CreateTimer(0.25).Timeout += () =>
		{
			var tween = CreateTween();
			tween.TweenProperty(ShaderMaterial, "shader_parameter/size", 0, .01f);
			tween.TweenProperty(ShaderMaterial, "shader_parameter/size", 1, 0.5);
			tween.Finished += () => { TopLevel = false; QueueFree(); };
		};
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		
	}
}
