using System.Text.Json;

namespace Obsydian.Content.Data;

/// <summary>
/// Loads a GameDataCollection from a JSON array file.
/// Each entry must implement IGameData (have an Id property).
///
/// Register with ContentManager:
///   content.RegisterLoader(new GameDataLoader&lt;ItemData&gt;());
///   var items = content.Load&lt;GameDataCollection&lt;ItemData&gt;&gt;("data/items.json");
///   var sword = items.GetById("sword_iron");
/// </summary>
public sealed class GameDataLoader<T> : IAssetLoader<GameDataCollection<T>> where T : class, IGameData
{
    private static readonly JsonSerializerOptions Options = new()
    {
        PropertyNameCaseInsensitive = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        ReadCommentHandling = JsonCommentHandling.Skip,
        AllowTrailingCommas = true,
    };

    public GameDataCollection<T> Load(string fullPath)
    {
        if (!File.Exists(fullPath))
            throw new FileNotFoundException($"Data file not found: {fullPath}");

        var json = File.ReadAllText(fullPath);
        var items = JsonSerializer.Deserialize<List<T>>(json, Options)
            ?? throw new InvalidDataException($"Failed to deserialize {typeof(T).Name} collection from: {fullPath}");

        return new GameDataCollection<T>(items);
    }
}
