using Obsydian.Core.Math;
using Obsydian.Graphics;
using Obsydian.Input;

namespace Obsydian.UI.Layout;

/// <summary>
/// Container that arranges children in a grid with a fixed number of columns.
/// Rows auto-size based on the tallest child in each row.
/// </summary>
public sealed class GridContainer : UIElement
{
    public int Columns { get; set; } = 2;
    public float RowSpacing { get; set; } = 4f;
    public float ColSpacing { get; set; } = 4f;
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
        if (Columns <= 0) return;

        var visibleChildren = new List<UIElement>();
        foreach (var child in Children)
        {
            if (child.Visible)
                visibleChildren.Add(child);
        }

        if (visibleChildren.Count == 0) return;

        float availableWidth = Bounds.Width - Padding * 2;
        float cellWidth = (availableWidth - ColSpacing * (Columns - 1)) / Columns;

        float y = Bounds.Y + Padding;
        int col = 0;
        float rowHeight = 0;

        foreach (var child in visibleChildren)
        {
            if (col >= Columns)
            {
                y += rowHeight + RowSpacing;
                col = 0;
                rowHeight = 0;
            }

            float x = Bounds.X + Padding + col * (cellWidth + ColSpacing);
            float childWidth = System.Math.Clamp(cellWidth - child.Margin.TotalHorizontal,
                child.MinSize.X, child.MaxSize.X);
            float childHeight = System.Math.Clamp(child.Bounds.Height,
                child.MinSize.Y, child.MaxSize.Y);

            child.Bounds = new Rect(
                x + child.Margin.Left,
                y + child.Margin.Top,
                childWidth,
                childHeight);

            rowHeight = MathF.Max(rowHeight, childHeight + child.Margin.TotalVertical);
            col++;
        }
    }
}
