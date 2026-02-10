using System.Numerics;
using ImGuiNET;
using Obsydian.Core;

namespace Obsydian.DevTools.Panels;

/// <summary>
/// ECS system profiler panel — lists all registered GameSystems,
/// shows per-system update time, and allows toggling systems on/off.
/// </summary>
internal sealed class SystemProfilerPanel
{
    private readonly Engine _engine;

    public SystemProfilerPanel(Engine engine)
    {
        _engine = engine;
    }

    public void Draw()
    {
        ImGui.SetNextWindowSize(new Vector2(380, 300), ImGuiCond.FirstUseEver);
        if (!ImGui.Begin("System Profiler"))
        {
            ImGui.End();
            return;
        }

        var systems = _engine.Systems.Systems;
        var timings = _engine.Systems.SystemTimings;

        if (systems.Count == 0)
        {
            ImGui.TextDisabled("No systems registered.");
            ImGui.End();
            return;
        }

        ImGui.Columns(3, "SystemColumns");
        ImGui.SetColumnWidth(0, 30);
        ImGui.SetColumnWidth(1, 230);

        ImGui.Text("On"); ImGui.NextColumn();
        ImGui.Text("System"); ImGui.NextColumn();
        ImGui.Text("Time (ms)"); ImGui.NextColumn();
        ImGui.Separator();

        double totalMs = 0;
        for (int i = 0; i < systems.Count; i++)
        {
            var system = systems[i];
            double ms = i < timings.Count ? timings[i].LastUpdateMs : 0;
            totalMs += ms;

            bool enabled = system.Enabled;
            ImGui.PushID(i);
            if (ImGui.Checkbox("##enabled", ref enabled))
                system.Enabled = enabled;
            ImGui.PopID();

            ImGui.NextColumn();

            string name = system.GetType().Name;
            if (!system.Enabled)
                ImGui.TextDisabled(name);
            else
                ImGui.Text(name);

            ImGui.NextColumn();

            if (system.Enabled)
            {
                var color = ms > 5.0 ? new Vector4(1, 0.3f, 0.3f, 1)
                          : ms > 2.0 ? new Vector4(1, 0.9f, 0.3f, 1)
                          : new Vector4(0.4f, 0.9f, 0.4f, 1);
                ImGui.TextColored(color, $"{ms:F3}");
            }
            else
            {
                ImGui.TextDisabled("—");
            }

            ImGui.NextColumn();
        }

        ImGui.Separator();
        ImGui.NextColumn();
        ImGui.Text("Total");
        ImGui.NextColumn();
        ImGui.Text($"{totalMs:F3}");
        ImGui.NextColumn();

        ImGui.Columns(1);
        ImGui.End();
    }
}
