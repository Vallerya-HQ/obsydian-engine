using Obsydian.Core.Math;
using Obsydian.Graphics;

namespace Obsydian.Engine.Tests.Graphics;

public class AnimationSetTests
{
    [Fact]
    public void Add_And_Get_ReturnsAnimation()
    {
        var set = new AnimationSet();
        var sheet = new SpriteSheet(new Texture { Id = 1, Width = 64, Height = 64 }, 16, 16);
        var anim = new Animation("walk_down", sheet, [new Rect(0, 0, 16, 16)], 0.1f);

        set.Add("walk", FacingDirection.Down, anim);
        var result = set.Get("walk", FacingDirection.Down);

        Assert.NotNull(result);
        Assert.Equal("walk_down", result.Name);
    }

    [Fact]
    public void Get_FallsBackToDown()
    {
        var set = new AnimationSet();
        var sheet = new SpriteSheet(new Texture { Id = 1, Width = 64, Height = 64 }, 16, 16);
        var anim = new Animation("idle_down", sheet, [new Rect(0, 0, 16, 16)], 0.1f);

        set.Add("idle", FacingDirection.Down, anim);
        var result = set.Get("idle", FacingDirection.Right); // Right not added

        Assert.NotNull(result);
        Assert.Equal("idle_down", result.Name); // Falls back to Down
    }

    [Fact]
    public void Get_NonExistentName_ReturnsNull()
    {
        var set = new AnimationSet();
        Assert.Null(set.Get("nonexistent", FacingDirection.Down));
    }

    [Fact]
    public void Has_ReturnsTrueForExisting()
    {
        var set = new AnimationSet();
        var sheet = new SpriteSheet(new Texture { Id = 1, Width = 64, Height = 64 }, 16, 16);
        set.Add("walk", FacingDirection.Down, new Animation("w", sheet, [new Rect(0, 0, 16, 16)], 0.1f));

        Assert.True(set.Has("walk"));
        Assert.False(set.Has("run"));
    }

    [Fact]
    public void FromGrid_Creates4DirectionSet()
    {
        // 4 rows x 4 columns sprite sheet
        var sheet = new SpriteSheet(new Texture { Id = 1, Width = 64, Height = 64 }, 16, 16);
        var set = AnimationSet.FromGrid("walk", sheet, 0, 4, 0.15f);

        Assert.True(set.Has("walk"));
        Assert.NotNull(set.Get("walk", FacingDirection.Down));
        Assert.NotNull(set.Get("walk", FacingDirection.Up));
        Assert.NotNull(set.Get("walk", FacingDirection.Left));
        Assert.NotNull(set.Get("walk", FacingDirection.Right));
    }
}
