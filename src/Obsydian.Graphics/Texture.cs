namespace Obsydian.Graphics;

/// <summary>
/// Represents a loaded texture/image. Handle to GPU-side texture data.
/// </summary>
public sealed class Texture : IDisposable
{
    public int Id { get; init; }
    public int Width { get; init; }
    public int Height { get; init; }
    public string? Name { get; init; }

    private bool _disposed;

    /// <summary>
    /// Called by the renderer when the texture's GPU resources should be freed.
    /// </summary>
    public Action<int>? OnDispose { get; init; }

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;
        OnDispose?.Invoke(Id);
        GC.SuppressFinalize(this);
    }

    ~Texture() => Dispose();
}
