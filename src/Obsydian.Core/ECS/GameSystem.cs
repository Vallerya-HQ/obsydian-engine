namespace Obsydian.Core.ECS;

/// <summary>
/// Base class for all game systems. Systems contain behavior — they
/// operate on entities that have specific component combinations.
/// </summary>
public abstract class GameSystem
{
    public bool Enabled { get; set; } = true;

    /// <summary>
    /// Called once when the system is registered with the engine.
    /// </summary>
    public virtual void Initialize(World world) { }

    /// <summary>
    /// Called every frame with the delta time in seconds.
    /// </summary>
    public abstract void Update(World world, float deltaTime);

    /// <summary>
    /// Called when the system is removed or the engine shuts down.
    /// </summary>
    public virtual void Shutdown() { }
}
