namespace Obsydian.Core.ECS;

/// <summary>
/// Typed dense-array component store. Provides O(1) add/get/remove with no boxing.
/// Uses a sparse-dense mapping for cache-friendly iteration via AsSpan().
/// </summary>
public sealed class ComponentStore<T> : IComponentStoreRemover where T : struct, IComponent
{
    private T[] _data;
    private int[] _entityToIndex; // sparse: entity ID → dense index (-1 if absent)
    private int[] _indexToEntity; // dense: dense index → entity ID
    private int _count;

    public int Count => _count;

    public ComponentStore(int initialCapacity = 64)
    {
        _data = new T[initialCapacity];
        _entityToIndex = new int[initialCapacity];
        _indexToEntity = new int[initialCapacity];
        Array.Fill(_entityToIndex, -1);
    }

    public void Set(int entityId, T component)
    {
        EnsureSparseCapacity(entityId);

        if (_entityToIndex[entityId] >= 0)
        {
            // Update existing
            _data[_entityToIndex[entityId]] = component;
            return;
        }

        // Add new
        EnsureDenseCapacity(_count);
        int denseIndex = _count++;
        _data[denseIndex] = component;
        _entityToIndex[entityId] = denseIndex;
        _indexToEntity[denseIndex] = entityId;
    }

    public ref T Get(int entityId)
    {
        if (entityId < 0 || entityId >= _entityToIndex.Length || _entityToIndex[entityId] < 0)
            throw new KeyNotFoundException($"Entity({entityId}) does not have component {typeof(T).Name}");

        return ref _data[_entityToIndex[entityId]];
    }

    public bool Has(int entityId)
    {
        return entityId >= 0 && entityId < _entityToIndex.Length && _entityToIndex[entityId] >= 0;
    }

    public void Remove(int entityId)
    {
        if (entityId < 0 || entityId >= _entityToIndex.Length || _entityToIndex[entityId] < 0)
            return;

        int denseIndex = _entityToIndex[entityId];
        int lastIndex = _count - 1;

        if (denseIndex < lastIndex)
        {
            // Swap-remove: move last element into the gap
            int lastEntity = _indexToEntity[lastIndex];
            _data[denseIndex] = _data[lastIndex];
            _indexToEntity[denseIndex] = lastEntity;
            _entityToIndex[lastEntity] = denseIndex;
        }

        _entityToIndex[entityId] = -1;
        _count--;
    }

    public ReadOnlySpan<T> AsSpan() => _data.AsSpan(0, _count);

    /// <summary>
    /// Returns the entity ID at the given dense index.
    /// </summary>
    public int GetEntityAt(int denseIndex) => _indexToEntity[denseIndex];

    private void EnsureSparseCapacity(int entityId)
    {
        if (entityId < _entityToIndex.Length) return;

        int newSize = _entityToIndex.Length;
        while (newSize <= entityId) newSize *= 2;

        var newSparse = new int[newSize];
        Array.Fill(newSparse, -1);
        Array.Copy(_entityToIndex, newSparse, _entityToIndex.Length);
        _entityToIndex = newSparse;
    }

    private void EnsureDenseCapacity(int index)
    {
        if (index < _data.Length) return;

        int newSize = _data.Length * 2;
        Array.Resize(ref _data, newSize);
        Array.Resize(ref _indexToEntity, newSize);
    }
}
