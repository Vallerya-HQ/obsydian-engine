using Obsydian.Content.Pipeline;
using Obsydian.Content.Validation;
using Obsydian.Content;

namespace Obsydian.Engine.Tests.Content;

public class ContentPipelineTests
{
    [Fact]
    public void AssetBuildTool_CopiesFiles_AndGeneratesManifest()
    {
        var srcDir = Path.Combine(Path.GetTempPath(), "obsydian_build_src_" + Guid.NewGuid().ToString("N"));
        var outDir = Path.Combine(Path.GetTempPath(), "obsydian_build_out_" + Guid.NewGuid().ToString("N"));

        Directory.CreateDirectory(Path.Combine(srcDir, "sprites"));
        File.WriteAllBytes(Path.Combine(srcDir, "sprites", "hero.png"), [1, 2, 3, 4]);
        File.WriteAllText(Path.Combine(srcDir, "data.json"), "{}");

        try
        {
            var builder = new AssetBuildTool();
            builder.RegisterProcessor(new CopyProcessor());

            var report = builder.Build(new BuildConfig
            {
                SourceRoot = srcDir,
                OutputRoot = outDir,
                GenerateManifest = true,
            });

            Assert.True(report.Success);
            Assert.Equal(2, report.Processed);
            Assert.Equal(0, report.Failed);
            Assert.True(File.Exists(Path.Combine(outDir, "sprites", "hero.png")));
            Assert.True(File.Exists(Path.Combine(outDir, "data.json")));
            Assert.True(File.Exists(Path.Combine(outDir, "content.manifest.json")));
        }
        finally
        {
            if (Directory.Exists(srcDir)) Directory.Delete(srcDir, true);
            if (Directory.Exists(outDir)) Directory.Delete(outDir, true);
        }
    }

    [Fact]
    public void IncrementalBuild_SkipsUpToDateFiles()
    {
        var srcDir = Path.Combine(Path.GetTempPath(), "obsydian_build_src_" + Guid.NewGuid().ToString("N"));
        var outDir = Path.Combine(Path.GetTempPath(), "obsydian_build_out_" + Guid.NewGuid().ToString("N"));

        Directory.CreateDirectory(srcDir);
        File.WriteAllText(Path.Combine(srcDir, "data.json"), "{}");

        try
        {
            var builder = new AssetBuildTool();
            builder.RegisterProcessor(new CopyProcessor());
            var config = new BuildConfig { SourceRoot = srcDir, OutputRoot = outDir };

            // First build
            var r1 = builder.Build(config);
            Assert.Equal(1, r1.Processed);
            Assert.Equal(0, r1.Skipped);

            // Second build — should skip
            var r2 = builder.Build(config);
            Assert.Equal(0, r2.Processed);
            Assert.Equal(1, r2.Skipped);
        }
        finally
        {
            if (Directory.Exists(srcDir)) Directory.Delete(srcDir, true);
            if (Directory.Exists(outDir)) Directory.Delete(outDir, true);
        }
    }

    [Fact]
    public void ContentArchive_PackAndRead()
    {
        var srcDir = Path.Combine(Path.GetTempPath(), "obsydian_pak_src_" + Guid.NewGuid().ToString("N"));
        var pakPath = Path.Combine(Path.GetTempPath(), $"obsydian_test_{Guid.NewGuid():N}.pak");

        Directory.CreateDirectory(Path.Combine(srcDir, "sub"));
        File.WriteAllText(Path.Combine(srcDir, "hello.txt"), "world");
        File.WriteAllBytes(Path.Combine(srcDir, "sub", "data.bin"), [0xDE, 0xAD]);

        try
        {
            ContentArchive.Pack(srcDir, pakPath);

            using var archive = new ContentArchive(pakPath);
            Assert.Equal(2, archive.EntryCount);
            Assert.True(archive.Contains("hello.txt"));
            Assert.True(archive.Contains("sub/data.bin"));

            Assert.Equal("world", archive.ReadText("hello.txt"));
            Assert.Equal([0xDE, 0xAD], archive.ReadBytes("sub/data.bin"));
        }
        finally
        {
            if (Directory.Exists(srcDir)) Directory.Delete(srcDir, true);
            if (File.Exists(pakPath)) File.Delete(pakPath);
        }
    }

    [Fact]
    public void ContentValidator_DetectsMissingDependencies()
    {
        var manifest = new ContentManifest();
        manifest.Add(new AssetEntry
        {
            Path = "maps/level.tmj",
            TypeTag = "TiledMap",
            Dependencies = ["tilesets/missing.png"],
        });

        var validator = new ContentValidator();
        var tmpDir = Path.Combine(Path.GetTempPath(), "obsydian_validate_" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(Path.Combine(tmpDir, "maps"));
        File.WriteAllText(Path.Combine(tmpDir, "maps", "level.tmj"), "{}");

        try
        {
            validator.ValidateManifest(manifest, tmpDir);
            Assert.True(validator.HasErrors);
            Assert.Contains(validator.Issues, i => i.Message.Contains("Missing dependency"));
        }
        finally
        {
            Directory.Delete(tmpDir, true);
        }
    }

    [Fact]
    public void AudioClip_CalculatesDuration()
    {
        // 1 second of 16-bit mono audio at 44100Hz
        int sampleRate = 44100;
        int channels = 1;
        int bitsPerSample = 16;
        int bytesPerSample = bitsPerSample / 8;
        var pcm = new byte[sampleRate * channels * bytesPerSample]; // 1 second

        var clip = new Obsydian.Audio.AudioClip(pcm, channels, sampleRate, bitsPerSample, "test");
        Assert.Equal(1f, clip.Duration, 0.01f);
    }
}
