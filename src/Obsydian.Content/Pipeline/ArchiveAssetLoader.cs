namespace Obsydian.Content.Pipeline;

/// <summary>
/// Asset loader that reads from a ContentArchive instead of the file system.
/// Wraps an existing IAssetLoader and redirects its file reads through the archive.
///
/// Usage:
///   var archive = new ContentArchive("game.pak");
///   var archiveLoader = new ArchiveAssetLoader&lt;Texture&gt;(textureLoader, archive);
///   content.RegisterLoader(archiveLoader);
///   // Now content.Load&lt;Texture&gt;("sprites/player.png") reads from the pak
/// </summary>
public sealed class ArchiveAssetLoader<T> : IAssetLoader<T> where T : class
{
    private readonly IAssetLoader<T> _inner;
    private readonly ContentArchive _archive;

    public ArchiveAssetLoader(IAssetLoader<T> inner, ContentArchive archive)
    {
        _inner = inner;
        _archive = archive;
    }

    public T Load(string fullPath)
    {
        // Extract the relative path by removing the content root prefix
        // The fullPath comes from ContentManager as Path.Combine(rootPath, assetPath)
        // We need the original assetPath to look up in the archive
        var fileName = Path.GetFileName(fullPath);

        // Try to find the entry in the archive by checking if it contains the path
        // First try the full path, then just the filename
        foreach (var entryPath in _archive.EntryPaths)
        {
            if (fullPath.EndsWith(entryPath, StringComparison.OrdinalIgnoreCase) ||
                entryPath.EndsWith(fileName, StringComparison.OrdinalIgnoreCase))
            {
                // Extract to a temp file and load via the inner loader
                var tempPath = Path.Combine(Path.GetTempPath(), $"obsydian_pak_{Guid.NewGuid():N}_{fileName}");
                try
                {
                    var bytes = _archive.ReadBytes(entryPath);
                    File.WriteAllBytes(tempPath, bytes);
                    return _inner.Load(tempPath);
                }
                finally
                {
                    if (File.Exists(tempPath))
                        File.Delete(tempPath);
                }
            }
        }

        // Fallback: try loading from the file system directly
        return _inner.Load(fullPath);
    }
}
