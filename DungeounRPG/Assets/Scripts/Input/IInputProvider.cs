using UnityEngine;

public interface IInputProvider
{
    bool DragStarted { get; }
    bool IsDragging  { get; }
    bool DragEnded   { get; }
    Vector2 PointerScreenPosition { get; }
}
