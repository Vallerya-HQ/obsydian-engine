using Obsydian.Core.Logging;

namespace Obsydian.Content.Pipeline;

/// <summary>
/// File system watcher for content hot-reload during development.
/// Watches a content directory for changes and notifies the ContentManager
/// to invalidate and reload affected assets.
///
/// Usage:
///   var watcher = new ContentWatcher(content, "content/");
///   watcher.Start();
///   // ... game loop ...
///   watcher.ProcessChanges(); // Call each frame to apply pending reloads
///   watcher.Stop();
/// </summary>
public sealed class ContentWatcher : IDisposable
{
    private readonly ContentManager _content;
    private readonly string _watchPath;
    private FileSystemWatcher? _watcher;
    private readonly HashSet<string> _pendingReloads = new(StringComparer.OrdinalIgnoreCase);
    private readonly object _lock = new();
    private readonly TimeSpan _debounce = TimeSpan.FromMilliseconds(200);
    private readonly Dictionary<string, DateTime> _lastChanged = [];

    /// <summary>Whether the watcher is currently active.</summary>
    public bool IsWatching => _watcher?.EnableRaisingEvents ?? false;

    /// <summary>Number of pending asset reloads.</summary>
    public int PendingCount { get { lock (_lock) return _pendingReloads.Count; } }

    /// <summary>Fired when an asset is about to be reloaded.</summary>
    public event Action<string>? OnAssetReloading;

    /// <summary>Fired after an asset has been reloaded successfully.</summary>
    public event Action<string>? OnAssetReloaded;

    public ContentWatcher(ContentManager content, string watchPath)
    {
        _content = content;
        _watchPath = Path.GetFullPath(watchPath);
    }

    /// <summary>Start watching for file changes.</summary>
    public void Start()
    {
        if (_watcher is not null) return;

        if (!Directory.Exists(_watchPath))
        {
            Log.Warn("HotReload", $"Watch path does not exist: {_watchPath}");
            return;
        }

        _watcher = new FileSystemWatcher(_watchPath)
        {
            IncludeSubdirectories = true,
            NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.FileName | NotifyFilters.Size,
        };

        _watcher.Changed += OnFileChanged;
        _watcher.Created += OnFileChanged;
        _watcher.Renamed += (_, e) => QueueReload(e.FullPath);
        _watcher.EnableRaisingEvents = true;

        Log.Info("HotReload", $"Watching for content changes: {_watchPath}");
    }

    /// <summary>Stop watching for file changes.</summary>
    public void Stop()
    {
        if (_watcher is null) return;
        _watcher.EnableRaisingEvents = false;
        _watcher.Dispose();
        _watcher = null;
        Log.Info("HotReload", "Stopped watching for content changes");
    }

    /// <summary>
    /// Process any pending reload requests. Call this once per frame
    /// from the main thread (since some loaders need GL context).
    /// Returns the number of assets reloaded.
    /// </summary>
    public int ProcessChanges()
    {
        List<string> toReload;
        lock (_lock)
        {
            if (_pendingReloads.Count == 0) return 0;
            toReload = [.. _pendingReloads];
            _pendingReloads.Clear();
        }

        int reloaded = 0;
        foreach (var assetPath in toReload)
        {
            try
            {
                OnAssetReloading?.Invoke(assetPath);
                _content.Unload(assetPath);
                Log.Debug("HotReload", $"Reloaded: {assetPath}");
                OnAssetReloaded?.Invoke(assetPath);
                reloaded++;
            }
            catch (Exception ex)
            {
                Log.Warn("HotReload", $"Failed to reload {assetPath}: {ex.Message}");
            }
        }

        return reloaded;
    }

    public void Dispose() => Stop();

    private void OnFileChanged(object sender, FileSystemEventArgs e) => QueueReload(e.FullPath);

    private void QueueReload(string fullPath)
    {
        // Convert to relative asset path
        if (!fullPath.StartsWith(_watchPath, StringComparison.OrdinalIgnoreCase))
            return;

        var relativePath = Path.GetRelativePath(_content.RootPath, fullPath).Replace('\\', '/');

        // Debounce: ignore rapid-fire events for the same file
        var now = DateTime.UtcNow;
        lock (_lock)
        {
            if (_lastChanged.TryGetValue(relativePath, out var lastTime) && (now - lastTime) < _debounce)
                return;

            _lastChanged[relativePath] = now;
            _pendingReloads.Add(relativePath);
        }
    }
}
