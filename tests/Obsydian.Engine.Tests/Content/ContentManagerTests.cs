using Obsydian.Content;

namespace Obsydian.Engine.Tests.Content;

public class ContentManagerTests
{
    [Fact]
    public void RootPath_IsExposed()
    {
        var mgr = new ContentManager("/some/path");
        Assert.Equal("/some/path", mgr.RootPath);
    }

    [Fact]
    public void RegisterLoader_And_Load_Works()
    {
        var tmpDir = Path.Combine(Path.GetTempPath(), "obsydian_content_test_" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(tmpDir);
        File.WriteAllText(Path.Combine(tmpDir, "test.txt"), "hello");

        try
        {
            var mgr = new ContentManager(tmpDir);
            mgr.RegisterLoader(new TextLoader());

            var text = mgr.Load<TextAsset>("test.txt");
            Assert.Equal("hello", text.Content);
        }
        finally
        {
            Directory.Delete(tmpDir, true);
        }
    }

    [Fact]
    public void Load_CachesAsset()
    {
        var tmpDir = Path.Combine(Path.GetTempPath(), "obsydian_content_test_" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(tmpDir);
        File.WriteAllText(Path.Combine(tmpDir, "test.txt"), "hello");

        try
        {
            var loader = new TextLoader();
            var mgr = new ContentManager(tmpDir);
            mgr.RegisterLoader(loader);

            var a = mgr.Load<TextAsset>("test.txt");
            var b = mgr.Load<TextAsset>("test.txt");
            Assert.Same(a, b); // cached
            Assert.Equal(1, loader.LoadCount);
        }
        finally
        {
            Directory.Delete(tmpDir, true);
        }
    }

    [Fact]
    public void Load_ThrowsForUnregisteredType()
    {
        var mgr = new ContentManager("/tmp");
        Assert.Throws<InvalidOperationException>(() => mgr.Load<TextAsset>("test.txt"));
    }

    public sealed class TextAsset
    {
        public string Content { get; set; } = "";
    }

    public sealed class TextLoader : IAssetLoader<TextAsset>
    {
        public int LoadCount { get; private set; }

        public TextAsset Load(string fullPath)
        {
            LoadCount++;
            return new TextAsset { Content = File.ReadAllText(fullPath) };
        }
    }
}
