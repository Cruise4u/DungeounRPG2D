using UnityEngine;

public class TouchInputProvider : ITokenInputProvider
{
    public bool DragStarted =>
        Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began;

    public bool IsDragging
    {
        get
        {
            if (Input.touchCount == 0) return false;
            var phase = Input.GetTouch(0).phase;
            return phase == TouchPhase.Moved || phase == TouchPhase.Stationary;
        }
    }

    public bool DragEnded
    {
        get
        {
            if (Input.touchCount == 0) return false;
            var phase = Input.GetTouch(0).phase;
            return phase == TouchPhase.Ended || phase == TouchPhase.Canceled;
        }
    }

    public Vector2 PointerScreenPosition =>
        Input.touchCount > 0 ? (Vector2)Input.GetTouch(0).position : Vector2.zero;
}
