using ImGuiNET;
using Obsydian.Core;
using Obsydian.Core.Scenes;
using Obsydian.DevTools.Panels;
using Obsydian.Input;
using Obsydian.Platform.Desktop.Rendering;
using Silk.NET.Input;
using Silk.NET.OpenGL;
using Silk.NET.OpenGL.Extensions.ImGui;
using Silk.NET.Windowing;

namespace Obsydian.DevTools;

/// <summary>
/// Dear ImGui-based developer tools overlay. Toggled with F3.
/// Provides: performance profiler, entity inspector, log console,
/// scene hierarchy, and ECS system profiler as dockable ImGui windows.
/// </summary>
public sealed class DevToolsOverlay : IDisposable
{
    private readonly ImGuiController _controller;

    private readonly PerformancePanel _performancePanel;
    private readonly EntityInspectorPanel _entityInspectorPanel;
    private readonly LogConsolePanel _logConsolePanel;
    private readonly SceneHierarchyPanel _sceneHierarchyPanel;
    private readonly SystemProfilerPanel _systemProfilerPanel;

    public bool IsVisible { get; set; }
    public Obsydian.Input.Key ToggleKey { get; set; } = Obsydian.Input.Key.F3;

    // Per-panel visibility toggles (only Performance on by default)
    private bool _showPerformance = true;
    private bool _showEntityInspector;
    private bool _showLogConsole;
    private bool _showSceneHierarchy;
    private bool _showSystemProfiler;

    /// <summary>True when ImGui wants to capture mouse input (user interacting with a panel).</summary>
    public bool WantCaptureMouse => IsVisible && ImGui.GetIO().WantCaptureMouse;

    /// <summary>True when ImGui wants to capture keyboard input (user typing in a text field).</summary>
    public bool WantCaptureKeyboard => IsVisible && ImGui.GetIO().WantCaptureKeyboard;

    public DevToolsOverlay(
        GL gl,
        IView nativeWindow,
        IInputContext inputContext,
        Engine engine,
        GlRenderer renderer,
        SceneManager? sceneManager = null)
    {
        _controller = new ImGuiController(gl, nativeWindow, inputContext);

        _performancePanel = new PerformancePanel(engine, renderer);
        _entityInspectorPanel = new EntityInspectorPanel(engine);
        _logConsolePanel = new LogConsolePanel();
        _sceneHierarchyPanel = new SceneHierarchyPanel(sceneManager);
        _systemProfilerPanel = new SystemProfilerPanel(engine);

        // Configure ImGui style
        var style = ImGui.GetStyle();
        style.WindowRounding = 4f;
        style.FrameRounding = 2f;
        style.Alpha = 0.95f;
        ImGui.StyleColorsDark();
    }

    /// <summary>
    /// Update ImGui frame. Call AFTER engine update, BEFORE Input.BeginFrame().
    /// </summary>
    public void Update(float dt, InputManager input)
    {
        if (input.IsKeyPressed(ToggleKey))
            IsVisible = !IsVisible;

        // Always update the controller so it processes input events correctly
        _controller.Update(dt);

        if (!IsVisible) return;

        // Main menu bar with panel toggles
        if (ImGui.BeginMainMenuBar())
        {
            if (ImGui.BeginMenu("Panels"))
            {
                ImGui.MenuItem("Performance", null, ref _showPerformance);
                ImGui.MenuItem("Entity Inspector", null, ref _showEntityInspector);
                ImGui.MenuItem("System Profiler", null, ref _showSystemProfiler);
                ImGui.MenuItem("Log Console", null, ref _showLogConsole);
                ImGui.MenuItem("Scene Hierarchy", null, ref _showSceneHierarchy);
                ImGui.EndMenu();
            }

            // Quick FPS readout in the menu bar
            var fps = dt > 0 ? 1f / dt : 0f;
            var fpsText = $"FPS: {fps:F0}";
            var textWidth = ImGui.CalcTextSize(fpsText).X;
            ImGui.SameLine(ImGui.GetWindowWidth() - textWidth - 10);
            ImGui.Text(fpsText);

            ImGui.EndMainMenuBar();
        }

        // Draw only visible panels
        if (_showPerformance) _performancePanel.Draw(dt);
        if (_showEntityInspector) _entityInspectorPanel.Draw();
        if (_showLogConsole) _logConsolePanel.Draw();
        if (_showSceneHierarchy) _sceneHierarchyPanel.Draw();
        if (_showSystemProfiler) _systemProfilerPanel.Draw();
    }

    /// <summary>
    /// Render ImGui draw data. Call AFTER Renderer.EndFrame().
    /// </summary>
    public void Render()
    {
        if (!IsVisible) return;
        _controller.Render();
    }

    public void Dispose()
    {
        _logConsolePanel.Dispose();
        _controller.Dispose();
    }
}
