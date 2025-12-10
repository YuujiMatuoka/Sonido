using System;
using System.Collections.Generic;

public static class EventBus
{
    private static Dictionary<Type, List<Delegate>> _subscribers = new();

    public static void Subscribe<T>(Action<T> callback)
    {
        var type = typeof(T);
        if (!_subscribers.ContainsKey(type))
            _subscribers[type] = new List<Delegate>();

        _subscribers[type].Add(callback);
    }

    public static void Unsubscribe<T>(Action<T> callback)
    {
        var type = typeof(T);
        if (_subscribers.TryGetValue(type, out var list))
            list.Remove(callback);
    }

    public static void Raise<T>(T evt)
    {
        var type = typeof(T);
        if (_subscribers.TryGetValue(type, out var list))
        {
            foreach (var callback in list)
                (callback as Action<T>)?.Invoke(evt);
        }
    }

    public static void ClearAll() => _subscribers.Clear();
}
