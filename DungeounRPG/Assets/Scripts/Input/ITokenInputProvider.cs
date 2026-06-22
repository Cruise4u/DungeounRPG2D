using UnityEngine;

public interface ITokenInputProvider
{
    bool DragStarted { get; }
    bool IsDragging  { get; }
    bool DragEnded   { get; }
    Vector2 PointerScreenPosition { get; }
}
