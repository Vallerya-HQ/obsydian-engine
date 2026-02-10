using Obsydian.Core.Logging;
using Obsydian.Platform;
using Silk.NET.Maths;
using Silk.NET.Windowing;

namespace Obsydian.Platform.Desktop;

/// <summary>
/// IWindow implementation backed by Silk.NET (GLFW on desktop).
/// The native window drives the game loop via its Update/Render callbacks.
/// </summary>
public sealed class DesktopWindow : IWindow
{
    private Silk.NET.Windowing.IWindow? _nativeWindow;

    public string Title
    {
        get => _nativeWindow?.Title ?? "";
        set { if (_nativeWindow is not null) _nativeWindow.Title = value; }
    }

    public int Width => _nativeWindow?.Size.X ?? 0;
    public int Height => _nativeWindow?.Size.Y ?? 0;

    /// <summary>Framebuffer size in actual pixels (may differ from Width/Height on HiDPI/Retina).</summary>
    public int FramebufferWidth => _nativeWindow?.FramebufferSize.X ?? 0;
    public int FramebufferHeight => _nativeWindow?.FramebufferSize.Y ?? 0;
    public bool IsOpen => _nativeWindow is { IsClosing: false };

    public bool IsFullscreen
    {
        get => _nativeWindow?.WindowState == WindowState.Fullscreen;
        set
        {
            if (_nativeWindow is not null)
                _nativeWindow.WindowState = value ? WindowState.Fullscreen : WindowState.Normal;
        }
    }

    public event Action? OnClose;
    public event Action<int, int>? OnResize;

    /// <summary>Called each frame by Silk.NET's event loop — use to pump engine update logic.</summary>
    public event Action<double>? OnUpdate;

    /// <summary>Called each frame by Silk.NET's event loop — use to pump engine render logic.</summary>
    public event Action<double>? OnRenderFrame;

    /// <summary>Called once after the OpenGL context is ready.</summary>
    public event Action? OnLoad;

    /// <summary>The underlying Silk.NET window. Available after Create().</summary>
    public Silk.NET.Windowing.IWindow NativeWindow =>
        _nativeWindow ?? throw new InvalidOperationException("Window not created yet.");

    public void Create(string title, int width, int height)
    {
        var options = WindowOptions.Default with
        {
            Title = title,
            Size = new Vector2D<int>(width, height),
            VSync = true,
            API = new GraphicsAPI(ContextAPI.OpenGL, ContextProfile.Core, ContextFlags.ForwardCompatible, new APIVersion(3, 3))
        };

        _nativeWindow = Window.Create(options);

        _nativeWindow.Load += () =>
        {
            Log.Info("Window", $"Created {width}x{height} OpenGL 3.3 window: {title}");
            OnLoad?.Invoke();
        };

        _nativeWindow.Update += dt => OnUpdate?.Invoke(dt);
        _nativeWindow.Render += dt => OnRenderFrame?.Invoke(dt);

        _nativeWindow.FramebufferResize += size =>
        {
            OnResize?.Invoke(size.X, size.Y);
        };

        _nativeWindow.Closing += () =>
        {
            OnClose?.Invoke();
        };
    }

    /// <summary>
    /// Enters the Silk.NET event loop. Blocks until the window is closed.
    /// </summary>
    public void Run()
    {
        NativeWindow.Run();
    }

    public void PollEvents()
    {
        // Silk.NET handles event polling internally in its Run loop.
        // This exists for IWindow compatibility in headless scenarios.
    }

    public void SwapBuffers()
    {
        // Silk.NET handles buffer swapping internally after the Render callback.
    }

    public void Close()
    {
        _nativeWindow?.Close();
    }

    public void Dispose()
    {
        _nativeWindow?.Dispose();
        _nativeWindow = null;
    }
}
