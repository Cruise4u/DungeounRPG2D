using UnityEngine;

public class MouseInputProvider : IInputProvider
{
    public bool DragStarted => Input.GetMouseButtonDown(0);
    public bool IsDragging  => Input.GetMouseButton(0);
    public bool DragEnded   => Input.GetMouseButtonUp(0);
    public Vector2 PointerScreenPosition => Input.mousePosition;
}
