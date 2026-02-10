using Obsydian.Core.Math;
using Obsydian.Graphics;
using Obsydian.Input;

namespace Obsydian.UI.Widgets;

/// <summary>
/// Panel that renders using a 9-slice texture for scalable borders.
/// The texture is divided into a 3x3 grid: corners stay fixed, edges stretch
/// in one direction, and the center fills the remaining space.
/// </summary>
public sealed class NineSlicePanel : UIElement
{
    public Texture? Texture { get; set; }

    /// <summary>Border size in pixels from each edge of the source texture.</summary>
    public int BorderTop { get; set; } = 8;
    public int BorderBottom { get; set; } = 8;
    public int BorderLeft { get; set; } = 8;
    public int BorderRight { get; set; } = 8;

    public Color Tint { get; set; } = Color.White;

    /// <summary>Set all borders to the same value.</summary>
    public void SetBorder(int size)
    {
        BorderTop = BorderBottom = BorderLeft = BorderRight = size;
    }

    public override void Update(float deltaTime, InputManager input)
    {
        base.Update(deltaTime, input);
    }

    public override void Draw(IRenderer renderer)
    {
        if (!Visible || Texture is null) return;

        var tex = Texture;
        var b = Bounds;

        int bl = BorderLeft, br = BorderRight, bt = BorderTop, bb = BorderBottom;
        int tw = tex.Width, th = tex.Height;

        float centerW = b.Width - bl - br;
        float centerH = b.Height - bt - bb;
        int srcCenterW = tw - bl - br;
        int srcCenterH = th - bt - bb;

        // Top-left corner
        renderer.DrawSprite(tex, new Vec2(b.X, b.Y),
            new Rect(0, 0, bl, bt), null, 0f, Tint);

        // Top edge
        if (centerW > 0)
            renderer.DrawSprite(tex, new Vec2(b.X + bl, b.Y),
                new Rect(bl, 0, srcCenterW, bt),
                new Vec2(centerW / srcCenterW, 1f), 0f, Tint);

        // Top-right corner
        renderer.DrawSprite(tex, new Vec2(b.Right - br, b.Y),
            new Rect(tw - br, 0, br, bt), null, 0f, Tint);

        // Left edge
        if (centerH > 0)
            renderer.DrawSprite(tex, new Vec2(b.X, b.Y + bt),
                new Rect(0, bt, bl, srcCenterH),
                new Vec2(1f, centerH / srcCenterH), 0f, Tint);

        // Center
        if (centerW > 0 && centerH > 0)
            renderer.DrawSprite(tex, new Vec2(b.X + bl, b.Y + bt),
                new Rect(bl, bt, srcCenterW, srcCenterH),
                new Vec2(centerW / srcCenterW, centerH / srcCenterH), 0f, Tint);

        // Right edge
        if (centerH > 0)
            renderer.DrawSprite(tex, new Vec2(b.Right - br, b.Y + bt),
                new Rect(tw - br, bt, br, srcCenterH),
                new Vec2(1f, centerH / srcCenterH), 0f, Tint);

        // Bottom-left corner
        renderer.DrawSprite(tex, new Vec2(b.X, b.Bottom - bb),
            new Rect(0, th - bb, bl, bb), null, 0f, Tint);

        // Bottom edge
        if (centerW > 0)
            renderer.DrawSprite(tex, new Vec2(b.X + bl, b.Bottom - bb),
                new Rect(bl, th - bb, srcCenterW, bb),
                new Vec2(centerW / srcCenterW, 1f), 0f, Tint);

        // Bottom-right corner
        renderer.DrawSprite(tex, new Vec2(b.Right - br, b.Bottom - bb),
            new Rect(tw - br, th - bb, br, bb), null, 0f, Tint);

        base.Draw(renderer);
    }
}
