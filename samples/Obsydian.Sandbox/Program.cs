using Obsydian.Core;
using Obsydian.Core.ECS;
using Obsydian.Core.Logging;
using Obsydian.Core.Math;
using Obsydian.Physics;

// ── Example: Obsydian Engine Sandbox ──────────────────────────────
// This demonstrates the basic ECS loop with the engine.

var engine = new Engine(new EngineConfig
{
    Title = "Obsydian Sandbox",
    TargetFps = 60,
    Version = "0.1.0"
});

// Register a simple movement system
engine.Systems.Register(new MovementSystem());

engine.OnInitialize += () =>
{
    Log.Info("Sandbox", "Creating test entities...");

    // Spawn some entities with position and velocity
    for (int i = 0; i < 5; i++)
    {
        var entity = engine.World.CreateEntity();
        engine.World.Add(entity, new PositionComponent(i * 100f, 0));
        engine.World.Add(entity, new VelocityComponent(10f + i * 5f, 0));
    }

    Log.Info("Sandbox", $"Created {engine.World.EntityCount} entities");
};

// Stop after 3 seconds for demo purposes
engine.OnUpdate += dt =>
{
    if (engine.Time.TotalTime > 3.0)
    {
        Log.Info("Sandbox", "Demo complete. Stopping engine.");
        engine.Stop();
    }
};

engine.Run();

// ── Components ────────────────────────────────────────────────────

record struct PositionComponent(float X, float Y) : IComponent;
record struct VelocityComponent(float Dx, float Dy) : IComponent;

// ── Systems ───────────────────────────────────────────────────────

class MovementSystem : GameSystem
{
    private int _logCounter;

    public override void Update(World world, float deltaTime)
    {
        foreach (var entity in world.Query<PositionComponent, VelocityComponent>())
        {
            ref var pos = ref world.Get<PositionComponent>(entity);
            var vel = world.Get<VelocityComponent>(entity);

            pos = new PositionComponent(pos.X + vel.Dx * deltaTime, pos.Y + vel.Dy * deltaTime);
        }

        // Log every ~60 frames
        _logCounter++;
        if (_logCounter % 60 == 0)
        {
            foreach (var entity in world.Query<PositionComponent>())
            {
                var pos = world.Get<PositionComponent>(entity);
                Log.Debug("Movement", $"{entity} at ({pos.X:F1}, {pos.Y:F1})");
            }
        }
    }
}
