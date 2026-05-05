using Godot;

public interface IGameWorldSelectable
{
    public bool IsInSelectionBox(Rect2 box);
    public void Select();
    public void Deselect();
}

public interface IGameWorldOrderable
{
    public void MoveToPosition(Vector3 position);
    public void HoldPosition();
    public float GetHeight();
    public void SetTargetObject(Node3D target);
}