using Obsydian.Core.Math;

namespace Obsydian.Platform;

/// <summary>
/// Platform-agnostic window abstraction. Implemented per-platform.
/// </summary>
public interface IWindow
{
    string Title { get; set; }
    int Width { get; }
    int Height { get; }
    bool IsOpen { get; }
    bool IsFullscreen { get; set; }

    void Create(string title, int width, int height);
    void PollEvents();
    void SwapBuffers();
    void Close();

    event Action? OnClose;
    event Action<int, int>? OnResize;
}
