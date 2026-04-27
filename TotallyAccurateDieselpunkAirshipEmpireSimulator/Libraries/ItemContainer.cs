using Godot;
using System;
using System.Linq;

public partial class ItemContainer : HFlowContainer
{
	private float _hsep = 2f;
	private float _vsep = 2f;
	
	
	[Export]
	public Control Template{get;set;}
	
	private Vector2 _initSize {get; set;}
	
	public override void _Ready()
	{
		// GrowHorizontal = GrowDirection.Both;
		// GrowVertical = GrowDirection.Both;
		base._Ready();
		_initSize = Size;
	}

	public void AddTextureRect(Control node)
	{
		AddChild(node);
		var currentItems = GetChildren().ToList();
		var space = CustomMinimumSize.X * CustomMinimumSize.Y - currentItems.Count() * 2;
		var childSpace = space / currentItems.Count;
		var childSize = Mathf.Sqrt(childSpace) * 0.85f;
		
		currentItems.OfType<Control>().ToList().ForEach(x => 
		{
			x.Size = Vector2.One * childSize;
			x.CustomMinimumSize = Vector2.One * childSize;
		});
	}
}
