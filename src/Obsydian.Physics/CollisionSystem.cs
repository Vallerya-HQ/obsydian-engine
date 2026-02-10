using Obsydian.Core.Math;

namespace Obsydian.Physics;

/// <summary>
/// Axis-Aligned Bounding Box for 2D collision detection.
/// </summary>
public readonly record struct AABB(Vec2 Min, Vec2 Max)
{
    public float Width => Max.X - Min.X;
    public float Height => Max.Y - Min.Y;
    public Vec2 Center => new((Min.X + Max.X) / 2, (Min.Y + Max.Y) / 2);

    public static AABB FromPositionSize(Vec2 position, Vec2 size) =>
        new(position, position + size);

    public bool Overlaps(AABB other) =>
        Min.X < other.Max.X && Max.X > other.Min.X &&
        Min.Y < other.Max.Y && Max.Y > other.Min.Y;

    public bool Contains(Vec2 point) =>
        point.X >= Min.X && point.X <= Max.X &&
        point.Y >= Min.Y && point.Y <= Max.Y;
}

public readonly record struct CollisionResult(bool Hit, Vec2 Normal, float Depth);

/// <summary>
/// Simple spatial collision detection for 2D games.
/// </summary>
public static class Collision
{
    public static CollisionResult TestAABB(AABB a, AABB b)
    {
        if (!a.Overlaps(b))
            return new CollisionResult(false, Vec2.Zero, 0);

        var overlapX = MathF.Min(a.Max.X - b.Min.X, b.Max.X - a.Min.X);
        var overlapY = MathF.Min(a.Max.Y - b.Min.Y, b.Max.Y - a.Min.Y);

        if (overlapX < overlapY)
        {
            var sign = a.Center.X < b.Center.X ? -1f : 1f;
            return new CollisionResult(true, new Vec2(sign, 0), overlapX);
        }
        else
        {
            var sign = a.Center.Y < b.Center.Y ? -1f : 1f;
            return new CollisionResult(true, new Vec2(0, sign), overlapY);
        }
    }

    public static bool PointInAABB(Vec2 point, AABB box) => box.Contains(point);
}
