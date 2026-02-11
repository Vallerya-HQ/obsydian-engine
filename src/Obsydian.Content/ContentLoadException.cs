namespace Obsydian.Content;

/// <summary>
/// Thrown when an asset fails to load. Wraps the inner exception with
/// asset path, type, and loader context for actionable error messages.
/// </summary>
public sealed class ContentLoadException : Exception
{
    public string AssetPath { get; }
    public Type AssetType { get; }

    public ContentLoadException(string assetPath, Type assetType, Exception inner)
        : base($"Failed to load {assetType.Name} from '{assetPath}': {inner.Message}", inner)
    {
        AssetPath = assetPath;
        AssetType = assetType;
    }

    public ContentLoadException(string assetPath, Type assetType, string message)
        : base($"Failed to load {assetType.Name} from '{assetPath}': {message}")
    {
        AssetPath = assetPath;
        AssetType = assetType;
    }
}
