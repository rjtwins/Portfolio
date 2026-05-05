using System;
using Godot;
public interface IRWR
{
	public void ReceiveRadiation(Vector2 point, RadiationType type, Guid sourceId);
}