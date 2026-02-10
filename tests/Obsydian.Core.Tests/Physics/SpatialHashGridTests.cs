using Obsydian.Core.Math;
using Obsydian.Physics;

namespace Obsydian.Core.Tests.Physics;

public class SpatialHashGridTests
{
    [Fact]
    public void Insert_And_Query_FindsEntity()
    {
        var grid = new SpatialHashGrid(64);
        var bounds = AABB.FromPositionSize(new Vec2(10, 10), new Vec2(20, 20));

        grid.Insert(1, bounds);

        var results = grid.Query(bounds);
        Assert.Contains(1, results);
    }

    [Fact]
    public void Query_NoOverlap_ReturnsEmpty()
    {
        var grid = new SpatialHashGrid(64);
        grid.Insert(1, AABB.FromPositionSize(new Vec2(0, 0), new Vec2(10, 10)));

        var farAway = AABB.FromPositionSize(new Vec2(200, 200), new Vec2(10, 10));
        var results = grid.Query(farAway);

        Assert.DoesNotContain(1, results);
    }

    [Fact]
    public void GetPotentialPairs_ReturnsPairsInSameCell()
    {
        var grid = new SpatialHashGrid(64);
        grid.Insert(1, AABB.FromPositionSize(new Vec2(10, 10), new Vec2(20, 20)));
        grid.Insert(2, AABB.FromPositionSize(new Vec2(15, 15), new Vec2(20, 20)));

        var pairs = grid.GetPotentialPairs();

        Assert.Single(pairs);
        Assert.Contains((1, 2), pairs);
    }

    [Fact]
    public void GetPotentialPairs_NoDuplicates()
    {
        var grid = new SpatialHashGrid(32);
        // Two entities spanning multiple cells
        grid.Insert(1, AABB.FromPositionSize(new Vec2(0, 0), new Vec2(50, 50)));
        grid.Insert(2, AABB.FromPositionSize(new Vec2(10, 10), new Vec2(50, 50)));

        var pairs = grid.GetPotentialPairs();

        // Should have exactly one pair despite sharing multiple cells
        Assert.Single(pairs);
    }

    [Fact]
    public void Clear_RemovesAllEntries()
    {
        var grid = new SpatialHashGrid(64);
        grid.Insert(1, AABB.FromPositionSize(new Vec2(0, 0), new Vec2(10, 10)));

        grid.Clear();

        var results = grid.Query(AABB.FromPositionSize(new Vec2(0, 0), new Vec2(10, 10)));
        Assert.Empty(results);
    }

    [Fact]
    public void LargeEntity_SpansMultipleCells()
    {
        var grid = new SpatialHashGrid(32);
        // Entity spanning 100x100 pixels = ~4x4 cells
        grid.Insert(1, AABB.FromPositionSize(new Vec2(0, 0), new Vec2(100, 100)));

        // Query a small area in the middle should still find it
        var results = grid.Query(AABB.FromPositionSize(new Vec2(50, 50), new Vec2(1, 1)));
        Assert.Contains(1, results);
    }
}
