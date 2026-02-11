using Obsydian.Content;

namespace Obsydian.Engine.Tests.Content;

public class AssetHandleTests
{
    private sealed class FakeAsset
    {
        public string Name { get; init; } = "";
    }

    private sealed class FakeLoader : IAssetLoader<FakeAsset>
    {
        public FakeAsset Load(string fullPath) => new() { Name = Path.GetFileName(fullPath) };
    }

    [Fact]
    public void LoadHandle_ReturnsAsset()
    {
        using var content = new ContentManager(Path.GetTempPath());
        content.RegisterLoader(new FakeLoader());

        using var handle = content.LoadHandle<FakeAsset>("test.dat");
        Assert.NotNull(handle.Asset);
        Assert.Contains("test.dat", handle.Asset.Name);
    }

    [Fact]
    public void Dispose_ReleasesRefCount()
    {
        using var content = new ContentManager(Path.GetTempPath());
        content.RegisterLoader(new FakeLoader());

        var handle = content.LoadHandle<FakeAsset>("test.dat");
        Assert.Equal(1, content.GetRefCount<FakeAsset>("test.dat"));

        handle.Dispose();
        Assert.Equal(0, content.GetRefCount<FakeAsset>("test.dat"));
        Assert.False(content.IsCached<FakeAsset>("test.dat"));
    }

    [Fact]
    public void MultipleHandles_KeepAssetAlive()
    {
        using var content = new ContentManager(Path.GetTempPath());
        content.RegisterLoader(new FakeLoader());

        var h1 = content.LoadHandle<FakeAsset>("test.dat");
        var h2 = content.LoadHandle<FakeAsset>("test.dat");
        Assert.Equal(2, content.GetRefCount<FakeAsset>("test.dat"));

        h1.Dispose();
        Assert.True(content.IsCached<FakeAsset>("test.dat")); // Still alive via h2

        h2.Dispose();
        Assert.False(content.IsCached<FakeAsset>("test.dat")); // Now evicted
    }

    [Fact]
    public void ImplicitConversion_Works()
    {
        using var content = new ContentManager(Path.GetTempPath());
        content.RegisterLoader(new FakeLoader());

        using var handle = content.LoadHandle<FakeAsset>("test.dat");
        FakeAsset asset = handle; // Implicit conversion
        Assert.Same(handle.Asset, asset);
    }
}
