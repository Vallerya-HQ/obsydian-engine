namespace Obsydian.Core.Math;

/// <summary>
/// Axis-aligned rectangle, used for bounding boxes and UI layout.
/// </summary>
public readonly record struct Rect(float X, float Y, float Width, float Height)
{
    public float Left => X;
    public float Top => Y;
    public float Right => X + Width;
    public float Bottom => Y + Height;
    public Vec2 Position => new(X, Y);
    public Vec2 Size => new(Width, Height);
    public Vec2 Center => new(X + Width / 2, Y + Height / 2);

    public bool Contains(Vec2 point) =>
        point.X >= Left && point.X <= Right &&
        point.Y >= Top && point.Y <= Bottom;

    public bool Intersects(Rect other) =>
        Left < other.Right && Right > other.Left &&
        Top < other.Bottom && Bottom > other.Top;

    public static Rect FromCenter(Vec2 center, Vec2 size) =>
        new(center.X - size.X / 2, center.Y - size.Y / 2, size.X, size.Y);

    public override string ToString() => $"Rect({X:F1}, {Y:F1}, {Width:F1}, {Height:F1})";
}
