namespace Obsydian.Core.ECS;

/// <summary>
/// The ECS world — owns all entities and their components.
/// Central registry for creating, querying, and destroying entities.
/// </summary>
public sealed class World
{
    private int _nextEntityId;
    private readonly HashSet<int> _alive = [];
    private readonly Dictionary<Type, Dictionary<int, IComponent>> _stores = [];

    public Entity CreateEntity()
    {
        var entity = new Entity(_nextEntityId++);
        _alive.Add(entity.Id);
        return entity;
    }

    public void DestroyEntity(Entity entity)
    {
        if (!_alive.Remove(entity.Id))
            return;

        foreach (var store in _stores.Values)
            store.Remove(entity.Id);
    }

    public bool IsAlive(Entity entity) => _alive.Contains(entity.Id);

    public void Add<T>(Entity entity, T component) where T : IComponent
    {
        var store = GetOrCreateStore<T>();
        store[entity.Id] = component;
    }

    public ref T Get<T>(Entity entity) where T : struct, IComponent
    {
        var store = GetOrCreateStore<T>();
        if (!store.TryGetValue(entity.Id, out var boxed))
            throw new KeyNotFoundException($"Entity {entity} does not have component {typeof(T).Name}");

        // For struct components, we need to unbox, return, and re-box on set.
        // This is a simple initial implementation — will be optimized with archetype storage later.
        return ref System.Runtime.CompilerServices.Unsafe.Unbox<T>(store[entity.Id]);
    }

    public T? TryGet<T>(Entity entity) where T : class, IComponent
    {
        var store = GetOrCreateStore<T>();
        return store.TryGetValue(entity.Id, out var component) ? (T)component : null;
    }

    public bool Has<T>(Entity entity) where T : IComponent
    {
        return _stores.TryGetValue(typeof(T), out var store) && store.ContainsKey(entity.Id);
    }

    public void Remove<T>(Entity entity) where T : IComponent
    {
        if (_stores.TryGetValue(typeof(T), out var store))
            store.Remove(entity.Id);
    }

    /// <summary>
    /// Query all entities that have all of the specified component types.
    /// </summary>
    public IEnumerable<Entity> Query<T1>() where T1 : IComponent
    {
        if (!_stores.TryGetValue(typeof(T1), out var store))
            yield break;

        foreach (var id in store.Keys)
        {
            if (_alive.Contains(id))
                yield return new Entity(id);
        }
    }

    public IEnumerable<Entity> Query<T1, T2>()
        where T1 : IComponent
        where T2 : IComponent
    {
        if (!_stores.TryGetValue(typeof(T1), out var store1))
            yield break;
        if (!_stores.TryGetValue(typeof(T2), out var store2))
            yield break;

        foreach (var id in store1.Keys)
        {
            if (_alive.Contains(id) && store2.ContainsKey(id))
                yield return new Entity(id);
        }
    }

    public IEnumerable<Entity> Query<T1, T2, T3>()
        where T1 : IComponent
        where T2 : IComponent
        where T3 : IComponent
    {
        if (!_stores.TryGetValue(typeof(T1), out var store1))
            yield break;
        if (!_stores.TryGetValue(typeof(T2), out var store2))
            yield break;
        if (!_stores.TryGetValue(typeof(T3), out var store3))
            yield break;

        foreach (var id in store1.Keys)
        {
            if (_alive.Contains(id) && store2.ContainsKey(id) && store3.ContainsKey(id))
                yield return new Entity(id);
        }
    }

    public int EntityCount => _alive.Count;

    private Dictionary<int, IComponent> GetOrCreateStore<T>() where T : IComponent
    {
        var type = typeof(T);
        if (!_stores.TryGetValue(type, out var store))
        {
            store = [];
            _stores[type] = store;
        }
        return store;
    }
}
