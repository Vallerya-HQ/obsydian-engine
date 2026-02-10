using Obsydian.Core.ECS;
using Obsydian.Core.Math;

namespace Obsydian.Core.Tests.ECS;

public record struct Position(float X, float Y) : IComponent;
public record struct Velocity(float Dx, float Dy) : IComponent;
public record struct Health(int Current, int Max) : IComponent;

public class WorldTests
{
    [Fact]
    public void CreateEntity_ReturnsUniqueEntities()
    {
        var world = new World();
        var e1 = world.CreateEntity();
        var e2 = world.CreateEntity();

        Assert.NotEqual(e1, e2);
        Assert.True(e1.IsValid);
        Assert.True(e2.IsValid);
    }

    [Fact]
    public void AddAndGet_Component()
    {
        var world = new World();
        var entity = world.CreateEntity();

        world.Add(entity, new Position(10, 20));

        Assert.True(world.Has<Position>(entity));
        ref var pos = ref world.Get<Position>(entity);
        Assert.Equal(10, pos.X);
        Assert.Equal(20, pos.Y);
    }

    [Fact]
    public void DestroyEntity_RemovesAllComponents()
    {
        var world = new World();
        var entity = world.CreateEntity();
        world.Add(entity, new Position(0, 0));
        world.Add(entity, new Health(100, 100));

        world.DestroyEntity(entity);

        Assert.False(world.IsAlive(entity));
        Assert.False(world.Has<Position>(entity));
        Assert.False(world.Has<Health>(entity));
    }

    [Fact]
    public void Query_ReturnsMatchingEntities()
    {
        var world = new World();

        var e1 = world.CreateEntity();
        world.Add(e1, new Position(0, 0));
        world.Add(e1, new Velocity(1, 1));

        var e2 = world.CreateEntity();
        world.Add(e2, new Position(5, 5));
        // no velocity

        var e3 = world.CreateEntity();
        world.Add(e3, new Position(10, 10));
        world.Add(e3, new Velocity(2, 2));

        var results = world.Query<Position, Velocity>().ToList();

        Assert.Equal(2, results.Count);
        Assert.Contains(e1, results);
        Assert.Contains(e3, results);
        Assert.DoesNotContain(e2, results);
    }

    [Fact]
    public void EntityCount_TracksCorrectly()
    {
        var world = new World();
        Assert.Equal(0, world.EntityCount);

        var e1 = world.CreateEntity();
        var e2 = world.CreateEntity();
        Assert.Equal(2, world.EntityCount);

        world.DestroyEntity(e1);
        Assert.Equal(1, world.EntityCount);
    }
}
