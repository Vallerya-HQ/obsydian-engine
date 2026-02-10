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

    // ─── AnimationPlayer Tests ─────────────────────────────────────────────────

    private static (AnimationPlayer player, Animation anim) CreatePlayerWithAnimation(
        int frameCount = 4, float duration = 0.1f, bool looping = true)
    {
        var sheet = new SpriteSheet(new Texture { Id = 1, Width = 64, Height = 16 }, 16, 16);
        var frames = Enumerable.Range(0, frameCount)
            .Select(i => sheet.GetCell(i, 0))
            .ToList();
        var anim = new Animation("test", sheet, frames, duration, looping);
        return (new AnimationPlayer(), anim);
    }

    [Fact]
    public void AnimationPlayer_Play_SetsCurrentFrameToZero()
    {
        var (player, anim) = CreatePlayerWithAnimation();
        player.Play(anim);

        Assert.Equal(0, player.CurrentFrame);
        Assert.True(player.IsPlaying);
        Assert.False(player.IsFinished);
    }

    [Fact]
    public void AnimationPlayer_Update_AdvancesFrame()
    {
        var (player, anim) = CreatePlayerWithAnimation(duration: 0.1f);
        player.Play(anim);

        player.Update(0.11f); // just past one frame duration

        Assert.Equal(1, player.CurrentFrame);
    }

    [Fact]
    public void AnimationPlayer_Update_Loops()
    {
        var (player, anim) = CreatePlayerWithAnimation(frameCount: 4, duration: 0.1f);
        player.Play(anim);

        // Advance through all 4 frames + past end
        for (int i = 0; i < 5; i++)
            player.Update(0.11f);

        Assert.Equal(1, player.CurrentFrame); // wraps: 0,1,2,3,0,1
        Assert.True(player.IsPlaying);
        Assert.False(player.IsFinished);
    }

    [Fact]
    public void AnimationPlayer_NonLooping_StopsAtLastFrame()
    {
        var (player, anim) = CreatePlayerWithAnimation(frameCount: 4, duration: 0.1f, looping: false);
        player.Play(anim);

        // Advance past all frames
        for (int i = 0; i < 5; i++)
            player.Update(0.11f);

        Assert.Equal(3, player.CurrentFrame); // stuck at last frame
        Assert.False(player.IsPlaying);
        Assert.True(player.IsFinished);
    }

    [Fact]
    public void AnimationPlayer_CurrentSourceRect_ReturnsCorrectRect()
    {
        var sheet = new SpriteSheet(new Texture { Id = 1, Width = 64, Height = 16 }, 16, 16);
        var frames = new[] { sheet.GetCell(0, 0), sheet.GetCell(1, 0), sheet.GetCell(2, 0), sheet.GetCell(3, 0) };
        var anim = new Animation("test", sheet, frames, 0.1f);
        var player = new AnimationPlayer();
        player.Play(anim);

        player.Update(0.11f); // advance to frame 1

        Assert.Equal(sheet.GetCell(1, 0), player.CurrentSourceRect);
    }

    [Fact]
    public void AnimationPlayer_Stop_PausesPlayback()
    {
        var (player, anim) = CreatePlayerWithAnimation();
        player.Play(anim);

        player.Stop();

        Assert.False(player.IsPlaying);
        // Update should have no effect when stopped
        player.Update(1f);
        Assert.Equal(0, player.CurrentFrame);
    }
}
