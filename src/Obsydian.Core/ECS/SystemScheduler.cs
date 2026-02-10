namespace Obsydian.Core.ECS;

/// <summary>
/// Manages the execution order and lifecycle of game systems.
/// </summary>
public sealed class SystemScheduler
{
    private readonly List<GameSystem> _systems = [];
    private readonly World _world;

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
        foreach (var system in _systems)
        {
            if (system.Enabled)
                system.Update(_world, deltaTime);
        }
    }

    public void ShutdownAll()
    {
        foreach (var system in _systems)
            system.Shutdown();
        _systems.Clear();
    }

    public IReadOnlyList<GameSystem> Systems => _systems;
}
