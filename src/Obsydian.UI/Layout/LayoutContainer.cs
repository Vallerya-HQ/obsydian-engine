using Obsydian.Core.Math;
using Obsydian.Graphics;
using Obsydian.Input;

namespace Obsydian.UI.Layout;

/// <summary>
/// Anchor presets for positioning elements relative to their parent.
/// </summary>
public enum Anchor
{
    TopLeft, TopCenter, TopRight,
    MiddleLeft, Center, MiddleRight,
    BottomLeft, BottomCenter, BottomRight
}

/// <summary>
/// Stacking direction for layout containers.
/// </summary>
public enum LayoutDirection { Vertical, Horizontal }

/// <summary>
/// Container that arranges children in a vertical or horizontal stack.
/// Children are positioned sequentially with configurable spacing and padding.
/// </summary>
public sealed class StackContainer : UIElement
{
    public LayoutDirection Direction { get; set; } = LayoutDirection.Vertical;
    public float Spacing { get; set; } = 4f;
    public float Padding { get; set; } = 8f;
    public HorizontalAlignment ChildHorizontalAlignment { get; set; } = HorizontalAlignment.Stretch;
    public VerticalAlignment ChildVerticalAlignment { get; set; } = VerticalAlignment.Start;

    public override void Update(float deltaTime, InputManager input)
    {
        ArrangeChildren();
        base.Update(deltaTime, input);
    }

    public override void Draw(IRenderer renderer)
    {
        if (!Visible) return;
        base.Draw(renderer);
    }

    private void ArrangeChildren()
    {
        float offset = Padding;

        foreach (var child in Children)
        {
            if (!child.Visible) continue;

            var childBounds = child.Bounds;
            var margin = child.Margin;

            if (Direction == LayoutDirection.Vertical)
            {
                float availWidth = Bounds.Width - Padding * 2 - margin.TotalHorizontal;
                float childWidth = child.HorizontalAlignment == HorizontalAlignment.Stretch
                    ? System.Math.Clamp(availWidth, child.MinSize.X, child.MaxSize.X)
                    : System.Math.Clamp(childBounds.Width, child.MinSize.X, child.MaxSize.X);

                float x = child.HorizontalAlignment switch
                {
                    HorizontalAlignment.Center => Bounds.X + Padding + margin.Left + (availWidth - childWidth) / 2,
                    HorizontalAlignment.End => Bounds.X + Bounds.Width - Padding - margin.Right - childWidth,
                    _ => Bounds.X + Padding + margin.Left
                };

                child.Bounds = new Rect(x, Bounds.Y + offset + margin.Top, childWidth, childBounds.Height);
                offset += childBounds.Height + margin.TotalVertical + Spacing;
            }
            else
            {
                float availHeight = Bounds.Height - Padding * 2 - margin.TotalVertical;
                float childHeight = child.VerticalAlignment == VerticalAlignment.Stretch
                    ? System.Math.Clamp(availHeight, child.MinSize.Y, child.MaxSize.Y)
                    : System.Math.Clamp(childBounds.Height, child.MinSize.Y, child.MaxSize.Y);

                float y = child.VerticalAlignment switch
                {
                    VerticalAlignment.Center => Bounds.Y + Padding + margin.Top + (availHeight - childHeight) / 2,
                    VerticalAlignment.End => Bounds.Y + Bounds.Height - Padding - margin.Bottom - childHeight,
                    _ => Bounds.Y + Padding + margin.Top
                };

                child.Bounds = new Rect(Bounds.X + offset + margin.Left, y, childBounds.Width, childHeight);
                offset += childBounds.Width + margin.TotalHorizontal + Spacing;
            }
        }
    }
}

/// <summary>
/// Container that positions a single child relative to its own bounds using anchor points.
/// Useful for HUD elements (health bar top-left, minimap top-right, etc.).
/// </summary>
public sealed class AnchorContainer : UIElement
{
    /// <summary>
    /// Position a child element relative to this container's bounds.
    /// </summary>
    public static Rect CalculatePosition(Rect parentBounds, Vec2 childSize, Anchor anchor, Vec2 offset = default)
    {
        float x = anchor switch
        {
            Anchor.TopLeft or Anchor.MiddleLeft or Anchor.BottomLeft => parentBounds.X + offset.X,
            Anchor.TopCenter or Anchor.Center or Anchor.BottomCenter => parentBounds.X + (parentBounds.Width - childSize.X) / 2 + offset.X,
            _ => parentBounds.Right - childSize.X + offset.X
        };

        float y = anchor switch
        {
            Anchor.TopLeft or Anchor.TopCenter or Anchor.TopRight => parentBounds.Y + offset.Y,
            Anchor.MiddleLeft or Anchor.Center or Anchor.MiddleRight => parentBounds.Y + (parentBounds.Height - childSize.Y) / 2 + offset.Y,
            _ => parentBounds.Bottom - childSize.Y + offset.Y
        };

        return new Rect(x, y, childSize.X, childSize.Y);
    }
}
