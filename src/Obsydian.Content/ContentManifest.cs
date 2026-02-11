using System.Text.Json;
using System.Text.Json.Serialization;
using Obsydian.Core.Logging;

namespace Obsydian.Content;

/// <summary>
/// Asset catalog that knows every asset in the project — its type, path,
/// size, and dependencies. Can be built by scanning a directory or loaded
/// from a manifest JSON file.
/// </summary>
public sealed class ContentManifest
{
    private readonly Dictionary<string, AssetEntry> _entries = new(StringComparer.OrdinalIgnoreCase);

    /// <summary>All registered asset entries.</summary>
    public IReadOnlyDictionary<string, AssetEntry> Entries => _entries;

    /// <summary>Total number of assets in the manifest.</summary>
    public int Count => _entries.Count;

    /// <summary>Register an asset entry manually.</summary>
    public void Add(AssetEntry entry)
    {
        _entries[entry.Path] = entry;
    }

    /// <summary>Check if an asset exists in the manifest.</summary>
    public bool Contains(string assetPath) => _entries.ContainsKey(assetPath);

    /// <summary>Get an asset entry by path, or null if not found.</summary>
    public AssetEntry? Get(string assetPath)
        => _entries.TryGetValue(assetPath, out var entry) ? entry : null;

    /// <summary>Get all assets of a given type tag.</summary>
    public IEnumerable<AssetEntry> GetByType(string typeTag)
        => _entries.Values.Where(e => string.Equals(e.TypeTag, typeTag, StringComparison.OrdinalIgnoreCase));

    /// <summary>Get all assets matching a glob-like pattern (supports * wildcard).</summary>
    public IEnumerable<AssetEntry> Search(string pattern)
    {
        if (!pattern.Contains('*'))
            return _entries.TryGetValue(pattern, out var exact) ? [exact] : [];

        var parts = pattern.Split('*', 2);
        return _entries.Values.Where(e =>
            e.Path.StartsWith(parts[0], StringComparison.OrdinalIgnoreCase) &&
            e.Path.EndsWith(parts[1], StringComparison.OrdinalIgnoreCase));
    }

    /// <summary>
    /// Scan a content directory and build a manifest from file extensions.
    /// Maps file extensions to type tags automatically.
    /// </summary>
    public static ContentManifest ScanDirectory(string rootPath, Dictionary<string, string>? extensionToType = null)
    {
        var manifest = new ContentManifest();
        var typeMap = extensionToType ?? DefaultExtensionMap;

        if (!Directory.Exists(rootPath))
        {
            Log.Warn("Manifest", $"Content root not found: {rootPath}");
            return manifest;
        }

        foreach (var file in Directory.EnumerateFiles(rootPath, "*", SearchOption.AllDirectories))
        {
            var ext = Path.GetExtension(file).ToLowerInvariant();
            if (!typeMap.TryGetValue(ext, out var typeTag))
                continue;

            var relativePath = Path.GetRelativePath(rootPath, file).Replace('\\', '/');
            var info = new FileInfo(file);

            manifest.Add(new AssetEntry
            {
                Path = relativePath,
                TypeTag = typeTag,
                SizeBytes = info.Length,
                LastModified = info.LastWriteTimeUtc,
            });
        }

        Log.Info("Manifest", $"Scanned {manifest.Count} assets from {rootPath}");
        return manifest;
    }

    /// <summary>Load a manifest from a JSON file.</summary>
    public static ContentManifest LoadFromFile(string path)
    {
        var json = File.ReadAllText(path);
        var entries = JsonSerializer.Deserialize<List<AssetEntry>>(json, JsonOpts)
            ?? throw new InvalidDataException($"Failed to parse manifest: {path}");

        var manifest = new ContentManifest();
        foreach (var entry in entries)
            manifest.Add(entry);

        Log.Info("Manifest", $"Loaded manifest with {manifest.Count} entries from {path}");
        return manifest;
    }

    /// <summary>Save this manifest to a JSON file.</summary>
    public void SaveToFile(string path)
    {
        var entries = _entries.Values.OrderBy(e => e.Path).ToList();
        var json = JsonSerializer.Serialize(entries, JsonOpts);
        var dir = Path.GetDirectoryName(path);
        if (dir is not null)
            Directory.CreateDirectory(dir);
        File.WriteAllText(path, json);
        Log.Info("Manifest", $"Saved manifest with {Count} entries to {path}");
    }

    /// <summary>
    /// Validate all assets in the manifest exist on disk. Returns missing paths.
    /// </summary>
    public List<string> Validate(string rootPath)
    {
        var missing = new List<string>();
        foreach (var entry in _entries.Values)
        {
            var fullPath = Path.Combine(rootPath, entry.Path);
            if (!File.Exists(fullPath))
                missing.Add(entry.Path);
        }
        return missing;
    }

    /// <summary>
    /// Get all assets that this asset depends on (forward dependencies).
    /// </summary>
    public IReadOnlyList<string> GetDependencies(string assetPath)
    {
        if (_entries.TryGetValue(assetPath, out var entry))
            return entry.Dependencies;
        return [];
    }

    /// <summary>
    /// Get all assets that depend on this asset (reverse dependencies).
    /// </summary>
    public List<string> GetReverseDependencies(string assetPath)
    {
        return _entries.Values
            .Where(e => e.Dependencies.Contains(assetPath, StringComparer.OrdinalIgnoreCase))
            .Select(e => e.Path)
            .ToList();
    }

    private static readonly JsonSerializerOptions JsonOpts = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingDefault,
    };

    private static readonly Dictionary<string, string> DefaultExtensionMap = new(StringComparer.OrdinalIgnoreCase)
    {
        [".png"] = "Texture",
        [".jpg"] = "Texture",
        [".jpeg"] = "Texture",
        [".bmp"] = "Texture",
        [".wav"] = "Audio",
        [".ogg"] = "Audio",
        [".mp3"] = "Audio",
        [".json"] = "Data",
        [".tmj"] = "TiledMap",
        [".tsj"] = "Tileset",
        [".tsx"] = "Tileset",
        [".fnt"] = "Font",
        [".ttf"] = "Font",
        [".csv"] = "Data",
        [".xml"] = "Data",
        [".txt"] = "Text",
    };
}

/// <summary>
/// Describes a single asset in the content manifest.
/// </summary>
public sealed class AssetEntry
{
    /// <summary>Relative path from content root (forward slashes).</summary>
    public required string Path { get; init; }

    /// <summary>Type tag: "Texture", "Audio", "TiledMap", "Data", etc.</summary>
    public required string TypeTag { get; init; }

    /// <summary>File size in bytes.</summary>
    public long SizeBytes { get; init; }

    /// <summary>Last modified timestamp (UTC).</summary>
    public DateTime LastModified { get; init; }

    /// <summary>Asset paths this asset depends on (e.g., tilemap → tileset texture).</summary>
    public List<string> Dependencies { get; init; } = [];

    /// <summary>Arbitrary metadata (e.g., texture dimensions, audio duration).</summary>
    public Dictionary<string, string> Metadata { get; init; } = [];
}
