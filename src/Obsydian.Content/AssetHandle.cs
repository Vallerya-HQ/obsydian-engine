namespace Obsydian.Content;

/// <summary>
/// Reference-counted handle to a loaded asset. When disposed, decrements
/// the ref count in ContentManager. The asset is evicted when all handles
/// are released.
///
/// Usage:
///   using var sprite = content.LoadHandle&lt;Texture&gt;("sprites/player.png");
///   renderer.Draw(sprite.Asset, position);
///   // Auto-released when scope exits
/// </summary>
public sealed class AssetHandle<T> : IDisposable where T : class
{
    private readonly ContentManager _content;
    private readonly string _assetPath;
    private bool _disposed;

    /// <summary>The loaded asset.</summary>
    public T Asset { get; }

    internal AssetHandle(ContentManager content, string assetPath, T asset)
    {
        _content = content;
        _assetPath = assetPath;
        Asset = asset;
    }

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;
        _content.Release<T>(_assetPath);
    }

    /// <summary>Implicit conversion to the underlying asset type for convenience.</summary>
    public static implicit operator T(AssetHandle<T> handle) => handle.Asset;
}

/// <summary>
/// Extension methods for ContentManager to work with AssetHandle.
/// </summary>
public static class AssetHandleExtensions
{
    /// <summary>
    /// Load an asset and return a ref-counted handle. Dispose the handle
    /// when done to allow the asset to be evicted.
    /// </summary>
    public static AssetHandle<T> LoadHandle<T>(this ContentManager content, string assetPath) where T : class
    {
        var asset = content.Acquire<T>(assetPath);
        return new AssetHandle<T>(content, assetPath, asset);
    }

    /// <summary>
    /// Async version of LoadHandle.
    /// </summary>
    public static async Task<AssetHandle<T>> LoadHandleAsync<T>(
        this ContentManager content, string assetPath, CancellationToken ct = default) where T : class
    {
        var asset = await content.LoadAsync<T>(assetPath, ct);
        // Acquire after async load to increment ref count
        content.Acquire<T>(assetPath); // already cached, just bumps ref
        return new AssetHandle<T>(content, assetPath, asset);
    }
}
