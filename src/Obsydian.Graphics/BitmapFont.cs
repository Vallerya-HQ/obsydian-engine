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
    /// Word-wrap text to fit within maxWidth pixels at the given scale.
    /// Inserts newline characters at word boundaries. Preserves existing newlines.
    /// </summary>
    public string WrapText(string text, float maxWidth, float scale = 1f)
    {
        if (string.IsNullOrEmpty(text) || maxWidth <= 0) return text;

        var result = new System.Text.StringBuilder();
        var paragraphs = text.Split('\n');

        for (int p = 0; p < paragraphs.Length; p++)
        {
            if (p > 0) result.Append('\n');

            var words = paragraphs[p].Split(' ');
            float lineWidth = 0;
            float spaceWidth = (GetGlyph(' ').XAdvance + Spacing) * scale;
            bool firstOnLine = true;

            foreach (var word in words)
            {
                if (word.Length == 0) continue;

                float wordWidth = 0;
                foreach (var c in word)
                    wordWidth += (GetGlyph(c).XAdvance + Spacing) * scale;

                if (!firstOnLine && lineWidth + spaceWidth + wordWidth > maxWidth)
                {
                    result.Append('\n');
                    lineWidth = 0;
                    firstOnLine = true;
                }

                if (!firstOnLine)
                {
                    result.Append(' ');
                    lineWidth += spaceWidth;
                }

                result.Append(word);
                lineWidth += wordWidth;
                firstOnLine = false;
            }
        }

        return result.ToString();
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
