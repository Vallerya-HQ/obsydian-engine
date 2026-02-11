using Obsydian.Content;
using Obsydian.Content.Localization;

namespace Obsydian.Engine.Tests.Content;

public class LocalizationTests
{
    [Fact]
    public void StringTableLoader_LoadsJsonDictionary()
    {
        var tmpDir = Path.Combine(Path.GetTempPath(), "obsydian_l10n_test_" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(tmpDir);
        File.WriteAllText(Path.Combine(tmpDir, "strings.json"),
            """{"greeting": "Hello!", "farewell": "Goodbye!"}""");

        try
        {
            using var content = new ContentManager(tmpDir);
            content.RegisterLoader(new StringTableLoader());

            var table = content.Load<StringTable>("strings.json");
            Assert.Equal("Hello!", table.Strings["greeting"]);
            Assert.Equal("Goodbye!", table.Strings["farewell"]);
        }
        finally
        {
            Directory.Delete(tmpDir, true);
        }
    }

    [Fact]
    public void LocalizedContentManager_FallsBackToBase()
    {
        var tmpDir = Path.Combine(Path.GetTempPath(), "obsydian_l10n_test_" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(tmpDir);
        File.WriteAllText(Path.Combine(tmpDir, "strings.json"),
            """{"greeting": "Hello!"}""");

        try
        {
            using var content = new ContentManager(tmpDir);
            content.RegisterLoader(new StringTableLoader());

            var localized = new LocalizedContentManager(content);
            localized.SetLanguage(LanguageCode.Ja); // No .ja file exists

            // Should fall back to base
            var table = localized.Load<StringTable>("strings.json");
            Assert.Equal("Hello!", table.Strings["greeting"]);
        }
        finally
        {
            Directory.Delete(tmpDir, true);
        }
    }

    [Fact]
    public void LocalizedContentManager_LoadsLocalizedVersion()
    {
        var tmpDir = Path.Combine(Path.GetTempPath(), "obsydian_l10n_test_" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(tmpDir);
        File.WriteAllText(Path.Combine(tmpDir, "strings.json"),
            """{"greeting": "Hello!"}""");
        File.WriteAllText(Path.Combine(tmpDir, "strings.ja.json"),
            """{"greeting": "こんにちは！"}""");

        try
        {
            using var content = new ContentManager(tmpDir);
            content.RegisterLoader(new StringTableLoader());

            var localized = new LocalizedContentManager(content);
            localized.SetLanguage(LanguageCode.Ja);

            var table = localized.Load<StringTable>("strings.json");
            Assert.Equal("こんにちは！", table.Strings["greeting"]);
        }
        finally
        {
            Directory.Delete(tmpDir, true);
        }
    }

    [Fact]
    public void LoadString_ParsesAssetPathAndKey()
    {
        var tmpDir = Path.Combine(Path.GetTempPath(), "obsydian_l10n_test_" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(tmpDir);
        File.WriteAllText(Path.Combine(tmpDir, "ui.json"),
            """{"title": "My Game", "start": "Start Game"}""");

        try
        {
            using var content = new ContentManager(tmpDir);
            content.RegisterLoader(new StringTableLoader());

            var localized = new LocalizedContentManager(content);
            Assert.Equal("My Game", localized.LoadString("ui.json:title"));
            Assert.Equal("Start Game", localized.LoadString("ui.json:start"));
        }
        finally
        {
            Directory.Delete(tmpDir, true);
        }
    }

    [Fact]
    public void LanguageCode_ToTag_ReturnsCorrectValues()
    {
        Assert.Equal("en", LanguageCode.En.ToTag());
        Assert.Equal("ja", LanguageCode.Ja.ToTag());
        Assert.Equal("", LanguageCode.Default.ToTag());
    }

    [Fact]
    public void OnLanguageChanged_Fires()
    {
        using var content = new ContentManager("/tmp");
        var localized = new LocalizedContentManager(content);

        LanguageCode? changed = null;
        localized.OnLanguageChanged += lang => changed = lang;

        localized.SetLanguage(LanguageCode.Fr);
        Assert.Equal(LanguageCode.Fr, changed);
    }
}
