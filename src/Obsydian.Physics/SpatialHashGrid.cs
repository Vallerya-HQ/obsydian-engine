namespace Obsydian.Physics;

/// <summary>
/// Uniform-grid spatial hash for broad-phase collision detection.
/// O(1) insert/query for uniformly distributed entities.
/// </summary>
public sealed class SpatialHashGrid
{
    private readonly int _cellSize;
    private readonly Dictionary<long, List<int>> _cells = [];
    private readonly Dictionary<int, List<long>> _entityCells = [];

    public SpatialHashGrid(int cellSize = 64)
    {
        _cellSize = cellSize;
    }

    /// <summary>
    /// Insert an entity's AABB into the grid. Call after Clear() each frame.
    /// </summary>
    public void Insert(int entityId, AABB bounds)
    {
        int minCellX = (int)MathF.Floor(bounds.Min.X / _cellSize);
        int minCellY = (int)MathF.Floor(bounds.Min.Y / _cellSize);
        int maxCellX = (int)MathF.Floor(bounds.Max.X / _cellSize);
        int maxCellY = (int)MathF.Floor(bounds.Max.Y / _cellSize);

        if (!_entityCells.TryGetValue(entityId, out var cellList))
        {
            cellList = [];
            _entityCells[entityId] = cellList;
        }

        for (int y = minCellY; y <= maxCellY; y++)
        {
            for (int x = minCellX; x <= maxCellX; x++)
            {
                long key = CellKey(x, y);
                if (!_cells.TryGetValue(key, out var cell))
                {
                    cell = [];
                    _cells[key] = cell;
                }
                cell.Add(entityId);
                cellList.Add(key);
            }
        }
    }

    /// <summary>
    /// Query all entity IDs that share cells with the given AABB.
    /// </summary>
    public HashSet<int> Query(AABB bounds)
    {
        var result = new HashSet<int>();

        int minCellX = (int)MathF.Floor(bounds.Min.X / _cellSize);
        int minCellY = (int)MathF.Floor(bounds.Min.Y / _cellSize);
        int maxCellX = (int)MathF.Floor(bounds.Max.X / _cellSize);
        int maxCellY = (int)MathF.Floor(bounds.Max.Y / _cellSize);

        for (int y = minCellY; y <= maxCellY; y++)
        {
            for (int x = minCellX; x <= maxCellX; x++)
            {
                if (_cells.TryGetValue(CellKey(x, y), out var cell))
                {
                    foreach (var id in cell)
                        result.Add(id);
                }
            }
        }

        return result;
    }

    /// <summary>
    /// Get all unique entity pairs that share at least one cell.
    /// </summary>
    public List<(int A, int B)> GetPotentialPairs()
    {
        var pairs = new HashSet<(int, int)>();

        foreach (var cell in _cells.Values)
        {
            for (int i = 0; i < cell.Count; i++)
            {
                for (int j = i + 1; j < cell.Count; j++)
                {
                    int a = cell[i];
                    int b = cell[j];
                    // Canonical ordering to avoid duplicates
                    if (a > b) (a, b) = (b, a);
                    pairs.Add((a, b));
                }
            }
        }

        return [..pairs];
    }

    /// <summary>
    /// Clear all entries. Call at the start of each frame before re-inserting.
    /// </summary>
    public void Clear()
    {
        foreach (var cell in _cells.Values)
            cell.Clear();
        _entityCells.Clear();
    }

    private static long CellKey(int x, int y) => ((long)x << 32) | (uint)y;
}
