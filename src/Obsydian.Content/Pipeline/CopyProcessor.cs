namespace Obsydian.Content.Pipeline;

/// <summary>
/// Pass-through processor that copies files as-is.
/// Use for assets that are already in runtime format (PNG, WAV, JSON, etc.).
/// </summary>
public sealed class CopyProcessor : IAssetProcessor
{
    private readonly List<string> _extensions;

    public IReadOnlyList<string> SupportedExtensions => _extensions;

    public CopyProcessor(params string[] extensions)
    {
        _extensions = extensions.Length > 0
            ? extensions.ToList()
            : [".png", ".jpg", ".jpeg", ".bmp", ".wav", ".ogg", ".json", ".tmj", ".tsj", ".fnt", ".ttf", ".txt", ".csv", ".xml"];
    }

    public AssetProcessResult Process(AssetProcessContext context)
    {
        File.Copy(context.SourcePath, context.OutputPath, overwrite: true);
        var info = new FileInfo(context.OutputPath);
        return AssetProcessResult.Ok(context.OutputPath, info.Length);
    }
}
