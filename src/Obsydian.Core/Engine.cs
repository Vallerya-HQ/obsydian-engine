using Obsydian.Core.ECS;
using Obsydian.Core.Events;
using Obsydian.Core.Logging;

namespace Obsydian.Core;

/// <summary>
/// The Obsydian Game Engine. Orchestrates the game loop, ECS world, and all subsystems.
/// Games create an Engine instance, register systems, and call Run().
/// </summary>
public sealed class Engine
{
    public World World { get; } = new();
    public EventBus Events { get; } = new();
    public SystemScheduler Systems { get; }
    public GameTime Time { get; } = new();
    public EngineConfig Config { get; }

    public bool IsRunning { get; private set; }

    /// <summary>Called once before the first frame.</summary>
    public event Action? OnInitialize;

    /// <summary>Called every frame before systems update.</summary>
    public event Action<float>? OnUpdate;

    /// <summary>Called every frame after systems update, for rendering.</summary>
    public event Action<float>? OnRender;

    /// <summary>Called when the engine is shutting down.</summary>
    public event Action? OnShutdown;

    public Engine(EngineConfig? config = null)
    {
        Config = config ?? new EngineConfig();
        Systems = new SystemScheduler(World);
    }

    public void Run()
    {
        Log.Info("Engine", $"Obsydian Engine v{Config.Version} starting...");
        Log.Info("Engine", $"Target FPS: {Config.TargetFps}");

        IsRunning = true;
        Time.Start();

        OnInitialize?.Invoke();

        var targetFrameTime = 1000.0 / Config.TargetFps;

        while (IsRunning)
        {
            Time.Tick();

            OnUpdate?.Invoke(Time.DeltaTime);
            Systems.UpdateAll(Time.DeltaTime);
            OnRender?.Invoke(Time.DeltaTime);

            // Frame rate limiter
            var elapsed = (Environment.TickCount64 - (long)(Time.TotalTime * 1000) + (long)(Time.DeltaTime * 1000));
            var sleepTime = (int)(targetFrameTime - Time.DeltaTime * 1000);
            if (sleepTime > 0)
                Thread.Sleep(sleepTime);
        }

        Shutdown();
    }

    public void Stop()
    {
        IsRunning = false;
    }

    private void Shutdown()
    {
        Log.Info("Engine", "Shutting down...");
        OnShutdown?.Invoke();
        Systems.ShutdownAll();
        Events.Clear();
        Log.Info("Engine", "Goodbye.");
    }
}

public sealed class EngineConfig
{
    public string Title { get; init; } = "Obsydian Game";
    public int WindowWidth { get; init; } = 1280;
    public int WindowHeight { get; init; } = 720;
    public int TargetFps { get; init; } = 60;
    public bool VSync { get; init; } = true;
    public bool Fullscreen { get; init; } = false;
    public string Version { get; init; } = "0.1.0";
}
