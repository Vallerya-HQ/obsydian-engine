using Obsydian.Core.Logging;

namespace Obsydian.Content.Data;

/// <summary>
/// Strongly-typed data loading registry. Games register their data types
/// (items, NPCs, crops, etc.) and this class provides type-safe loading
/// with caching via ContentManager.
///
/// Inspired by Stardew Valley's DataLoader pattern — centralizes all game
/// data access through a single typed API instead of scattered string paths.
///
/// Usage:
///   // Register data types during initialization
///   var dataLoader = new DataLoader(content);
///   dataLoader.Register&lt;ItemData&gt;("data/items.json");
///   dataLoader.Register&lt;CropData&gt;("data/crops.json");
///
///   // Load anywhere in the game
///   var items = dataLoader.Load&lt;ItemData&gt;();
///   var crops = dataLoader.Load&lt;CropData&gt;();
/// </summary>
public sealed class DataLoader
{
    private readonly ContentManager _content;
    private readonly Dictionary<Type, DataRegistration> _registrations = [];

    public DataLoader(ContentManager content)
    {
        _content = content;
    }

    /// <summary>
    /// Register a data type with its source asset path.
    /// The asset must be loadable via a registered IAssetLoader.
    /// </summary>
    public void Register<T>(string assetPath) where T : class
    {
        _registrations[typeof(T)] = new DataRegistration(assetPath, typeof(T));
        Log.Debug("DataLoader", $"Registered {typeof(T).Name} → {assetPath}");
    }

    /// <summary>
    /// Load data of a registered type. Cached by ContentManager.
    /// </summary>
    public T Load<T>() where T : class
    {
        if (!_registrations.TryGetValue(typeof(T), out var reg))
            throw new InvalidOperationException(
                $"Data type {typeof(T).Name} is not registered. Call Register<{typeof(T).Name}>() first.");

        return _content.Load<T>(reg.AssetPath);
    }

    /// <summary>
    /// Try to load data, returning null if the type isn't registered or loading fails.
    /// </summary>
    public T? TryLoad<T>() where T : class
    {
        if (!_registrations.TryGetValue(typeof(T), out var reg))
            return null;

        try
        {
            return _content.Load<T>(reg.AssetPath);
        }
        catch
        {
            return null;
        }
    }

    /// <summary>
    /// Reload a data type by unloading it from cache first.
    /// Useful for hot-reload during development.
    /// </summary>
    public T Reload<T>() where T : class
    {
        if (!_registrations.TryGetValue(typeof(T), out var reg))
            throw new InvalidOperationException($"Data type {typeof(T).Name} is not registered.");

        _content.Unload(reg.AssetPath);
        return _content.Load<T>(reg.AssetPath);
    }

    /// <summary>Check if a data type is registered.</summary>
    public bool IsRegistered<T>() where T : class => _registrations.ContainsKey(typeof(T));

    /// <summary>Get the asset path for a registered data type.</summary>
    public string? GetAssetPath<T>() where T : class
        => _registrations.TryGetValue(typeof(T), out var reg) ? reg.AssetPath : null;

    /// <summary>Get all registered type names and their paths.</summary>
    public IEnumerable<(string typeName, string assetPath)> GetRegistrations()
        => _registrations.Values.Select(r => (r.DataType.Name, r.AssetPath));

    private sealed record DataRegistration(string AssetPath, Type DataType);
}
