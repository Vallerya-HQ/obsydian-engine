namespace Obsydian.Core.ECS;

/// <summary>
/// Lightweight identifier for an entity in the ECS world.
/// An entity is just an ID — all data lives in components.
/// </summary>
public readonly record struct Entity(int Id)
{
    public static readonly Entity None = new(-1);

    public bool IsValid => Id >= 0;

    public override string ToString() => $"Entity({Id})";
}
