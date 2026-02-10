using Obsydian.Core.Math;

namespace Obsydian.Physics;

/// <summary>
/// Broad-phase collision wrapper using a spatial hash grid.
/// Feeds potential collision pairs to the narrow phase (Collision.TestAABB).
/// </summary>
public sealed class BroadPhase
{
    private readonly SpatialHashGrid _grid;
    private readonly Dictionary<int, AABB> _bounds = [];

    public BroadPhase(int cellSize = 64)
    {
        _grid = new SpatialHashGrid(cellSize);
    }

    /// <summary>
    /// Register or update an entity's bounds for this frame.
    /// </summary>
    public void Update(int entityId, Vec2 position, Vec2 size)
    {
        var aabb = AABB.FromPositionSize(position, size);
        _bounds[entityId] = aabb;
        _grid.Insert(entityId, aabb);
    }

    /// <summary>
    /// Get all unique pairs of entities that potentially collide.
    /// Call after all entities have been Updated for this frame.
    /// </summary>
    public List<(int A, int B)> GetPotentialPairs() => _grid.GetPotentialPairs();

    /// <summary>
    /// Get the cached AABB for an entity.
    /// </summary>
    public AABB GetBounds(int entityId) => _bounds[entityId];

    /// <summary>
    /// Clear all data for the next frame.
    /// </summary>
    public void Clear()
    {
        _grid.Clear();
        _bounds.Clear();
    }
}
