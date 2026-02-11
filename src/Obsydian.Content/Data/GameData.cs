namespace Obsydian.Content.Data;

/// <summary>
/// Base interface for all game data types. Implementing this interface
/// marks a class as loadable via DataLoader and provides a consistent
/// contract for the content pipeline.
/// </summary>
public interface IGameData
{
    /// <summary>Unique identifier for this data entry.</summary>
    string Id { get; }
}

/// <summary>
/// A collection of game data entries loaded from a JSON array.
/// Provides indexed and keyed access to data.
///
/// Expected JSON format:
/// [
///   { "id": "sword_iron", "name": "Iron Sword", "damage": 10 },
///   { "id": "sword_steel", "name": "Steel Sword", "damage": 25 }
/// ]
/// </summary>
public sealed class GameDataCollection<T> where T : class, IGameData
{
    private readonly List<T> _items;
    private readonly Dictionary<string, T> _byId;

    public IReadOnlyList<T> Items => _items;
    public int Count => _items.Count;

    public GameDataCollection(List<T> items)
    {
        _items = items;
        _byId = new Dictionary<string, T>(StringComparer.OrdinalIgnoreCase);
        foreach (var item in items)
        {
            if (!string.IsNullOrEmpty(item.Id))
                _byId[item.Id] = item;
        }
    }

    /// <summary>Look up a data entry by its ID.</summary>
    public T? GetById(string id) => _byId.TryGetValue(id, out var item) ? item : null;

    /// <summary>Check if an entry with the given ID exists.</summary>
    public bool Contains(string id) => _byId.ContainsKey(id);

    /// <summary>Get all IDs in this collection.</summary>
    public IEnumerable<string> GetIds() => _byId.Keys;
}
