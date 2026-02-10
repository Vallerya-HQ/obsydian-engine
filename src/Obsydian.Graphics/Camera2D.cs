using Obsydian.Core.Math;

namespace Obsydian.Graphics;

/// <summary>
/// 2D camera with position, zoom, and viewport tracking.
/// Supports smooth follow and world-to-screen coordinate conversion.
/// </summary>
public sealed class Camera2D
{
    public Vec2 Position { get; set; }
    public float Zoom { get; set; } = 1f;
    public int ViewportWidth { get; set; }
    public int ViewportHeight { get; set; }

    /// <summary>
    /// Smoothing factor for follow (0 = instant, higher = smoother). Typical range: 3-10.
    /// </summary>
    public float FollowSmoothing { get; set; } = 5f;

    /// <summary>
    /// Dead zone radius — camera won't move if target is within this distance of center.
    /// </summary>
    public float DeadZone { get; set; } = 0f;

    public Camera2D(int viewportWidth, int viewportHeight)
    {
        ViewportWidth = viewportWidth;
        ViewportHeight = viewportHeight;
    }

    /// <summary>
    /// Instantly center the camera on a world position.
    /// </summary>
    public void LookAt(Vec2 worldPosition)
    {
        Position = worldPosition - new Vec2(ViewportWidth / (2f * Zoom), ViewportHeight / (2f * Zoom));
    }

    /// <summary>
    /// Smoothly follow a target position. Call each frame.
    /// </summary>
    public void Follow(Vec2 targetWorldPosition, float deltaTime)
    {
        var desired = targetWorldPosition - new Vec2(ViewportWidth / (2f * Zoom), ViewportHeight / (2f * Zoom));
        var diff = desired - Position;

        if (DeadZone > 0 && diff.LengthSquared < DeadZone * DeadZone)
            return;

        if (FollowSmoothing <= 0)
        {
            Position = desired;
        }
        else
        {
            var t = 1f - MathF.Exp(-FollowSmoothing * deltaTime);
            Position = Vec2.Lerp(Position, desired, t);
        }
    }

    /// <summary>
    /// Clamp camera position so it doesn't show outside the world bounds.
    /// </summary>
    public void ClampToWorld(float worldWidth, float worldHeight)
    {
        var visibleW = ViewportWidth / Zoom;
        var visibleH = ViewportHeight / Zoom;

        var minX = 0f;
        var minY = 0f;
        var maxX = MathF.Max(0, worldWidth - visibleW);
        var maxY = MathF.Max(0, worldHeight - visibleH);

        Position = new Vec2(
            MathF.Max(minX, MathF.Min(Position.X, maxX)),
            MathF.Max(minY, MathF.Min(Position.Y, maxY))
        );
    }

    /// <summary>
    /// Convert a screen position to world coordinates.
    /// </summary>
    public Vec2 ScreenToWorld(Vec2 screenPos)
    {
        return Position + screenPos / Zoom;
    }

    /// <summary>
    /// Convert a world position to screen coordinates.
    /// </summary>
    public Vec2 WorldToScreen(Vec2 worldPos)
    {
        return (worldPos - Position) * Zoom;
    }

    /// <summary>
    /// Get the visible world-space rectangle.
    /// </summary>
    public Rect VisibleArea => new(Position.X, Position.Y, ViewportWidth / Zoom, ViewportHeight / Zoom);
}
