using System.Text.Json;

namespace Obsydian.Content.Loaders;

/// <summary>
/// Generic JSON data loader. Deserializes JSON files into typed objects.
/// Register with ContentManager for each data type you need to load.
/// </summary>
public sealed class JsonDataLoader<T> : IAssetLoader<T> where T : class
{
    private static readonly JsonSerializerOptions Options = new()
    {
        PropertyNameCaseInsensitive = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        ReadCommentHandling = JsonCommentHandling.Skip,
        AllowTrailingCommas = true,
    };

    public T Load(string fullPath)
    {
        var json = File.ReadAllText(fullPath);
        return JsonSerializer.Deserialize<T>(json, Options)
            ?? throw new InvalidDataException($"Failed to deserialize {typeof(T).Name} from: {fullPath}");
    }
}

/// <summary>
/// Loads a list of items from a JSON array file.
/// </summary>
public sealed class JsonArrayLoader<T> : IAssetLoader<DataCollection<T>> where T : class
{
    private static readonly JsonSerializerOptions Options = new()
    {
        PropertyNameCaseInsensitive = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        ReadCommentHandling = JsonCommentHandling.Skip,
        AllowTrailingCommas = true,
    };

    public DataCollection<T> Load(string fullPath)
    {
        var json = File.ReadAllText(fullPath);
        var items = JsonSerializer.Deserialize<List<T>>(json, Options)
            ?? throw new InvalidDataException($"Failed to deserialize List<{typeof(T).Name}> from: {fullPath}");
        return new DataCollection<T>(items);
    }
}

/// <summary>
/// Wrapper for a list of data items loaded from JSON. Implements IDisposable for ContentManager compatibility.
/// </summary>
public sealed class DataCollection<T> where T : class
{
    public IReadOnlyList<T> Items { get; }

    public DataCollection(List<T> items)
    {
        Items = items;
    }
}
