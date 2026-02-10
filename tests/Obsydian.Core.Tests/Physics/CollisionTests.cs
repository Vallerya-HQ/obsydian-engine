using Obsydian.Core.Math;
using Obsydian.Physics;

namespace Obsydian.Core.Tests.Physics;

public class CollisionTests
{
    [Fact]
    public void AABB_Overlap_Detected()
    {
        var a = AABB.FromPositionSize(new Vec2(0, 0), new Vec2(10, 10));
        var b = AABB.FromPositionSize(new Vec2(5, 5), new Vec2(10, 10));

        Assert.True(a.Overlaps(b));
    }

    [Fact]
    public void AABB_NoOverlap()
    {
        var a = AABB.FromPositionSize(new Vec2(0, 0), new Vec2(10, 10));
        var b = AABB.FromPositionSize(new Vec2(20, 20), new Vec2(10, 10));

        Assert.False(a.Overlaps(b));
    }

    [Fact]
    public void TestAABB_ReturnsCollisionResult()
    {
        var a = AABB.FromPositionSize(new Vec2(0, 0), new Vec2(10, 10));
        var b = AABB.FromPositionSize(new Vec2(8, 0), new Vec2(10, 10));

        var result = Collision.TestAABB(a, b);

        Assert.True(result.Hit);
        Assert.True(result.Depth > 0);
    }
}
