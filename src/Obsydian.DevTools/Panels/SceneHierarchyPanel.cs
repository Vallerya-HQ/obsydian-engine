using System.Numerics;
using ImGuiNET;
using Obsydian.Core.Scenes;

namespace Obsydian.DevTools.Panels;

/// <summary>
/// Scene hierarchy panel — shows the current scene stack from SceneManager.
/// Optional: only renders if a SceneManager reference was provided.
/// </summary>
internal sealed class SceneHierarchyPanel
{
    private readonly SceneManager? _sceneManager;

    public SceneHierarchyPanel(SceneManager? sceneManager)
    {
        _sceneManager = sceneManager;
    }

    public void Draw()
    {
        ImGui.SetNextWindowSize(new Vector2(280, 200), ImGuiCond.FirstUseEver);
        if (!ImGui.Begin("Scene Hierarchy"))
        {
            ImGui.End();
            return;
        }

        if (_sceneManager is null)
        {
            ImGui.TextDisabled("No SceneManager provided.");
            ImGui.End();
            return;
        }

        var current = _sceneManager.Current;
        if (current is null)
        {
            ImGui.TextDisabled("No active scene.");
        }
        else
        {
            ImGui.Text("Active Scene:");
            ImGui.Indent();
            ImGui.BulletText(current.GetType().Name);
            ImGui.Unindent();
        }

        ImGui.End();
    }
}
