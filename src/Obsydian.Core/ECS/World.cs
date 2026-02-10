namespace Obsydian.Core.ECS;

/// <summary>
/// The ECS world — owns all entities and their components.
/// Central registry for creating, querying, and destroying entities.
/// Uses typed dense-array ComponentStore&lt;T&gt; for cache-friendly, zero-boxing storage.
/// </summary>
public sealed class World
{
    private int _nextEntityId;
    private readonly HashSet<int> _alive = [];
    private readonly Dictionary<Type, object> _stores = [];

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
        {
            if (store is IComponentStoreRemover remover)
                remover.Remove(entity.Id);
        }
    }

    public bool IsAlive(Entity entity) => _alive.Contains(entity.Id);

    public void Add<T>(Entity entity, T component) where T : struct, IComponent
    {
        GetOrCreateStore<T>().Set(entity.Id, component);
    }

    public ref T Get<T>(Entity entity) where T : struct, IComponent
    {
        return ref GetOrCreateStore<T>().Get(entity.Id);
    }

    public bool Has<T>(Entity entity) where T : struct, IComponent
    {
        return _stores.TryGetValue(typeof(T), out var store) && ((ComponentStore<T>)store).Has(entity.Id);
    }

    public void Remove<T>(Entity entity) where T : struct, IComponent
    {
        if (_stores.TryGetValue(typeof(T), out var store))
            ((ComponentStore<T>)store).Remove(entity.Id);
    }

    /// <summary>
    /// Query all entities that have the specified component type.
    /// </summary>
    public IEnumerable<Entity> Query<T1>() where T1 : struct, IComponent
    {
        if (!_stores.TryGetValue(typeof(T1), out var raw))
            yield break;

        var store = (ComponentStore<T1>)raw;
        for (int i = 0; i < store.Count; i++)
        {
            int id = store.GetEntityAt(i);
            if (_alive.Contains(id))
                yield return new Entity(id);
        }
    }

    public IEnumerable<Entity> Query<T1, T2>()
        where T1 : struct, IComponent
        where T2 : struct, IComponent
    {
        if (!_stores.TryGetValue(typeof(T1), out var raw1))
            yield break;
        if (!_stores.TryGetValue(typeof(T2), out var raw2))
            yield break;

        var store1 = (ComponentStore<T1>)raw1;
        var store2 = (ComponentStore<T2>)raw2;

        // Iterate the smaller store
        if (store1.Count <= store2.Count)
        {
            for (int i = 0; i < store1.Count; i++)
            {
                int id = store1.GetEntityAt(i);
                if (_alive.Contains(id) && store2.Has(id))
                    yield return new Entity(id);
            }
        }
        else
        {
            for (int i = 0; i < store2.Count; i++)
            {
                int id = store2.GetEntityAt(i);
                if (_alive.Contains(id) && store1.Has(id))
                    yield return new Entity(id);
            }
        }
    }

    public IEnumerable<Entity> Query<T1, T2, T3>()
        where T1 : struct, IComponent
        where T2 : struct, IComponent
        where T3 : struct, IComponent
    {
        if (!_stores.TryGetValue(typeof(T1), out var raw1))
            yield break;
        if (!_stores.TryGetValue(typeof(T2), out var raw2))
            yield break;
        if (!_stores.TryGetValue(typeof(T3), out var raw3))
            yield break;

        var store1 = (ComponentStore<T1>)raw1;
        var store2 = (ComponentStore<T2>)raw2;
        var store3 = (ComponentStore<T3>)raw3;

        // Iterate the smallest store
        var smallest = store1.Count <= store2.Count && store1.Count <= store3.Count ? (object)store1
            : store2.Count <= store3.Count ? store2 : (object)store3;

        if (smallest == store1)
        {
            for (int i = 0; i < store1.Count; i++)
            {
                int id = store1.GetEntityAt(i);
                if (_alive.Contains(id) && store2.Has(id) && store3.Has(id))
                    yield return new Entity(id);
            }
        }
        else if (smallest == store2)
        {
            for (int i = 0; i < store2.Count; i++)
            {
                int id = store2.GetEntityAt(i);
                if (_alive.Contains(id) && store1.Has(id) && store3.Has(id))
                    yield return new Entity(id);
            }
        }
        else
        {
            for (int i = 0; i < store3.Count; i++)
            {
                int id = store3.GetEntityAt(i);
                if (_alive.Contains(id) && store1.Has(id) && store2.Has(id))
                    yield return new Entity(id);
            }
        }
    }

    public int EntityCount => _alive.Count;

    private ComponentStore<T> GetOrCreateStore<T>() where T : struct, IComponent
    {
        var type = typeof(T);
        if (_stores.TryGetValue(type, out var raw))
            return (ComponentStore<T>)raw;

        var store = new ComponentStore<T>();
        _stores[type] = store;
        return store;
    }
}

/// <summary>
/// Type-erased removal interface so DestroyEntity can remove from all stores.
/// </summary>
internal interface IComponentStoreRemover
{
    void Remove(int entityId);
}
