using Obsydian.Core.Logging;

namespace Obsydian.Content;

/// <summary>
/// Lifetime for content loaded within a scope.
/// Inspired by CryEngine's pak lifetime model.
/// </summary>
public enum ContentLifetime
{
    /// <summary>Unloaded when the scope is disposed (e.g., loading screen assets).</summary>
    Scoped,

    /// <summary>Kept until explicitly unloaded (e.g., level assets kept during gameplay).</summary>
    Level,

    /// <summary>Never auto-unloaded (e.g., UI textures, fonts, player sprite).</summary>
    Permanent,
}

/// <summary>
/// Scoped content loading context. Tracks which assets were loaded within
/// this scope and releases them all when the scope is disposed.
///
/// Usage:
///   using var levelContent = new ContentScope(content, "Level1");
///   var bg = levelContent.Load&lt;Texture&gt;("levels/1/background.png");
///   var map = levelContent.Load&lt;Tilemap&gt;("levels/1/map.tmj");
///   // All level assets released when scope exits
///
/// Permanent assets loaded through any scope are never auto-released.
/// </summary>
public sealed class ContentScope : IDisposable
{
    private readonly ContentManager _content;
    private readonly List<(string path, Type type)> _scopedAssets = [];
    private bool _disposed;

    /// <summary>Name of this scope (for debugging).</summary>
    public string Name { get; }

    /// <summary>Number of assets tracked by this scope.</summary>
    public int AssetCount => _scopedAssets.Count;

    public ContentScope(ContentManager content, string name)
    {
        _content = content;
        Name = name;
        Log.Debug("ContentScope", $"Created scope: {name}");
    }

    /// <summary>
    /// Load an asset within this scope. Scoped assets are released when
    /// the scope is disposed. Permanent assets are loaded but not tracked for release.
    /// </summary>
    public T Load<T>(string assetPath, ContentLifetime lifetime = ContentLifetime.Scoped) where T : class
    {
        var asset = lifetime == ContentLifetime.Scoped
            ? _content.Acquire<T>(assetPath)   // Tracked — ref count incremented
            : _content.Load<T>(assetPath);     // Permanent — just load, no ref tracking

        if (lifetime == ContentLifetime.Scoped)
            _scopedAssets.Add((assetPath, typeof(T)));

        return asset;
    }

    /// <summary>Async load within this scope.</summary>
    public async Task<T> LoadAsync<T>(string assetPath, ContentLifetime lifetime = ContentLifetime.Scoped,
        CancellationToken ct = default) where T : class
    {
        var asset = await _content.LoadAsync<T>(assetPath, ct);

        if (lifetime == ContentLifetime.Scoped)
        {
            _content.Acquire<T>(assetPath); // already cached, just bumps ref
            _scopedAssets.Add((assetPath, typeof(T)));
        }

        return asset;
    }

    /// <summary>
    /// Preload all assets listed in the manifest that match a path prefix.
    /// Returns the number of assets queued.
    /// </summary>
    public async Task<int> PreloadFromManifest<T>(
        ContentManifest manifest, string pathPrefix, ContentLifetime lifetime = ContentLifetime.Scoped,
        CancellationToken ct = default) where T : class
    {
        var assets = manifest.Search($"{pathPrefix}*").ToList();
        foreach (var entry in assets)
            await LoadAsync<T>(entry.Path, lifetime, ct);
        return assets.Count;
    }

    /// <summary>Release all scoped assets immediately.</summary>
    public void ReleaseAll()
    {
        foreach (var (path, type) in _scopedAssets)
        {
            // Use reflection to call Release<T> with the correct type
            var method = typeof(ContentManager)
                .GetMethod(nameof(ContentManager.Release))!
                .MakeGenericMethod(type);
            method.Invoke(_content, [path]);
        }

        Log.Debug("ContentScope", $"Released {_scopedAssets.Count} assets from scope: {Name}");
        _scopedAssets.Clear();
    }

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;
        ReleaseAll();
    }
}
