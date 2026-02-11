using Obsydian.Core.Logging;

namespace Obsydian.Content.Pipeline;

/// <summary>
/// CLI asset build tool that processes source assets into runtime formats.
/// Manages a pipeline of IAssetProcessors, tracks what needs rebuilding,
/// and generates a content manifest.
///
/// Usage:
///   var builder = new AssetBuildTool();
///   builder.RegisterProcessor(new CopyProcessor());
///   builder.RegisterProcessor(new TextureProcessor());
///   var report = builder.Build(new BuildConfig { SourceRoot = "content-src", OutputRoot = "content" });
/// </summary>
public sealed class AssetBuildTool
{
    private readonly List<IAssetProcessor> _processors = [];
    private readonly Dictionary<string, IAssetProcessor> _extensionMap = new(StringComparer.OrdinalIgnoreCase);

    /// <summary>Register a processor for its supported extensions.</summary>
    public void RegisterProcessor(IAssetProcessor processor)
    {
        _processors.Add(processor);
        foreach (var ext in processor.SupportedExtensions)
            _extensionMap[ext] = processor;
    }

    /// <summary>
    /// Build all assets from source to output.
    /// Returns a build report with statistics.
    /// </summary>
    public BuildReport Build(BuildConfig config)
    {
        var report = new BuildReport();
        var startTime = DateTime.UtcNow;

        if (!Directory.Exists(config.SourceRoot))
        {
            report.Errors.Add($"Source root not found: {config.SourceRoot}");
            return report;
        }

        Directory.CreateDirectory(config.OutputRoot);

        var manifest = new ContentManifest();
        var files = Directory.EnumerateFiles(config.SourceRoot, "*", SearchOption.AllDirectories).ToList();
        report.TotalFiles = files.Count;

        foreach (var sourceFile in files)
        {
            var ext = Path.GetExtension(sourceFile).ToLowerInvariant();
            var relativePath = Path.GetRelativePath(config.SourceRoot, sourceFile).Replace('\\', '/');
            var outputPath = Path.Combine(config.OutputRoot, relativePath);

            // Skip if output is newer than source (incremental build)
            if (!config.ForceRebuild && File.Exists(outputPath))
            {
                var sourceTime = File.GetLastWriteTimeUtc(sourceFile);
                var outputTime = File.GetLastWriteTimeUtc(outputPath);
                if (outputTime >= sourceTime)
                {
                    report.Skipped++;
                    // Still add to manifest
                    AddToManifest(manifest, relativePath, outputPath);
                    continue;
                }
            }

            // Ensure output directory exists
            var outputDir = Path.GetDirectoryName(outputPath);
            if (outputDir is not null)
                Directory.CreateDirectory(outputDir);

            if (_extensionMap.TryGetValue(ext, out var processor))
            {
                var context = new AssetProcessContext
                {
                    SourcePath = sourceFile,
                    OutputPath = outputPath,
                    RelativePath = relativePath,
                    Config = config,
                };

                try
                {
                    var result = processor.Process(context);
                    if (result.Success)
                    {
                        report.Processed++;
                        report.OutputBytes += result.OutputSize;

                        manifest.Add(new AssetEntry
                        {
                            Path = relativePath,
                            TypeTag = GetTypeTag(ext),
                            SizeBytes = result.OutputSize,
                            LastModified = DateTime.UtcNow,
                            Dependencies = result.Dependencies,
                        });
                    }
                    else
                    {
                        report.Failed++;
                        report.Errors.Add($"{relativePath}: {result.ErrorMessage}");
                    }
                }
                catch (Exception ex)
                {
                    report.Failed++;
                    report.Errors.Add($"{relativePath}: {ex.Message}");
                }
            }
            else
            {
                // No processor — copy as-is
                File.Copy(sourceFile, outputPath, overwrite: true);
                report.Processed++;
                var info = new FileInfo(outputPath);
                report.OutputBytes += info.Length;
                AddToManifest(manifest, relativePath, outputPath);
            }
        }

        // Generate manifest
        if (config.GenerateManifest)
        {
            var manifestPath = Path.Combine(config.OutputRoot, "content.manifest.json");
            manifest.SaveToFile(manifestPath);
        }

        report.Duration = DateTime.UtcNow - startTime;

        Log.Info("Build", $"Build complete: {report.Processed} processed, {report.Skipped} skipped, {report.Failed} failed ({report.Duration.TotalSeconds:F1}s)");
        return report;
    }

    /// <summary>
    /// Clean all built outputs.
    /// </summary>
    public void Clean(BuildConfig config)
    {
        if (Directory.Exists(config.OutputRoot))
        {
            Directory.Delete(config.OutputRoot, recursive: true);
            Log.Info("Build", $"Cleaned output directory: {config.OutputRoot}");
        }
    }

    private static void AddToManifest(ContentManifest manifest, string relativePath, string outputPath)
    {
        var ext = Path.GetExtension(relativePath).ToLowerInvariant();
        var info = new FileInfo(outputPath);
        manifest.Add(new AssetEntry
        {
            Path = relativePath,
            TypeTag = GetTypeTag(ext),
            SizeBytes = info.Length,
            LastModified = info.LastWriteTimeUtc,
        });
    }

    private static string GetTypeTag(string ext) => ext switch
    {
        ".png" or ".jpg" or ".jpeg" or ".bmp" => "Texture",
        ".wav" or ".ogg" or ".mp3" => "Audio",
        ".json" => "Data",
        ".tmj" => "TiledMap",
        ".tsj" or ".tsx" => "Tileset",
        ".fnt" or ".ttf" => "Font",
        _ => "Other",
    };
}

/// <summary>
/// Report generated after a content build.
/// </summary>
public sealed class BuildReport
{
    public int TotalFiles { get; set; }
    public int Processed { get; set; }
    public int Skipped { get; set; }
    public int Failed { get; set; }
    public long OutputBytes { get; set; }
    public TimeSpan Duration { get; set; }
    public List<string> Errors { get; } = [];

    public bool Success => Failed == 0 && Errors.Count == 0;
}
