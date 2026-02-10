using Obsydian.Core.Math;

namespace Obsydian.Graphics;

/// <summary>
/// A texture atlas / sprite sheet with named regions for individual sprites.
/// </summary>
public sealed class SpriteSheet
{
    public Texture Texture { get; }
    public int CellWidth { get; }
    public int CellHeight { get; }

    private readonly Dictionary<string, Rect> _regions = [];

    public SpriteSheet(Texture texture, int cellWidth, int cellHeight)
    {
        Texture = texture;
        CellWidth = cellWidth;
        CellHeight = cellHeight;
    }

    public void DefineRegion(string name, Rect sourceRect)
    {
        _regions[name] = sourceRect;
    }

    public Rect GetRegion(string name)
    {
        return _regions.TryGetValue(name, out var rect)
            ? rect
            : throw new KeyNotFoundException($"Sprite region '{name}' not found.");
    }

    public Rect GetCell(int column, int row)
    {
        return new Rect(column * CellWidth, row * CellHeight, CellWidth, CellHeight);
    }

    public int Columns => Texture.Width / CellWidth;
    public int Rows => Texture.Height / CellHeight;
}
