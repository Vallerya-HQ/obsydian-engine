using Obsydian.Core.Math;

namespace Obsydian.Physics;

/// <summary>
/// Resolves entity movement against solid tiles using axis-separated collision.
/// Moves on X first, checks, then moves on Y. This prevents corner-sticking.
/// </summary>
public static class TileCollision
{
    /// <summary>
    /// Move an AABB by velocity, resolving collisions against a tile grid.
    /// Returns the resolved position after collision.
    /// </summary>
    /// <param name="position">Current top-left position of the entity's collider.</param>
    /// <param name="size">Size of the entity's collider.</param>
    /// <param name="velocity">Desired movement this frame (already scaled by dt).</param>
    /// <param name="isSolidTile">Function that returns true if tile at (col, row) is solid.</param>
    /// <param name="tileWidth">Width of each tile in pixels.</param>
    /// <param name="tileHeight">Height of each tile in pixels.</param>
    /// <returns>New position after collision resolution.</returns>
    public static Vec2 MoveAndSlide(
        Vec2 position, Vec2 size, Vec2 velocity,
        Func<int, int, bool> isSolidTile,
        int tileWidth, int tileHeight)
    {
        // Move X
        var newX = position.X + velocity.X;
        var testRect = new Rect(newX, position.Y, size.X, size.Y);
        if (CheckTileOverlap(testRect, isSolidTile, tileWidth, tileHeight))
        {
            // Snap to tile edge
            if (velocity.X > 0)
                newX = SnapToGridEdge(newX + size.X, tileWidth, true) - size.X;
            else if (velocity.X < 0)
                newX = SnapToGridEdge(newX, tileWidth, false);
        }

        // Move Y
        var newY = position.Y + velocity.Y;
        testRect = new Rect(newX, newY, size.X, size.Y);
        if (CheckTileOverlap(testRect, isSolidTile, tileWidth, tileHeight))
        {
            if (velocity.Y > 0)
                newY = SnapToGridEdge(newY + size.Y, tileHeight, true) - size.Y;
            else if (velocity.Y < 0)
                newY = SnapToGridEdge(newY, tileHeight, false);
        }

        return new Vec2(newX, newY);
    }

    private static bool CheckTileOverlap(Rect rect, Func<int, int, bool> isSolid, int tw, int th)
    {
        int startX = System.Math.Max(0, (int)(rect.X / tw));
        int startY = System.Math.Max(0, (int)(rect.Y / th));
        int endX = (int)((rect.Right - 0.01f) / tw);
        int endY = (int)((rect.Bottom - 0.01f) / th);

        for (int y = startY; y <= endY; y++)
        for (int x = startX; x <= endX; x++)
        {
            if (isSolid(x, y))
                return true;
        }
        return false;
    }

    private static float SnapToGridEdge(float pos, int tileSize, bool movingPositive)
    {
        if (movingPositive)
            return MathF.Floor(pos / tileSize) * tileSize;
        else
            return MathF.Ceiling(pos / tileSize) * tileSize;
    }
}
