using Godot;
using System;
using System.Linq;

public partial class ComponentButton2 : TextureButton
{
	[Export] public PackedScene ComponentScene;
	[Export] public Texture2D Texture;
	[Export] public Node2D ComponentNodesNode;
	
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		TextureNormal = Texture;
		Pressed += () => GetTree().CreateTimer(0.05f).Timeout += GenerateComponent;
	}

	private void GenerateComponent()
	{
		// if (ShipEditor3.Instance.currentlyHeldNode != null)
		// 	return;
		// var component = ComponentScene.Instantiate<Node2D>();
		// var editorComponent = (IComponent)component;
		// editorComponent.InEditor = true;
		// editorComponent.InEditorOnMouse = true;
		// ComponentNodesNode.AddChild(component);
		// ShipEditor3.Instance.currentlyHeldNode = editorComponent;
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		
	}
}
