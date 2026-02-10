using Obsydian.Core;

namespace Obsydian.Platform.Desktop.Tests;

public class EngineLifecycleTests
{
    [Fact]
    public void Initialize_Sets_IsRunning_True()
    {
        var engine = new Engine();
        engine.Initialize();
        Assert.True(engine.IsRunning);
        engine.Shutdown();
    }

    [Fact]
    public void Initialize_Fires_OnInitialize_Event()
    {
        var engine = new Engine();
        var fired = false;
        engine.OnInitialize += () => fired = true;
        engine.Initialize();
        Assert.True(fired);
        engine.Shutdown();
    }

    [Fact]
    public void Update_Fires_OnUpdate_Event()
    {
        var engine = new Engine();
        engine.Initialize();

        float receivedDt = -1;
        engine.OnUpdate += dt => receivedDt = dt;
        engine.Update(0.016f);

        Assert.True(receivedDt >= 0);
        engine.Shutdown();
    }

    [Fact]
    public void Render_Fires_OnRender_Event()
    {
        var engine = new Engine();
        engine.Initialize();

        float receivedDt = -1;
        engine.OnRender += dt => receivedDt = dt;
        engine.Render(0.016f);

        Assert.Equal(0.016f, receivedDt);
        engine.Shutdown();
    }

    [Fact]
    public void Shutdown_Fires_OnShutdown_Event()
    {
        var engine = new Engine();
        engine.Initialize();

        var fired = false;
        engine.OnShutdown += () => fired = true;
        engine.Shutdown();

        Assert.True(fired);
        Assert.False(engine.IsRunning);
    }

    [Fact]
    public void Stop_Sets_IsRunning_False()
    {
        var engine = new Engine();
        engine.Initialize();
        engine.Stop();
        Assert.False(engine.IsRunning);
        engine.Shutdown();
    }
}
