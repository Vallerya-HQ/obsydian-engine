using System.IO.Compression;
using Obsydian.Core.Logging;

namespace Obsydian.Content.Pipeline;

/// <summary>
/// ZIP-based content archive for shipping bundled assets.
/// Inspired by CryEngine's CCryPak virtual file system.
///
/// Packing:
///   ContentArchive.Pack("content/", "game.pak");
///
/// Reading:
///   using var archive = new ContentArchive("game.pak");
///   var bytes = archive.ReadBytes("sprites/player.png");
///   var stream = archive.OpenRead("music/title.ogg");
/// </summary>
public sealed class ContentArchive : IDisposable
{
    private readonly ZipArchive _zip;
    private readonly Dictionary<string, ZipArchiveEntry> _entryMap;

    /// <summary>Number of entries in the archive.</summary>
    public int EntryCount => _zip.Entries.Count;

    /// <summary>All entry paths in the archive.</summary>
    public IEnumerable<string> EntryPaths => _entryMap.Keys;

    /// <summary>Open an existing archive for reading.</summary>
    public ContentArchive(string archivePath)
    {
        if (!File.Exists(archivePath))
            throw new FileNotFoundException($"Archive not found: {archivePath}");

        _zip = ZipFile.OpenRead(archivePath);
        _entryMap = new Dictionary<string, ZipArchiveEntry>(StringComparer.OrdinalIgnoreCase);
        foreach (var entry in _zip.Entries)
        {
            if (!string.IsNullOrEmpty(entry.Name)) // Skip directory entries
                _entryMap[entry.FullName.Replace('\\', '/')] = entry;
        }

        Log.Debug("Archive", $"Opened archive: {archivePath} ({EntryCount} entries)");
    }

    /// <summary>Check if the archive contains an entry.</summary>
    public bool Contains(string path) => _entryMap.ContainsKey(NormalizePath(path));

    /// <summary>Read an entry's full contents as a byte array.</summary>
    public byte[] ReadBytes(string path)
    {
        var entry = GetEntry(path);
        using var stream = entry.Open();
        using var ms = new MemoryStream();
        stream.CopyTo(ms);
        return ms.ToArray();
    }

    /// <summary>Read an entry as a string (UTF-8).</summary>
    public string ReadText(string path)
    {
        var entry = GetEntry(path);
        using var stream = entry.Open();
        using var reader = new StreamReader(stream);
        return reader.ReadToEnd();
    }

    /// <summary>Open a read stream for an entry.</summary>
    public Stream OpenRead(string path)
    {
        var entry = GetEntry(path);
        // Copy to MemoryStream so the caller can seek
        var stream = entry.Open();
        var ms = new MemoryStream();
        stream.CopyTo(ms);
        stream.Dispose();
        ms.Position = 0;
        return ms;
    }

    /// <summary>Get the compressed size of an entry.</summary>
    public long GetCompressedSize(string path)
    {
        var entry = GetEntry(path);
        return entry.CompressedLength;
    }

    /// <summary>Get the uncompressed size of an entry.</summary>
    public long GetSize(string path)
    {
        var entry = GetEntry(path);
        return entry.Length;
    }

    public void Dispose() => _zip.Dispose();

    private ZipArchiveEntry GetEntry(string path)
    {
        var normalized = NormalizePath(path);
        if (!_entryMap.TryGetValue(normalized, out var entry))
            throw new FileNotFoundException($"Entry not found in archive: {path}");
        return entry;
    }

    private static string NormalizePath(string path) => path.Replace('\\', '/');

    /// <summary>
    /// Pack a directory into a content archive (.pak).
    /// </summary>
    public static void Pack(string sourceDir, string outputPath, CompressionLevel level = CompressionLevel.Optimal)
    {
        if (!Directory.Exists(sourceDir))
            throw new DirectoryNotFoundException($"Source directory not found: {sourceDir}");

        var outputDir = Path.GetDirectoryName(outputPath);
        if (outputDir is not null)
            Directory.CreateDirectory(outputDir);

        if (File.Exists(outputPath))
            File.Delete(outputPath);

        ZipFile.CreateFromDirectory(sourceDir, outputPath, level, includeBaseDirectory: false);

        var info = new FileInfo(outputPath);
        Log.Info("Archive", $"Packed {sourceDir} → {outputPath} ({info.Length / 1024}KB)");
    }

    /// <summary>
    /// Unpack an archive to a directory.
    /// </summary>
    public static void Unpack(string archivePath, string outputDir)
    {
        if (!File.Exists(archivePath))
            throw new FileNotFoundException($"Archive not found: {archivePath}");

        Directory.CreateDirectory(outputDir);
        ZipFile.ExtractToDirectory(archivePath, outputDir, overwriteFiles: true);
        Log.Info("Archive", $"Unpacked {archivePath} → {outputDir}");
    }
}
