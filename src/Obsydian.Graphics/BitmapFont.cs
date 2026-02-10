using Obsydian.Core.Math;

namespace Obsydian.Graphics;

/// <summary>
/// Glyph metrics for a single character in a bitmap font.
/// </summary>
public readonly record struct Glyph(Rect SourceRect, int XAdvance);

/// <summary>
/// A bitmap font backed by a texture atlas. Each character maps to a source rectangle.
/// Supports measuring and rendering text.
/// </summary>
public sealed class BitmapFont
{
    public Texture Texture { get; }
    public int LineHeight { get; }
    public int Spacing { get; set; } = 1;

    private readonly Dictionary<char, Glyph> _glyphs = [];

    public BitmapFont(Texture texture, int lineHeight)
    {
        Texture = texture;
        LineHeight = lineHeight;
    }

    public void AddGlyph(char c, Rect sourceRect, int xAdvance)
    {
        _glyphs[c] = new Glyph(sourceRect, xAdvance);
    }

    public bool HasGlyph(char c) => _glyphs.ContainsKey(c);

    public Glyph GetGlyph(char c) =>
        _glyphs.TryGetValue(c, out var g) ? g : _glyphs.GetValueOrDefault('?');

    /// <summary>Measure the pixel width of a string at scale 1.</summary>
    public Vec2 MeasureString(string text)
    {
        float width = 0;
        float maxWidth = 0;
        int lines = 1;

        foreach (var c in text)
        {
            if (c == '\n')
            {
                maxWidth = MathF.Max(maxWidth, width);
                width = 0;
                lines++;
                continue;
            }

            var glyph = GetGlyph(c);
            width += glyph.XAdvance + Spacing;
        }

        maxWidth = MathF.Max(maxWidth, width);
        return new Vec2(maxWidth, lines * LineHeight);
    }

    /// <summary>
    /// Draw text using the given renderer. Call within BeginFrame/EndFrame.
    /// </summary>
    public void DrawString(IRenderer renderer, string text, Vec2 position, Color color, float scale = 1f)
    {
        float cursorX = position.X;
        float cursorY = position.Y;

        foreach (var c in text)
        {
            if (c == '\n')
            {
                cursorX = position.X;
                cursorY += LineHeight * scale;
                continue;
            }

            var glyph = GetGlyph(c);
            if (glyph.SourceRect.Width > 0)
            {
                renderer.DrawSprite(Texture,
                    new Vec2(cursorX, cursorY),
                    glyph.SourceRect,
                    new Vec2(scale, scale),
                    tint: color);
            }

            cursorX += (glyph.XAdvance + Spacing) * scale;
        }
    }
}
