using Obsydian.Content;

namespace Obsydian.Engine.Tests.Content;

public class ContentManifestTests
{
    [Fact]
    public void ScanDirectory_FindsAssets()
    {
        var tmpDir = Path.Combine(Path.GetTempPath(), "obsydian_manifest_test_" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(Path.Combine(tmpDir, "sprites"));
        Directory.CreateDirectory(Path.Combine(tmpDir, "audio"));

        File.WriteAllBytes(Path.Combine(tmpDir, "sprites", "player.png"), [0x89, 0x50]); // Fake PNG
        File.WriteAllBytes(Path.Combine(tmpDir, "audio", "click.wav"), [0x52, 0x49]); // Fake WAV
        File.WriteAllText(Path.Combine(tmpDir, "data.json"), "{}");

        try
        {
            var manifest = ContentManifest.ScanDirectory(tmpDir);

            Assert.Equal(3, manifest.Count);
            Assert.True(manifest.Contains("sprites/player.png"));
            Assert.True(manifest.Contains("audio/click.wav"));
            Assert.True(manifest.Contains("data.json"));

            var texture = manifest.Get("sprites/player.png");
            Assert.NotNull(texture);
            Assert.Equal("Texture", texture.TypeTag);

            var audio = manifest.Get("audio/click.wav");
            Assert.NotNull(audio);
            Assert.Equal("Audio", audio.TypeTag);
        }
        finally
        {
            Directory.Delete(tmpDir, true);
        }
    }

    [Fact]
    public void SaveAndLoad_RoundTrips()
    {
        var tmpDir = Path.Combine(Path.GetTempPath(), "obsydian_manifest_test_" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(tmpDir);

        try
        {
            var manifest = new ContentManifest();
            manifest.Add(new AssetEntry
            {
                Path = "sprites/hero.png",
                TypeTag = "Texture",
                SizeBytes = 1024,
                Dependencies = ["tilesets/main.png"]
            });
            manifest.Add(new AssetEntry
            {
                Path = "maps/level1.tmj",
                TypeTag = "TiledMap",
                SizeBytes = 4096,
            });

            var path = Path.Combine(tmpDir, "content.manifest.json");
            manifest.SaveToFile(path);

            var loaded = ContentManifest.LoadFromFile(path);
            Assert.Equal(2, loaded.Count);
            Assert.True(loaded.Contains("sprites/hero.png"));

            var hero = loaded.Get("sprites/hero.png");
            Assert.NotNull(hero);
            Assert.Equal("Texture", hero.TypeTag);
            Assert.Equal(1024, hero.SizeBytes);
            Assert.Single(hero.Dependencies);
        }
        finally
        {
            Directory.Delete(tmpDir, true);
        }
    }

    [Fact]
    public void GetByType_FiltersCorrectly()
    {
        var manifest = new ContentManifest();
        manifest.Add(new AssetEntry { Path = "a.png", TypeTag = "Texture" });
        manifest.Add(new AssetEntry { Path = "b.png", TypeTag = "Texture" });
        manifest.Add(new AssetEntry { Path = "c.wav", TypeTag = "Audio" });

        var textures = manifest.GetByType("Texture").ToList();
        Assert.Equal(2, textures.Count);

        var audio = manifest.GetByType("Audio").ToList();
        Assert.Single(audio);
    }

    [Fact]
    public void Search_MatchesWildcard()
    {
        var manifest = new ContentManifest();
        manifest.Add(new AssetEntry { Path = "sprites/player.png", TypeTag = "Texture" });
        manifest.Add(new AssetEntry { Path = "sprites/enemy.png", TypeTag = "Texture" });
        manifest.Add(new AssetEntry { Path = "audio/music.ogg", TypeTag = "Audio" });

        var sprites = manifest.Search("sprites/*.png").ToList();
        Assert.Equal(2, sprites.Count);

        var all = manifest.Search("*.ogg").ToList();
        Assert.Single(all);
    }

    [Fact]
    public void GetReverseDependencies_FindsDependents()
    {
        var manifest = new ContentManifest();
        manifest.Add(new AssetEntry
        {
            Path = "tilesets/main.png",
            TypeTag = "Texture",
        });
        manifest.Add(new AssetEntry
        {
            Path = "maps/level1.tmj",
            TypeTag = "TiledMap",
            Dependencies = ["tilesets/main.png"],
        });
        manifest.Add(new AssetEntry
        {
            Path = "maps/level2.tmj",
            TypeTag = "TiledMap",
            Dependencies = ["tilesets/main.png"],
        });

        var dependents = manifest.GetReverseDependencies("tilesets/main.png");
        Assert.Equal(2, dependents.Count);
    }

    [Fact]
    public void Validate_DetectsMissingFiles()
    {
        var tmpDir = Path.Combine(Path.GetTempPath(), "obsydian_manifest_test_" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(tmpDir);
        File.WriteAllText(Path.Combine(tmpDir, "exists.json"), "{}");

        try
        {
            var manifest = new ContentManifest();
            manifest.Add(new AssetEntry { Path = "exists.json", TypeTag = "Data" });
            manifest.Add(new AssetEntry { Path = "missing.png", TypeTag = "Texture" });

            var missing = manifest.Validate(tmpDir);
            Assert.Single(missing);
            Assert.Equal("missing.png", missing[0]);
        }
        finally
        {
            Directory.Delete(tmpDir, true);
        }
    }
}
