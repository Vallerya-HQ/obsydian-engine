using Obsydian.Core.Math;

namespace Obsydian.Graphics.Tilemap;

/// <summary>
/// A single tile in the map. TileId 0 = empty/air.
/// </summary>
public record struct Tile(int TileId, bool Solid = false, bool FlipX = false, bool FlipY = false);

/// <summary>
/// A layer of tiles in the tilemap. Multiple layers allow ground, decoration, overhead.
/// </summary>
public sealed class TileLayer
{
    public string Name { get; }
    public int Width { get; }
    public int Height { get; }
    public bool Visible { get; set; } = true;

    private readonly Tile[,] _tiles;

    public TileLayer(string name, int width, int height)
    {
        Name = name;
        Width = width;
        Height = height;
        _tiles = new Tile[width, height];
    }

    public ref Tile this[int x, int y] => ref _tiles[x, y];

    public bool InBounds(int x, int y) => x >= 0 && x < Width && y >= 0 && y < Height;

    public Tile GetTileSafe(int x, int y) =>
        InBounds(x, y) ? _tiles[x, y] : default;

    public void SetTile(int x, int y, Tile tile)
    {
        if (InBounds(x, y))
            _tiles[x, y] = tile;
    }

    /// <summary>Fill the entire layer with a single tile.</summary>
    public void Fill(Tile tile)
    {
        for (int y = 0; y < Height; y++)
        for (int x = 0; x < Width; x++)
            _tiles[x, y] = tile;
    }

    /// <summary>Fill a rectangular region.</summary>
    public void FillRect(int startX, int startY, int w, int h, Tile tile)
    {
        for (int y = startY; y < startY + h && y < Height; y++)
        for (int x = startX; x < startX + w && x < Width; x++)
        {
            if (InBounds(x, y))
                _tiles[x, y] = tile;
        }
    }
}

/// <summary>
/// Multi-layer tilemap with a tileset texture. The tileset is a grid of tiles.
/// </summary>
public sealed class Tilemap
{
    public int TileWidth { get; }
    public int TileHeight { get; }
    public int MapWidth { get; }
    public int MapHeight { get; }
    public Texture Tileset { get; }

    private readonly List<TileLayer> _layers = [];
    public IReadOnlyList<TileLayer> Layers => _layers;

    /// <summary>Number of tile columns in the tileset texture.</summary>
    public int TilesetColumns => Tileset.Width / TileWidth;
    public int TilesetRows => Tileset.Height / TileHeight;

    /// <summary>World-space pixel dimensions.</summary>
    public float WorldWidth => MapWidth * TileWidth;
    public float WorldHeight => MapHeight * TileHeight;

    public Tilemap(Texture tileset, int tileWidth, int tileHeight, int mapWidth, int mapHeight)
    {
        Tileset = tileset;
        TileWidth = tileWidth;
        TileHeight = tileHeight;
        MapWidth = mapWidth;
        MapHeight = mapHeight;
    }

    public TileLayer AddLayer(string name)
    {
        var layer = new TileLayer(name, MapWidth, MapHeight);
        _layers.Add(layer);
        return layer;
    }

    public TileLayer? GetLayer(string name) =>
        _layers.Find(l => l.Name == name);

    /// <summary>
    /// Get the tileset source rect for a tile ID. IDs are 1-based (0 = empty).
    /// Tiles are numbered left-to-right, top-to-bottom in the tileset.
    /// </summary>
    public Rect GetTileSourceRect(int tileId)
    {
        if (tileId <= 0) return default;
        int index = tileId - 1;
        int col = index % TilesetColumns;
        int row = index / TilesetColumns;
        return new Rect(col * TileWidth, row * TileHeight, TileWidth, TileHeight);
    }

    /// <summary>
    /// Check if a world position collides with a solid tile.
    /// </summary>
    public bool IsSolid(Vec2 worldPos)
    {
        int tx = (int)(worldPos.X / TileWidth);
        int ty = (int)(worldPos.Y / TileHeight);

        foreach (var layer in _layers)
        {
            var tile = layer.GetTileSafe(tx, ty);
            if (tile.Solid)
                return true;
        }
        return false;
    }

    /// <summary>
    /// Check if any solid tile overlaps the given world-space AABB.
    /// </summary>
    public bool CollidesWithRect(Rect worldRect)
    {
        int startX = System.Math.Max(0, (int)(worldRect.X / TileWidth));
        int startY = System.Math.Max(0, (int)(worldRect.Y / TileHeight));
        int endX = System.Math.Min(MapWidth - 1, (int)((worldRect.Right - 0.01f) / TileWidth));
        int endY = System.Math.Min(MapHeight - 1, (int)((worldRect.Bottom - 0.01f) / TileHeight));

        foreach (var layer in _layers)
        {
            for (int y = startY; y <= endY; y++)
            for (int x = startX; x <= endX; x++)
            {
                if (layer.GetTileSafe(x, y).Solid)
                    return true;
            }
        }
        return false;
    }
}
