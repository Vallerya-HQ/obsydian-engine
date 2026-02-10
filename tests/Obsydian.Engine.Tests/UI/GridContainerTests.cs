using Obsydian.Core.Math;
using Obsydian.Graphics;
using Obsydian.Input;
using Obsydian.UI;
using Obsydian.UI.Layout;

namespace Obsydian.Engine.Tests.UI;

public class GridContainerTests
{
    private sealed class TestElement : UIElement
    {
    }

    [Fact]
    public void ArrangesChildren_InColumns()
    {
        var grid = new GridContainer
        {
            Columns = 2,
            Padding = 0,
            ColSpacing = 0,
            RowSpacing = 0,
            Bounds = new Rect(0, 0, 200, 200)
        };

        var child1 = new TestElement { Bounds = new Rect(0, 0, 50, 30) };
        var child2 = new TestElement { Bounds = new Rect(0, 0, 50, 30) };
        var child3 = new TestElement { Bounds = new Rect(0, 0, 50, 30) };

        grid.AddChild(child1);
        grid.AddChild(child2);
        grid.AddChild(child3);

        var input = new InputManager();
        grid.Update(0.016f, input);

        // First row: child1 at x=0, child2 at x=100
        Assert.Equal(0, child1.Bounds.X);
        Assert.Equal(100, child2.Bounds.X);

        // Second row: child3 at x=0, y=30
        Assert.Equal(0, child3.Bounds.X);
        Assert.Equal(30, child3.Bounds.Y);
    }

    [Fact]
    public void RespectsColumnCount()
    {
        var grid = new GridContainer
        {
            Columns = 3,
            Padding = 0,
            ColSpacing = 0,
            RowSpacing = 0,
            Bounds = new Rect(0, 0, 300, 300)
        };

        for (int i = 0; i < 6; i++)
            grid.AddChild(new TestElement { Bounds = new Rect(0, 0, 50, 30) });

        var input = new InputManager();
        grid.Update(0.016f, input);

        // 3 columns: all 6 children should occupy 2 rows
        // Row 1: Y=0, Row 2: Y=30
        Assert.Equal(0, grid.Children[0].Bounds.Y);
        Assert.Equal(0, grid.Children[1].Bounds.Y);
        Assert.Equal(0, grid.Children[2].Bounds.Y);
        Assert.Equal(30, grid.Children[3].Bounds.Y);
        Assert.Equal(30, grid.Children[4].Bounds.Y);
        Assert.Equal(30, grid.Children[5].Bounds.Y);
    }

    [Fact]
    public void SkipsInvisibleChildren()
    {
        var grid = new GridContainer
        {
            Columns = 2,
            Padding = 0,
            ColSpacing = 0,
            RowSpacing = 0,
            Bounds = new Rect(0, 0, 200, 200)
        };

        var child1 = new TestElement { Bounds = new Rect(0, 0, 50, 30), Visible = false };
        var child2 = new TestElement { Bounds = new Rect(0, 0, 50, 30) };

        grid.AddChild(child1);
        grid.AddChild(child2);

        var input = new InputManager();
        grid.Update(0.016f, input);

        // child2 should be in the first position since child1 is invisible
        Assert.Equal(0, child2.Bounds.X);
        Assert.Equal(0, child2.Bounds.Y);
    }
}
