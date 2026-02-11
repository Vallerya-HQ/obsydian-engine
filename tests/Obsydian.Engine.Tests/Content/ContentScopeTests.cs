using Obsydian.Content;

namespace Obsydian.Engine.Tests.Content;

public class ContentScopeTests
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
    public void Scope_TracksLoadedAssets()
    {
        using var content = new ContentManager(Path.GetTempPath());
        content.RegisterLoader(new FakeLoader());

        using var scope = new ContentScope(content, "TestLevel");
        scope.Load<FakeAsset>("a.dat");
        scope.Load<FakeAsset>("b.dat");

        Assert.Equal(2, scope.AssetCount);
    }

    [Fact]
    public void Dispose_ReleasesAllScopedAssets()
    {
        using var content = new ContentManager(Path.GetTempPath());
        content.RegisterLoader(new FakeLoader());

        var scope = new ContentScope(content, "TestLevel");
        scope.Load<FakeAsset>("a.dat");
        scope.Load<FakeAsset>("b.dat");

        Assert.True(content.IsCached<FakeAsset>("a.dat"));
        Assert.True(content.IsCached<FakeAsset>("b.dat"));

        scope.Dispose();

        Assert.False(content.IsCached<FakeAsset>("a.dat"));
        Assert.False(content.IsCached<FakeAsset>("b.dat"));
    }

    [Fact]
    public void PermanentAssets_SurviveScopeDispose()
    {
        using var content = new ContentManager(Path.GetTempPath());
        content.RegisterLoader(new FakeLoader());

        var scope = new ContentScope(content, "TestLevel");
        scope.Load<FakeAsset>("scoped.dat", ContentLifetime.Scoped);
        scope.Load<FakeAsset>("permanent.dat", ContentLifetime.Permanent);

        scope.Dispose();

        Assert.False(content.IsCached<FakeAsset>("scoped.dat"));
        Assert.True(content.IsCached<FakeAsset>("permanent.dat")); // Still alive
    }
}
