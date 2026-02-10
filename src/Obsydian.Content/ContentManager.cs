using Obsydian.Core.Logging;

namespace Obsydian.Content;

/// <summary>
/// Manages loading, caching, and unloading of game assets.
/// Supports textures, sounds, data files, and custom asset types.
/// </summary>
public sealed class ContentManager : IDisposable
{
    private readonly Dictionary<string, object> _cache = [];
    private readonly Dictionary<Type, IAssetLoader> _loaders = [];
    private readonly string _rootPath;

    /// <summary>The root content directory path.</summary>
    public string RootPath => _rootPath;

    public ContentManager(string rootPath)
    {
        _rootPath = rootPath;
    }

    public void RegisterLoader<T>(IAssetLoader<T> loader) where T : class
    {
        _loaders[typeof(T)] = loader;
    }

    public T Load<T>(string assetPath) where T : class
    {
        var key = $"{typeof(T).Name}:{assetPath}";

        if (_cache.TryGetValue(key, out var cached))
            return (T)cached;

        if (!_loaders.TryGetValue(typeof(T), out var loader))
            throw new InvalidOperationException($"No loader registered for type {typeof(T).Name}");

        var fullPath = Path.Combine(_rootPath, assetPath);
        var asset = ((IAssetLoader<T>)loader).Load(fullPath);
        _cache[key] = asset;

        Log.Debug("Content", $"Loaded {typeof(T).Name}: {assetPath}");
        return asset;
    }

    /// <summary>
    /// Asynchronously load an asset. Uses IAsyncAssetLoader if available, otherwise wraps sync load.
    /// </summary>
    public async Task<T> LoadAsync<T>(string assetPath, CancellationToken ct = default) where T : class
    {
        var key = $"{typeof(T).Name}:{assetPath}";

        if (_cache.TryGetValue(key, out var cached))
            return (T)cached;

        if (!_loaders.TryGetValue(typeof(T), out var loader))
            throw new InvalidOperationException($"No loader registered for type {typeof(T).Name}");

        var fullPath = Path.Combine(_rootPath, assetPath);

        T asset;
        if (loader is IAsyncAssetLoader<T> asyncLoader)
        {
            asset = await asyncLoader.LoadAsync(fullPath, ct);
        }
        else
        {
            asset = await Task.Run(() => ((IAssetLoader<T>)loader).Load(fullPath), ct);
        }

        _cache[key] = asset;
        Log.Debug("Content", $"Loaded async {typeof(T).Name}: {assetPath}");
        return asset;
    }

    public void Unload(string assetPath)
    {
        var keysToRemove = _cache.Keys.Where(k => k.EndsWith($":{assetPath}")).ToList();
        foreach (var key in keysToRemove)
        {
            if (_cache[key] is IDisposable disposable)
                disposable.Dispose();
            _cache.Remove(key);
        }
    }

    public void UnloadAll()
    {
        foreach (var asset in _cache.Values)
        {
            if (asset is IDisposable disposable)
                disposable.Dispose();
        }
        _cache.Clear();
    }

    public void Dispose() => UnloadAll();
}

public interface IAssetLoader
{
}

public interface IAssetLoader<out T> : IAssetLoader where T : class
{
    T Load(string fullPath);
}
