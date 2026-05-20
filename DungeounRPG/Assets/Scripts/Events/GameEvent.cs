using System;
using System.Collections.Generic;
using UnityEngine;

public class GameEvent
{
    public static bool DebugMode = false;

    private readonly string _name;
    private readonly SortedDictionary<int, List<Action>> _listeners = new();

    public GameEvent(string name)
    {
        _name = name;
    }

    public void Subscribe(Action callback, int priority = 0)
    {
        if (!_listeners.ContainsKey(priority))
            _listeners[priority] = new List<Action>();

        _listeners[priority].Add(callback);

        if (DebugMode)
            Debug.Log($"[GameEvent] {_name} ← subscribed [{priority}] {callback.Method.DeclaringType?.Name}.{callback.Method.Name}");
    }

    public void Unsubscribe(Action callback)
    {
        foreach (var list in _listeners.Values)
            list.Remove(callback);
    }

    public void Raise()
    {
        if (DebugMode)
            Debug.Log($"[GameEvent] ▶ Raising: {_name}");

        foreach (var (priority, listeners) in _listeners)
        {
            foreach (var listener in listeners)
            {
                if (DebugMode)
                    Debug.Log($"[GameEvent] {_name} → [{priority}] {listener.Method.DeclaringType?.Name}.{listener.Method.Name}");

                listener.Invoke();
            }
        }
    }
}
