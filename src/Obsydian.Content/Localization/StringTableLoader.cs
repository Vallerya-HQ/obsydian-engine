using System.Text.Json;

namespace Obsydian.Content.Localization;

/// <summary>
/// Loads string table JSON files (Dictionary&lt;string, string&gt;) for localization.
/// Expected format:
/// {
///   "greeting": "Hello!",
///   "farewell": "Goodbye!",
///   "item_sword": "Iron Sword"
/// }
/// </summary>
public sealed class StringTableLoader : IAssetLoader<StringTable>
{
    private static readonly JsonSerializerOptions Options = new()
    {
        PropertyNameCaseInsensitive = true,
        ReadCommentHandling = JsonCommentHandling.Skip,
        AllowTrailingCommas = true,
    };

    public StringTable Load(string fullPath)
    {
        if (!File.Exists(fullPath))
            throw new FileNotFoundException($"String table not found: {fullPath}");

        var json = File.ReadAllText(fullPath);
        var dict = JsonSerializer.Deserialize<Dictionary<string, string>>(json, Options)
            ?? throw new InvalidDataException($"Failed to parse string table: {fullPath}");

        return new StringTable(dict);
    }
}
