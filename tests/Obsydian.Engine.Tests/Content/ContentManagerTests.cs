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

    [Fact]
    public void Load_WrapsExceptionInContentLoadException()
    {
        var tmpDir = Path.Combine(Path.GetTempPath(), "obsydian_content_test_" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(tmpDir);

        try
        {
            var mgr = new ContentManager(tmpDir);
            mgr.RegisterLoader(new TextLoader());

            var ex = Assert.Throws<ContentLoadException>(() => mgr.Load<TextAsset>("nonexistent.txt"));
            Assert.Equal("nonexistent.txt", ex.AssetPath);
            Assert.Equal(typeof(TextAsset), ex.AssetType);
            Assert.NotNull(ex.InnerException);
        }
        finally
        {
            Directory.Delete(tmpDir, true);
        }
    }

    [Fact]
    public void HasLoader_ReturnsCorrectly()
    {
        var mgr = new ContentManager("/tmp");
        Assert.False(mgr.HasLoader<TextAsset>());
        mgr.RegisterLoader(new TextLoader());
        Assert.True(mgr.HasLoader<TextAsset>());
    }

    [Fact]
    public void IsCached_ReturnsCorrectly()
    {
        var tmpDir = Path.Combine(Path.GetTempPath(), "obsydian_content_test_" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(tmpDir);
        File.WriteAllText(Path.Combine(tmpDir, "test.txt"), "hello");

        try
        {
            var mgr = new ContentManager(tmpDir);
            mgr.RegisterLoader(new TextLoader());

            Assert.False(mgr.IsCached<TextAsset>("test.txt"));
            mgr.Load<TextAsset>("test.txt");
            Assert.True(mgr.IsCached<TextAsset>("test.txt"));
        }
        finally
        {
            Directory.Delete(tmpDir, true);
        }
    }

    [Fact]
    public void Load_DoesNotIncrementRefCount()
    {
        var tmpDir = Path.Combine(Path.GetTempPath(), "obsydian_content_test_" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(tmpDir);
        File.WriteAllText(Path.Combine(tmpDir, "test.txt"), "hello");

        try
        {
            var mgr = new ContentManager(tmpDir);
            mgr.RegisterLoader(new TextLoader());

            mgr.Load<TextAsset>("test.txt");
            Assert.Equal(0, mgr.GetRefCount<TextAsset>("test.txt")); // Load doesn't track refs

            mgr.Load<TextAsset>("test.txt"); // Cached hit — still no ref increment
            Assert.Equal(0, mgr.GetRefCount<TextAsset>("test.txt"));
        }
        finally
        {
            Directory.Delete(tmpDir, true);
        }
    }

    [Fact]
    public void Acquire_IncrementsRefCount_Release_Decrements()
    {
        var tmpDir = Path.Combine(Path.GetTempPath(), "obsydian_content_test_" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(tmpDir);
        File.WriteAllText(Path.Combine(tmpDir, "test.txt"), "hello");

        try
        {
            var mgr = new ContentManager(tmpDir);
            mgr.RegisterLoader(new TextLoader());

            mgr.Acquire<TextAsset>("test.txt");
            Assert.Equal(1, mgr.GetRefCount<TextAsset>("test.txt"));

            mgr.Acquire<TextAsset>("test.txt");
            Assert.Equal(2, mgr.GetRefCount<TextAsset>("test.txt"));

            mgr.Release<TextAsset>("test.txt");
            Assert.Equal(1, mgr.GetRefCount<TextAsset>("test.txt"));
            Assert.True(mgr.IsCached<TextAsset>("test.txt"));

            mgr.Release<TextAsset>("test.txt");
            Assert.False(mgr.IsCached<TextAsset>("test.txt")); // Evicted at 0
        }
        finally
        {
            Directory.Delete(tmpDir, true);
        }
    }

    [Fact]
    public async Task ConcurrentLoads_AreThreadSafe()
    {
        var tmpDir = Path.Combine(Path.GetTempPath(), "obsydian_content_test_" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(tmpDir);
        for (int i = 0; i < 20; i++)
            File.WriteAllText(Path.Combine(tmpDir, $"file{i}.txt"), $"content{i}");

        try
        {
            var mgr = new ContentManager(tmpDir);
            mgr.RegisterLoader(new TextLoader());

            // Load 20 files across threads simultaneously
            var tasks = Enumerable.Range(0, 20)
                .Select(i => Task.Run(() => mgr.Load<TextAsset>($"file{i}.txt")))
                .ToArray();

            await Task.WhenAll(tasks);

            Assert.Equal(20, mgr.CachedCount);
            foreach (var t in tasks)
                Assert.NotNull(t.Result);
        }
        finally
        {
            Directory.Delete(tmpDir, true);
        }
    }

    [Fact]
    public void OnAssetLoaded_FiresOnFirstLoad()
    {
        var tmpDir = Path.Combine(Path.GetTempPath(), "obsydian_content_test_" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(tmpDir);
        File.WriteAllText(Path.Combine(tmpDir, "test.txt"), "hello");

        try
        {
            var mgr = new ContentManager(tmpDir);
            mgr.RegisterLoader(new TextLoader());

            string? loadedPath = null;
            mgr.OnAssetLoaded += (path, _) => loadedPath = path;

            mgr.Load<TextAsset>("test.txt");
            Assert.Equal("test.txt", loadedPath);

            loadedPath = null;
            mgr.Load<TextAsset>("test.txt"); // Cached — should NOT fire again
            Assert.Null(loadedPath);
        }
        finally
        {
            Directory.Delete(tmpDir, true);
        }
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
