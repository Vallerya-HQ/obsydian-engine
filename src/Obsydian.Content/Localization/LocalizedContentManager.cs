using Obsydian.Core.Logging;

namespace Obsydian.Content.Localization;

/// <summary>
/// Content manager wrapper that supports localized asset loading.
/// Follows Stardew Valley's fallback chain:
///   1. Try: asset.{languageTag}  (e.g., "strings/ui.ja")
///   2. Try: asset               (base/English fallback)
///
/// For string tables, supports key-based lookup:
///   LoadString("Strings/UI:greeting") → loads Strings/UI.json, returns strings["greeting"]
/// </summary>
public sealed class LocalizedContentManager : IDisposable
{
    private readonly ContentManager _content;
    private string _customLanguageTag = "";

    /// <summary>Current language.</summary>
    public LanguageCode CurrentLanguage { get; private set; } = LanguageCode.Default;

    /// <summary>Current language tag string (e.g., "en", "ja").</summary>
    public string CurrentLanguageTag => CurrentLanguage == LanguageCode.Custom
        ? _customLanguageTag
        : CurrentLanguage.ToTag();

    /// <summary>Fired when the language changes.</summary>
    public event Action<LanguageCode>? OnLanguageChanged;

    public LocalizedContentManager(ContentManager content)
    {
        _content = content;
    }

    /// <summary>Set the active language.</summary>
    public void SetLanguage(LanguageCode language, string? customTag = null)
    {
        if (language == LanguageCode.Custom && string.IsNullOrEmpty(customTag))
            throw new ArgumentException("Custom language requires a tag", nameof(customTag));

        CurrentLanguage = language;
        _customLanguageTag = customTag ?? "";

        Log.Info("Localization", $"Language set to: {language} ({CurrentLanguageTag})");
        OnLanguageChanged?.Invoke(language);
    }

    /// <summary>
    /// Load a localized asset. Tries language-specific version first,
    /// falls back to base asset.
    /// </summary>
    public T Load<T>(string assetPath) where T : class
    {
        if (CurrentLanguage != LanguageCode.Default)
        {
            var localizedPath = GetLocalizedPath(assetPath);
            var fullLocalized = Path.Combine(_content.RootPath, localizedPath);
            if (File.Exists(fullLocalized))
                return _content.Load<T>(localizedPath);
        }

        return _content.Load<T>(assetPath);
    }

    /// <summary>
    /// Load a string from a string table using "AssetPath:key" format.
    /// The string table is a JSON file containing a Dictionary&lt;string, string&gt;.
    /// </summary>
    public string LoadString(string reference)
    {
        var colonIndex = reference.IndexOf(':');
        if (colonIndex < 0)
            throw new ArgumentException($"String reference must be in 'AssetPath:key' format, got: {reference}");

        var assetPath = reference[..colonIndex];
        var key = reference[(colonIndex + 1)..];

        var table = Load<StringTable>(assetPath);
        if (table.Strings.TryGetValue(key, out var value))
            return value;

        Log.Warn("Localization", $"Missing string key '{key}' in {assetPath}");
        return $"[{key}]"; // Return bracketed key as placeholder
    }

    /// <summary>
    /// Load a string with format arguments.
    /// </summary>
    public string LoadString(string reference, params object[] args)
    {
        var template = LoadString(reference);
        return string.Format(template, args);
    }

    public void Dispose() { } // ContentManager owns the actual cleanup

    private string GetLocalizedPath(string assetPath)
    {
        var ext = Path.GetExtension(assetPath);
        var basePath = assetPath[..^ext.Length];
        return $"{basePath}.{CurrentLanguageTag}{ext}";
    }
}

/// <summary>
/// A string table loaded from a JSON dictionary file.
/// Used for UI text, dialogue, item descriptions, etc.
/// </summary>
public sealed class StringTable
{
    public IReadOnlyDictionary<string, string> Strings { get; }

    public StringTable(Dictionary<string, string> strings)
    {
        Strings = strings;
    }
}
