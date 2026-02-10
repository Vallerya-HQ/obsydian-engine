namespace Obsydian.Core.Events;

/// <summary>
/// Publish/subscribe event bus for decoupled communication between systems.
/// </summary>
public sealed class EventBus
{
    private readonly Dictionary<Type, List<Delegate>> _handlers = [];

    public void Subscribe<T>(Action<T> handler) where T : struct
    {
        var type = typeof(T);
        if (!_handlers.TryGetValue(type, out var list))
        {
            list = [];
            _handlers[type] = list;
        }
        list.Add(handler);
    }

    public void Unsubscribe<T>(Action<T> handler) where T : struct
    {
        if (_handlers.TryGetValue(typeof(T), out var list))
            list.Remove(handler);
    }

    public void Publish<T>(T evt) where T : struct
    {
        if (!_handlers.TryGetValue(typeof(T), out var list))
            return;

        foreach (var handler in list)
            ((Action<T>)handler)(evt);
    }

    public void Clear()
    {
        _handlers.Clear();
    }
}
