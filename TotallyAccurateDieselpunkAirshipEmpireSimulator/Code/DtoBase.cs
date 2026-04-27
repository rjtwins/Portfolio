using Godot;
using System;

namespace Code
{
	public abstract class DtoBase<T>
	{
		public Vector2 GlobalPosition {get; set;}
		public Vector2 Position {get; set;}
		public Vector2 GlobalRotation {get; set;}
		public Vector2 Rotation {get; set;}
		
		public string Name {get; set;}
		public string TypeString {get; set;}
	}
}