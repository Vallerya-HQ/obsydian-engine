using System.Diagnostics;

namespace Obsydian.Core.ECS;

/// <summary>
/// Manages the execution order and lifecycle of game systems.
/// </summary>
public sealed class SystemScheduler
{
    private readonly List<GameSystem> _systems = [];
    private readonly World _world;
    private readonly Stopwatch _stopwatch = new();
    private readonly List<(GameSystem System, double LastUpdateMs)> _timings = [];

    public SystemScheduler(World world)
    {
        _world = world;
    }

    public void Register(GameSystem system)
    {
        _systems.Add(system);
        system.Initialize(_world);
    }

    public void Remove(GameSystem system)
    {
        system.Shutdown();
        _systems.Remove(system);
    }

    public void UpdateAll(float deltaTime)
    {
        _timings.Clear();
        foreach (var system in _systems)
        {
            if (system.Enabled)
            {
                _stopwatch.Restart();
                system.Update(_world, deltaTime);
                _stopwatch.Stop();
                _timings.Add((system, _stopwatch.Elapsed.TotalMilliseconds));
            }
            else
            {
                _timings.Add((system, 0));
            }
        }
    }

    public void ShutdownAll()
    {
        foreach (var system in _systems)
            system.Shutdown();
        _systems.Clear();
        _timings.Clear();
    }

    public IReadOnlyList<GameSystem> Systems => _systems;

    /// <summary>Per-system timing from the last UpdateAll() call.</summary>
    public IReadOnlyList<(GameSystem System, double LastUpdateMs)> SystemTimings => _timings;
}
