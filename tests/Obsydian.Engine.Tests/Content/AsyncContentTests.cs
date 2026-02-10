using Obsydian.Content;

namespace Obsydian.Engine.Tests.Content;

public class AsyncContentTests
{
    private sealed class FakeData
    {
        public string Value { get; init; } = "";
    }

    private sealed class FakeLoader : IAssetLoader<FakeData>
    {
        public FakeData Load(string fullPath) => new() { Value = fullPath };
    }

    private sealed class FakeAsyncLoader : IAsyncAssetLoader<FakeData>
    {
        public FakeData Load(string fullPath) => new() { Value = fullPath };

        public async Task<FakeData> LoadAsync(string fullPath, CancellationToken ct = default)
        {
            await Task.Delay(1, ct);
            return new FakeData { Value = $"async:{fullPath}" };
        }
    }

    [Fact]
    public async Task LoadAsync_UsesSyncFallback()
    {
        var dir = Path.GetTempPath();
        using var content = new ContentManager(dir);
        content.RegisterLoader<FakeData>(new FakeLoader());

        var result = await content.LoadAsync<FakeData>("test.dat");

        Assert.Contains("test.dat", result.Value);
    }

    [Fact]
    public async Task LoadAsync_UsesAsyncLoader_WhenAvailable()
    {
        var dir = Path.GetTempPath();
        using var content = new ContentManager(dir);
        content.RegisterLoader<FakeData>(new FakeAsyncLoader());

        var result = await content.LoadAsync<FakeData>("test.dat");

        Assert.StartsWith("async:", result.Value);
    }

    [Fact]
    public async Task LoadAsync_CachesResult()
    {
        var dir = Path.GetTempPath();
        using var content = new ContentManager(dir);
        content.RegisterLoader<FakeData>(new FakeLoader());

        var first = await content.LoadAsync<FakeData>("test.dat");
        var second = await content.LoadAsync<FakeData>("test.dat");

        Assert.Same(first, second);
    }

    [Fact]
    public async Task AsyncContentQueue_TracksProgress()
    {
        var dir = Path.GetTempPath();
        using var content = new ContentManager(dir);
        content.RegisterLoader<FakeData>(new FakeLoader());
        var queue = new AsyncContentQueue(content);

        var task1 = queue.QueueLoad<FakeData>("a.dat");
        var task2 = queue.QueueLoad<FakeData>("b.dat");

        await Task.WhenAll(task1, task2);

        Assert.Equal(2, queue.TotalQueued);
        Assert.Equal(2, queue.Loaded);
        Assert.True(queue.IsComplete);
        Assert.Equal(1f, queue.Progress);
    }
}
