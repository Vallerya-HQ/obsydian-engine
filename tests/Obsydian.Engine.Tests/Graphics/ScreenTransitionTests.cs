using Obsydian.Graphics;

namespace Obsydian.Engine.Tests.Graphics;

public class ScreenTransitionTests
{
    [Fact]
    public void InitialState_IsNone()
    {
        var t = new ScreenTransition();
        Assert.Equal(TransitionState.None, t.State);
        Assert.False(t.IsActive);
    }

    [Fact]
    public void Start_BeginsFadeOut()
    {
        var t = new ScreenTransition();
        t.Start();
        Assert.Equal(TransitionState.FadingOut, t.State);
        Assert.True(t.IsActive);
    }

    [Fact]
    public void FadeOut_IncreasesAlpha()
    {
        var t = new ScreenTransition { FadeOutDuration = 1f };
        t.Start();
        t.Update(0.5f);
        Assert.True(t.Alpha > 0f && t.Alpha < 1f);
    }

    [Fact]
    public void FadeOut_CallsMidpoint_ThenFadesIn()
    {
        bool midpointCalled = false;
        var t = new ScreenTransition { FadeOutDuration = 0.1f, FadeInDuration = 0.1f };
        t.Start(onMidpoint: () => midpointCalled = true);
        t.Update(0.2f); // past fade out duration
        Assert.True(midpointCalled);
        Assert.Equal(TransitionState.FadingIn, t.State);
    }

    [Fact]
    public void FullTransition_CallsComplete()
    {
        bool completed = false;
        var t = new ScreenTransition { FadeOutDuration = 0.1f, FadeInDuration = 0.1f };
        t.Start(onComplete: () => completed = true);
        t.Update(0.15f); // finish fade out
        t.Update(0.15f); // finish fade in
        Assert.True(completed);
        Assert.False(t.IsActive);
    }

    [Fact]
    public void FadeIn_StartsFromFullAlpha()
    {
        var t = new ScreenTransition { FadeInDuration = 1f };
        t.FadeIn();
        Assert.Equal(TransitionState.FadingIn, t.State);
        Assert.Equal(1f, t.Alpha);
    }

    [Fact]
    public void FadeIn_DecreasesAlpha()
    {
        var t = new ScreenTransition { FadeInDuration = 1f };
        t.FadeIn();
        t.Update(0.5f);
        Assert.True(t.Alpha < 1f && t.Alpha > 0f);
    }
}
