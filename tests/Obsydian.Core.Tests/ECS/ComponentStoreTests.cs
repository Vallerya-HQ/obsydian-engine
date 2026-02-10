using Obsydian.Core.ECS;

namespace Obsydian.Core.Tests.ECS;

public class ComponentStoreTests
{
    [Fact]
    public void SetAndGet_ReturnsComponent()
    {
        var store = new ComponentStore<Position>();
        store.Set(0, new Position(10, 20));

        ref var pos = ref store.Get(0);

        Assert.Equal(10, pos.X);
        Assert.Equal(20, pos.Y);
    }

    [Fact]
    public void Set_OverwritesExisting()
    {
        var store = new ComponentStore<Position>();
        store.Set(0, new Position(10, 20));
        store.Set(0, new Position(30, 40));

        Assert.Equal(1, store.Count);
        Assert.Equal(30, store.Get(0).X);
    }

    [Fact]
    public void Has_ReturnsFalseForMissing()
    {
        var store = new ComponentStore<Position>();

        Assert.False(store.Has(0));
        Assert.False(store.Has(999));
        Assert.False(store.Has(-1));
    }

    [Fact]
    public void Remove_SwapRemove_KeepsOtherEntities()
    {
        var store = new ComponentStore<Position>();
        store.Set(0, new Position(1, 1));
        store.Set(1, new Position(2, 2));
        store.Set(2, new Position(3, 3));

        store.Remove(0); // swap-removes: entity 2's data moves to index 0

        Assert.Equal(2, store.Count);
        Assert.False(store.Has(0));
        Assert.True(store.Has(1));
        Assert.True(store.Has(2));
        Assert.Equal(2, store.Get(1).X);
        Assert.Equal(3, store.Get(2).X);
    }

    [Fact]
    public void Remove_LastElement_DecreasesCount()
    {
        var store = new ComponentStore<Position>();
        store.Set(5, new Position(1, 1));

        store.Remove(5);

        Assert.Equal(0, store.Count);
        Assert.False(store.Has(5));
    }

    [Fact]
    public void AsSpan_ReturnsAllComponents()
    {
        var store = new ComponentStore<Position>();
        store.Set(0, new Position(1, 1));
        store.Set(1, new Position(2, 2));
        store.Set(2, new Position(3, 3));

        var span = store.AsSpan();

        Assert.Equal(3, span.Length);
    }

    [Fact]
    public void Get_ThrowsForMissingEntity()
    {
        var store = new ComponentStore<Position>();

        Assert.Throws<KeyNotFoundException>(() => store.Get(42));
    }

    [Fact]
    public void GrowsSparseArray_ForLargeEntityIds()
    {
        var store = new ComponentStore<Position>(4);
        store.Set(1000, new Position(99, 99));

        Assert.True(store.Has(1000));
        Assert.Equal(99, store.Get(1000).X);
    }

    [Fact]
    public void GetEntityAt_ReturnsDenseMapping()
    {
        var store = new ComponentStore<Position>();
        store.Set(5, new Position(1, 1));
        store.Set(10, new Position(2, 2));

        Assert.Equal(5, store.GetEntityAt(0));
        Assert.Equal(10, store.GetEntityAt(1));
    }

    [Fact]
    public void RefReturn_AllowsMutation()
    {
        var store = new ComponentStore<Position>();
        store.Set(0, new Position(1, 1));

        ref var pos = ref store.Get(0);
        pos = new Position(99, 99);

        Assert.Equal(99, store.Get(0).X);
    }
}
