namespace Obsydian.Content.Pipeline;

/// <summary>
/// Processes a source asset into a runtime-ready format.
/// Inspired by CryEngine's IConverter/ICompiler two-level design.
///
/// Examples:
///   - TextureProcessor: resize, generate mipmaps, compress to atlas
///   - AudioProcessor: convert MP3 → OGG, normalize volume
///   - DataProcessor: validate JSON schema, strip comments
///   - CopyProcessor: pass-through for already-ready assets
/// </summary>
public interface IAssetProcessor
{
    /// <summary>File extensions this processor handles (e.g., ".png", ".psd").</summary>
    IReadOnlyList<string> SupportedExtensions { get; }

    /// <summary>
    /// Process a source file into a runtime file.
    /// Returns the output file path (may differ from input if format changed).
    /// </summary>
    AssetProcessResult Process(AssetProcessContext context);
}

/// <summary>
/// Context passed to asset processors during a build.
/// </summary>
public sealed class AssetProcessContext
{
    /// <summary>Full path to the source file.</summary>
    public required string SourcePath { get; init; }

    /// <summary>Full path to write the output file.</summary>
    public required string OutputPath { get; init; }

    /// <summary>Relative path from content root (forward slashes).</summary>
    public required string RelativePath { get; init; }

    /// <summary>Build configuration options.</summary>
    public required BuildConfig Config { get; init; }
}

/// <summary>
/// Result of processing a single asset.
/// </summary>
public sealed class AssetProcessResult
{
    public bool Success { get; init; }
    public string OutputPath { get; init; } = "";
    public long OutputSize { get; init; }
    public string? ErrorMessage { get; init; }
    public List<string> Dependencies { get; init; } = [];

    public static AssetProcessResult Ok(string outputPath, long outputSize, List<string>? deps = null)
        => new() { Success = true, OutputPath = outputPath, OutputSize = outputSize, Dependencies = deps ?? [] };

    public static AssetProcessResult Fail(string error)
        => new() { Success = false, ErrorMessage = error };
}

/// <summary>
/// Build configuration for the asset pipeline.
/// </summary>
public sealed class BuildConfig
{
    /// <summary>Source content directory.</summary>
    public required string SourceRoot { get; init; }

    /// <summary>Output directory for processed assets.</summary>
    public required string OutputRoot { get; init; }

    /// <summary>Target platform name (e.g., "desktop", "mobile").</summary>
    public string Platform { get; init; } = "desktop";

    /// <summary>Whether to overwrite existing outputs.</summary>
    public bool ForceRebuild { get; init; }

    /// <summary>Whether to generate a manifest file.</summary>
    public bool GenerateManifest { get; init; } = true;
}
