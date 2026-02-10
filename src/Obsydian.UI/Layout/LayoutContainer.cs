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

            if (Direction == LayoutDirection.Vertical)
            {
                child.Bounds = new Rect(
                    Bounds.X + Padding,
                    Bounds.Y + offset,
                    Bounds.Width - Padding * 2,
                    childBounds.Height);
                offset += childBounds.Height + Spacing;
            }
            else
            {
                child.Bounds = new Rect(
                    Bounds.X + offset,
                    Bounds.Y + Padding,
                    childBounds.Width,
                    Bounds.Height - Padding * 2);
                offset += childBounds.Width + Spacing;
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
