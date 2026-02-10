using System.Numerics;

namespace Obsydian.Core.Math;

/// <summary>
/// 2D vector using float precision. Wraps System.Numerics for SIMD support.
/// </summary>
public readonly record struct Vec2(float X, float Y)
{
    public static readonly Vec2 Zero = new(0, 0);
    public static readonly Vec2 One = new(1, 1);
    public static readonly Vec2 Up = new(0, -1);
    public static readonly Vec2 Down = new(0, 1);
    public static readonly Vec2 Left = new(-1, 0);
    public static readonly Vec2 Right = new(1, 0);

    public float Length => MathF.Sqrt(X * X + Y * Y);
    public float LengthSquared => X * X + Y * Y;

    public Vec2 Normalized
    {
        get
        {
            var len = Length;
            return len > 0 ? new Vec2(X / len, Y / len) : Zero;
        }
    }

    public static Vec2 operator +(Vec2 a, Vec2 b) => new(a.X + b.X, a.Y + b.Y);
    public static Vec2 operator -(Vec2 a, Vec2 b) => new(a.X - b.X, a.Y - b.Y);
    public static Vec2 operator *(Vec2 a, float s) => new(a.X * s, a.Y * s);
    public static Vec2 operator *(float s, Vec2 a) => new(a.X * s, a.Y * s);
    public static Vec2 operator /(Vec2 a, float s) => new(a.X / s, a.Y / s);
    public static Vec2 operator -(Vec2 a) => new(-a.X, -a.Y);

    public static float Dot(Vec2 a, Vec2 b) => a.X * b.X + a.Y * b.Y;
    public static float Distance(Vec2 a, Vec2 b) => (a - b).Length;
    public static Vec2 Lerp(Vec2 a, Vec2 b, float t) => a + (b - a) * t;

    public override string ToString() => $"({X:F2}, {Y:F2})";

    // Implicit conversions to/from System.Numerics for interop
    public static implicit operator Vector2(Vec2 v) => new(v.X, v.Y);
    public static implicit operator Vec2(Vector2 v) => new(v.X, v.Y);
}
