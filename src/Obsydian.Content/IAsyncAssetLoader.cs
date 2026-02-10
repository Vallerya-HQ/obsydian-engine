namespace Obsydian.Content;

/// <summary>
/// Optional async asset loading interface. Loaders that support async
/// loading implement this for background/concurrent loading.
/// </summary>
public interface IAsyncAssetLoader<T> : IAssetLoader<T> where T : class
{
    Task<T> LoadAsync(string fullPath, CancellationToken ct = default);
}
