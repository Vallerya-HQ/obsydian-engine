using Obsydian.Core.Math;
using Obsydian.Graphics;
using Obsydian.Input;

namespace Obsydian.UI.Widgets;

/// <summary>
/// Scrollable container. Children are rendered offset by scroll position.
/// Scrolling via mouse wheel when hovered, or keyboard arrows.
/// </summary>
public sealed class ScrollPanel : UIElement
{
    public float ScrollY { get; set; }
    public float ContentHeight { get; set; }
    public float ScrollSpeed { get; set; } = 30f;
    public Color BackgroundColor { get; set; } = new(30, 30, 30, 200);
    public Color ScrollbarColor { get; set; } = new(120, 120, 120);
    public float ScrollbarWidth { get; set; } = 6f;
    public bool ShowScrollbar { get; set; } = true;

    /// <summary>Maximum scroll offset.</summary>
    public float MaxScroll => System.Math.Max(0, ContentHeight - Bounds.Height);

    public override void Update(float deltaTime, InputManager input)
    {
        if (!Visible) return;

        if (HitTest(input.MousePosition))
        {
            ScrollY -= input.ScrollDelta * ScrollSpeed;
            ScrollY = System.Math.Clamp(ScrollY, 0, MaxScroll);
        }

        // Update children with scroll offset applied
        foreach (var child in Children)
            child.Update(deltaTime, input);
    }

    public override void Draw(IRenderer renderer)
    {
        if (!Visible) return;

        renderer.DrawRect(Bounds, BackgroundColor);

        // Draw children offset by scroll
        foreach (var child in Children)
        {
            if (!child.Visible) continue;

            var originalBounds = child.Bounds;
            child.Bounds = new Rect(
                originalBounds.X,
                originalBounds.Y - ScrollY,
                originalBounds.Width,
                originalBounds.Height);

            // Only draw if visible within panel bounds
            if (child.Bounds.Bottom > Bounds.Y && child.Bounds.Y < Bounds.Bottom)
                child.Draw(renderer);

            child.Bounds = originalBounds;
        }

        // Scrollbar
        if (ShowScrollbar && ContentHeight > Bounds.Height)
        {
            float scrollRatio = Bounds.Height / ContentHeight;
            float thumbHeight = System.Math.Max(20, Bounds.Height * scrollRatio);
            float thumbY = Bounds.Y + (ScrollY / MaxScroll) * (Bounds.Height - thumbHeight);

            renderer.DrawRect(
                new Rect(Bounds.Right - ScrollbarWidth - 2, thumbY, ScrollbarWidth, thumbHeight),
                ScrollbarColor);
        }
    }
}
