namespace Obsydian.Content.Localization;

/// <summary>
/// Supported language codes. Mirrors Stardew Valley's approach —
/// the engine ships with a base set, and games can extend via Custom.
/// </summary>
public enum LanguageCode
{
    Default,  // No localization — use base asset
    En,       // English
    Fr,       // French
    De,       // German
    Es,       // Spanish
    Pt,       // Portuguese
    It,       // Italian
    Ja,       // Japanese
    Ko,       // Korean
    Zh,       // Chinese (Simplified)
    Ru,       // Russian
    Tr,       // Turkish
    Ar,       // Arabic
    Th,       // Thai
    Pl,       // Polish
    Nl,       // Dutch
    Custom,   // Game-defined language
}

public static class LanguageCodeExtensions
{
    /// <summary>Get the standard BCP-47 language tag (e.g., "en", "pt-BR").</summary>
    public static string ToTag(this LanguageCode code) => code switch
    {
        LanguageCode.En => "en",
        LanguageCode.Fr => "fr",
        LanguageCode.De => "de",
        LanguageCode.Es => "es",
        LanguageCode.Pt => "pt",
        LanguageCode.It => "it",
        LanguageCode.Ja => "ja",
        LanguageCode.Ko => "ko",
        LanguageCode.Zh => "zh",
        LanguageCode.Ru => "ru",
        LanguageCode.Tr => "tr",
        LanguageCode.Ar => "ar",
        LanguageCode.Th => "th",
        LanguageCode.Pl => "pl",
        LanguageCode.Nl => "nl",
        _ => "",
    };
}
