using Obsydian.Core.Math;

namespace Obsydian.Graphics;

/// <summary>
/// A single glyph's destination and source rectangles for batched text rendering.
/// </summary>
public readonly record struct GlyphQuad(Rect Dest, Rect Source);
