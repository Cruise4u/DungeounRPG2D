using System;
using System.Collections.Generic;
using UnityEngine;

public class TypedGameEvent<T>
{
    private readonly string _name;
    private readonly SortedDictionary<int, List<Action<T>>> _listeners = new();

    public TypedGameEvent(string name)
    {
        _name = name;
    }

    public void Subscribe(Action<T> callback, int priority = 0)
    {
        if (!_listeners.ContainsKey(priority))
            _listeners[priority] = new List<Action<T>>();

        _listeners[priority].Add(callback);

        if (GameEvent.DebugMode)
            Debug.Log($"[GameEvent] {_name} ← subscribed [{priority}] {callback.Method.DeclaringType?.Name}.{callback.Method.Name}");
    }

    public void Unsubscribe(Action<T> callback)
    {
        foreach (var list in _listeners.Values)
            list.Remove(callback);
    }

    public void Raise(T value)
    {
        if (GameEvent.DebugMode)
            Debug.Log($"[GameEvent] ▶ Raising: {_name} with value: {value}");

        foreach (var (priority, listeners) in _listeners)
        {
            foreach (var listener in listeners)
            {
                if (GameEvent.DebugMode)
                    Debug.Log($"[GameEvent] {_name} → [{priority}] {listener.Method.DeclaringType?.Name}.{listener.Method.Name}");

                listener.Invoke(value);
            }
        }
    }
}
