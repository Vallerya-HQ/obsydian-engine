using Obsydian.Core.Math;

namespace Obsydian.Graphics.Tilemap;

/// <summary>
/// Renders a Tilemap using the IRenderer. Only draws tiles visible within the camera viewport.
/// </summary>
public static class TilemapRenderer
{
    /// <summary>
    /// Draw all visible layers of the tilemap. Call within BeginFrame/EndFrame.
    /// </summary>
    public static void Draw(IRenderer renderer, Tilemap tilemap, Camera2D camera)
    {
        var visible = camera.VisibleArea;
        Draw(renderer, tilemap, visible);
    }

    /// <summary>
    /// Draw all visible layers within a given world-space viewport rect.
    /// </summary>
    public static void Draw(IRenderer renderer, Tilemap tilemap, Rect visibleArea)
    {
        int startX = System.Math.Max(0, (int)(visibleArea.X / tilemap.TileWidth) - 1);
        int startY = System.Math.Max(0, (int)(visibleArea.Y / tilemap.TileHeight) - 1);
        int endX = System.Math.Min(tilemap.MapWidth - 1, (int)(visibleArea.Right / tilemap.TileWidth) + 1);
        int endY = System.Math.Min(tilemap.MapHeight - 1, (int)(visibleArea.Bottom / tilemap.TileHeight) + 1);

        foreach (var layer in tilemap.Layers)
        {
            if (!layer.Visible) continue;

            for (int y = startY; y <= endY; y++)
            for (int x = startX; x <= endX; x++)
            {
                var tile = layer.GetTileSafe(x, y);
                if (tile.TileId <= 0) continue;

                var src = tilemap.GetTileSourceRect(tile.TileId);
                var worldPos = new Vec2(x * tilemap.TileWidth, y * tilemap.TileHeight);

                renderer.DrawSprite(tilemap.Tileset, worldPos, src);
            }
        }
    }
}
