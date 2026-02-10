using Obsydian.Core.Math;

namespace Obsydian.Core.Tests.Math;

public class Vec2Tests
{
    [Fact]
    public void Addition()
    {
        var a = new Vec2(1, 2);
        var b = new Vec2(3, 4);
        Assert.Equal(new Vec2(4, 6), a + b);
    }

    [Fact]
    public void Length()
    {
        var v = new Vec2(3, 4);
        Assert.Equal(5f, v.Length, precision: 5);
    }

    [Fact]
    public void Normalized_ReturnsUnitLength()
    {
        var v = new Vec2(10, 0);
        var n = v.Normalized;
        Assert.Equal(1f, n.Length, precision: 5);
        Assert.Equal(new Vec2(1, 0), n);
    }

    [Fact]
    public void Dot_Product()
    {
        Assert.Equal(0, Vec2.Dot(Vec2.Up, Vec2.Right));
    }
}

public class RectTests
{
    [Fact]
    public void Contains_Point()
    {
        var rect = new Rect(0, 0, 100, 100);
        Assert.True(rect.Contains(new Vec2(50, 50)));
        Assert.False(rect.Contains(new Vec2(150, 50)));
    }

    [Fact]
    public void Intersects_Rects()
    {
        var a = new Rect(0, 0, 100, 100);
        var b = new Rect(50, 50, 100, 100);
        var c = new Rect(200, 200, 50, 50);

        Assert.True(a.Intersects(b));
        Assert.False(a.Intersects(c));
    }
}

public class ColorTests
{
    [Fact]
    public void FromHex_Parses()
    {
        var c = Color.FromHex("#FF8800");
        Assert.Equal(255, c.R);
        Assert.Equal(136, c.G);
        Assert.Equal(0, c.B);
        Assert.Equal(255, c.A);
    }
}
