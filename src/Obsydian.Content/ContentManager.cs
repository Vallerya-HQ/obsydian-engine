using System.Collections.Concurrent;
using Obsydian.Core.Logging;

namespace Obsydian.Content;

/// <summary>
/// Manages loading, caching, and unloading of game assets.
/// Thread-safe for concurrent loading from multiple threads.
/// </summary>
public sealed class ContentManager : IDisposable
{
    private readonly ConcurrentDictionary<string, object> _cache = new();
    private readonly ConcurrentDictionary<Type, IAssetLoader> _loaders = new();
    private readonly ConcurrentDictionary<string, int> _refCounts = new();
    private readonly string _rootPath;

    /// <summary>The root content directory path.</summary>
    public string RootPath => _rootPath;

    /// <summary>Number of currently cached assets.</summary>
    public int CachedCount => _cache.Count;

    /// <summary>Fired after an asset is loaded (on the loading thread).</summary>
    public event Action<string, Type>? OnAssetLoaded;

    /// <summary>Fired after an asset is unloaded.</summary>
    public event Action<string>? OnAssetUnloaded;

    public ContentManager(string rootPath)
    {
        _rootPath = rootPath;
    }

    public void RegisterLoader<T>(IAssetLoader<T> loader) where T : class
    {
        _loaders[typeof(T)] = loader;
    }

    /// <summary>Check whether a loader is registered for the given type.</summary>
    public bool HasLoader<T>() where T : class => _loaders.ContainsKey(typeof(T));

    /// <summary>Check whether an asset is already cached.</summary>
    public bool IsCached<T>(string assetPath) where T : class
        => _cache.ContainsKey(CacheKey<T>(assetPath));

    public T Load<T>(string assetPath) where T : class
    {
        var key = CacheKey<T>(assetPath);

        if (_cache.TryGetValue(key, out var cached))
            return (T)cached;

        if (!_loaders.TryGetValue(typeof(T), out var loader))
            throw new InvalidOperationException($"No loader registered for type {typeof(T).Name}");

        var fullPath = Path.Combine(_rootPath, assetPath);

        try
        {
            var asset = ((IAssetLoader<T>)loader).Load(fullPath);
            var final = (T)_cache.GetOrAdd(key, asset);

            if (ReferenceEquals(final, asset))
            {
                Log.Debug("Content", $"Loaded {typeof(T).Name}: {assetPath}");
                OnAssetLoaded?.Invoke(assetPath, typeof(T));
            }

            return final;
        }
        catch (ContentLoadException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw new ContentLoadException(assetPath, typeof(T), ex);
        }
    }

    /// <summary>
    /// Asynchronously load an asset. Uses IAsyncAssetLoader if available, otherwise wraps sync load.
    /// </summary>
    public async Task<T> LoadAsync<T>(string assetPath, CancellationToken ct = default) where T : class
    {
        var key = CacheKey<T>(assetPath);

        if (_cache.TryGetValue(key, out var cached))
            return (T)cached;

        if (!_loaders.TryGetValue(typeof(T), out var loader))
            throw new InvalidOperationException($"No loader registered for type {typeof(T).Name}");

        var fullPath = Path.Combine(_rootPath, assetPath);

        try
        {
            T asset;
            if (loader is IAsyncAssetLoader<T> asyncLoader)
            {
                asset = await asyncLoader.LoadAsync(fullPath, ct);
            }
            else
            {
                asset = await Task.Run(() => ((IAssetLoader<T>)loader).Load(fullPath), ct);
            }

            var final = (T)_cache.GetOrAdd(key, asset);

            if (ReferenceEquals(final, asset))
            {
                Log.Debug("Content", $"Loaded async {typeof(T).Name}: {assetPath}");
                OnAssetLoaded?.Invoke(assetPath, typeof(T));
            }

            return final;
        }
        catch (ContentLoadException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw new ContentLoadException(assetPath, typeof(T), ex);
        }
    }

    /// <summary>
    /// Load and increment ref count. Used by AssetHandle and ContentScope.
    /// Each Acquire must be paired with a Release.
    /// </summary>
    public T Acquire<T>(string assetPath) where T : class
    {
        var asset = Load<T>(assetPath);
        var key = CacheKey<T>(assetPath);
        _refCounts.AddOrUpdate(key, 1, (_, c) => c + 1);
        return asset;
    }

    /// <summary>
    /// Decrement ref count for an asset. Disposes and removes when count reaches zero.
    /// </summary>
    public void Release<T>(string assetPath) where T : class
    {
        var key = CacheKey<T>(assetPath);
        var newCount = _refCounts.AddOrUpdate(key, 0, (_, c) => System.Math.Max(0, c - 1));
        if (newCount <= 0)
            EvictKey(key, assetPath);
    }

    /// <summary>Force-unload an asset regardless of ref count.</summary>
    public void Unload(string assetPath)
    {
        var keysToRemove = _cache.Keys.Where(k => k.EndsWith($":{assetPath}")).ToList();
        foreach (var key in keysToRemove)
            EvictKey(key, assetPath);
    }

    /// <summary>Force-unload all cached assets.</summary>
    public void UnloadAll()
    {
        foreach (var kvp in _cache)
        {
            if (kvp.Value is IDisposable disposable)
                disposable.Dispose();
        }
        _cache.Clear();
        _refCounts.Clear();
    }

    /// <summary>Get the ref count for a cached asset (0 if not cached).</summary>
    public int GetRefCount<T>(string assetPath) where T : class
    {
        var key = CacheKey<T>(assetPath);
        return _refCounts.TryGetValue(key, out var count) ? count : 0;
    }

    /// <summary>Enumerate all currently cached asset keys.</summary>
    public IEnumerable<string> GetCachedKeys() => _cache.Keys;

    public void Dispose() => UnloadAll();

    private static string CacheKey<T>(string assetPath) => $"{typeof(T).Name}:{assetPath}";

    private void EvictKey(string key, string assetPath)
    {
        if (_cache.TryRemove(key, out var removed))
        {
            if (removed is IDisposable disposable)
                disposable.Dispose();
            _refCounts.TryRemove(key, out _);
            OnAssetUnloaded?.Invoke(assetPath);
        }
    }
}

public interface IAssetLoader;

public interface IAssetLoader<out T> : IAssetLoader where T : class
{
    T Load(string fullPath);
}
