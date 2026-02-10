using System.Numerics;
using ImGuiNET;
using Obsydian.Core;
using Obsydian.Platform.Desktop.Rendering;

namespace Obsydian.DevTools.Panels;

/// <summary>
/// Performance panel showing FPS, frame time, draw calls, entity count, and GC memory.
/// Includes a frame time history graph (last 120 frames).
/// </summary>
internal sealed class PerformancePanel
{
    private const int HistorySize = 120;

    private readonly Engine _engine;
    private readonly GlRenderer _renderer;
    private readonly float[] _frameTimeHistory = new float[HistorySize];
    private int _historyIndex;
    private float _fpsSmoothed;

    public PerformancePanel(Engine engine, GlRenderer renderer)
    {
        _engine = engine;
        _renderer = renderer;
    }

    public void Draw(float dt)
    {
        // Update ring buffer
        float frameTimeMs = dt * 1000f;
        _frameTimeHistory[_historyIndex] = frameTimeMs;
        _historyIndex = (_historyIndex + 1) % HistorySize;

        // Smooth FPS
        float fps = dt > 0 ? 1f / dt : 0f;
        _fpsSmoothed = _fpsSmoothed == 0 ? fps : _fpsSmoothed * 0.95f + fps * 0.05f;

        // Compute min/max/avg
        float min = float.MaxValue, max = 0f, sum = 0f;
        for (int i = 0; i < HistorySize; i++)
        {
            float v = _frameTimeHistory[i];
            if (v > 0)
            {
                if (v < min) min = v;
                if (v > max) max = v;
                sum += v;
            }
        }
        float avg = sum / HistorySize;
        if (min == float.MaxValue) min = 0;

        ImGui.SetNextWindowSize(new Vector2(320, 260), ImGuiCond.FirstUseEver);
        if (!ImGui.Begin("Performance"))
        {
            ImGui.End();
            return;
        }

        ImGui.Text($"FPS: {_fpsSmoothed:F1}");
        ImGui.Text($"Frame Time: {frameTimeMs:F2} ms");
        ImGui.Text($"  Min: {min:F2}  Max: {max:F2}  Avg: {avg:F2} ms");
        ImGui.Separator();
        ImGui.Text($"Draw Calls: {_renderer.LastDrawCallCount}");
        ImGui.Text($"Entities: {_engine.World.EntityCount}");
        ImGui.Text($"GC Memory: {GC.GetTotalMemory(false) / 1024:N0} KB");
        ImGui.Separator();

        // Reorder the ring buffer for display (oldest first)
        var ordered = new float[HistorySize];
        for (int i = 0; i < HistorySize; i++)
            ordered[i] = _frameTimeHistory[(_historyIndex + i) % HistorySize];

        ImGui.PlotLines("##frametime", ref ordered[0], HistorySize, 0,
            $"Frame Time (ms)", 0f, MathF.Max(max * 1.2f, 16.67f),
            new Vector2(ImGui.GetContentRegionAvail().X, 60));

        ImGui.End();
    }
}
