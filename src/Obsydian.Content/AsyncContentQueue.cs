using Obsydian.Core.Logging;

namespace Obsydian.Content;

/// <summary>
/// Priority-based async content loading queue. Runs loads on the ThreadPool
/// and provides progress tracking for loading screens.
/// </summary>
public sealed class AsyncContentQueue
{
    private readonly ContentManager _content;
    private int _totalQueued;
    private int _loaded;

    public int TotalQueued => _totalQueued;
    public int Loaded => Volatile.Read(ref _loaded);
    public float Progress => _totalQueued > 0 ? (float)Loaded / _totalQueued : 1f;
    public bool IsComplete => Loaded >= _totalQueued;

    public event Action<string>? OnAssetLoaded;

    public AsyncContentQueue(ContentManager content)
    {
        _content = content;
    }

    /// <summary>
    /// Queue an asset for async loading. Returns a task that completes with the loaded asset.
    /// </summary>
    public Task<T> QueueLoad<T>(string assetPath, CancellationToken ct = default) where T : class
    {
        Interlocked.Increment(ref _totalQueued);

        return Task.Run(() =>
        {
            var result = _content.Load<T>(assetPath);
            Interlocked.Increment(ref _loaded);
            OnAssetLoaded?.Invoke(assetPath);
            return result;
        }, ct);
    }

    /// <summary>
    /// Preload multiple assets concurrently. Returns when all are loaded.
    /// </summary>
    public async Task PreloadAsync<T>(string[] assetPaths, CancellationToken ct = default) where T : class
    {
        var tasks = new Task[assetPaths.Length];
        for (int i = 0; i < assetPaths.Length; i++)
            tasks[i] = QueueLoad<T>(assetPaths[i], ct);

        await Task.WhenAll(tasks);
    }

    public void Reset()
    {
        _totalQueued = 0;
        _loaded = 0;
    }
}
