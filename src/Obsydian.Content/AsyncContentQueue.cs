using Obsydian.Core.Logging;

namespace Obsydian.Content;

/// <summary>
/// Async content loading queue with error tracking.
/// Thread-safe — multiple producers can queue, progress is atomic.
/// </summary>
public sealed class AsyncContentQueue
{
    private readonly ContentManager _content;
    private int _totalQueued;
    private int _loaded;
    private int _failed;

    public int TotalQueued => Volatile.Read(ref _totalQueued);
    public int Loaded => Volatile.Read(ref _loaded);
    public int Failed => Volatile.Read(ref _failed);
    public float Progress => _totalQueued > 0 ? (float)(Loaded + Failed) / _totalQueued : 1f;
    public bool IsComplete => (Loaded + Failed) >= TotalQueued;
    public bool HasErrors => Failed > 0;

    /// <summary>Fired when an asset finishes loading successfully.</summary>
    public event Action<string>? OnAssetLoaded;

    /// <summary>Fired when an asset fails to load, with the exception.</summary>
    public event Action<string, Exception>? OnAssetFailed;

    public AsyncContentQueue(ContentManager content)
    {
        _content = content;
    }

    /// <summary>
    /// Queue an asset for async loading.
    /// </summary>
    public Task<T> QueueLoad<T>(string assetPath, CancellationToken ct = default) where T : class
    {
        Interlocked.Increment(ref _totalQueued);
        return LoadAndTrack<T>(assetPath, ct);
    }

    /// <summary>
    /// Preload multiple assets of the same type concurrently.
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
        _failed = 0;
    }

    private async Task<T> LoadAndTrack<T>(string assetPath, CancellationToken ct) where T : class
    {
        try
        {
            var result = await _content.LoadAsync<T>(assetPath, ct);
            Interlocked.Increment(ref _loaded);
            OnAssetLoaded?.Invoke(assetPath);
            return result;
        }
        catch (Exception ex)
        {
            Interlocked.Increment(ref _failed);
            Log.Warn("Content", $"Failed to load {assetPath}: {ex.Message}");
            OnAssetFailed?.Invoke(assetPath, ex);
            throw;
        }
    }
}
