using System.Numerics;
using System.Reflection;
using ImGuiNET;
using Obsydian.Core;
using Obsydian.Core.ECS;

namespace Obsydian.DevTools.Panels;

/// <summary>
/// Entity inspector panel — lists all alive entities, shows components
/// and their field values on the selected entity (read-only for V1).
/// </summary>
internal sealed class EntityInspectorPanel
{
    private readonly Engine _engine;
    private int _selectedEntityId = -1;

    public EntityInspectorPanel(Engine engine)
    {
        _engine = engine;
    }

    public void Draw()
    {
        ImGui.SetNextWindowSize(new Vector2(400, 500), ImGuiCond.FirstUseEver);
        if (!ImGui.Begin("Entity Inspector"))
        {
            ImGui.End();
            return;
        }

        var world = _engine.World;

        // Entity list
        ImGui.BeginChild("EntityList", new Vector2(120, 0), true);
        ImGui.Text($"Entities ({world.EntityCount})");
        ImGui.Separator();

        foreach (int id in world.AliveEntityIds)
        {
            bool isSelected = _selectedEntityId == id;
            if (ImGui.Selectable($"Entity {id}", isSelected))
                _selectedEntityId = id;
        }

        ImGui.EndChild();

        ImGui.SameLine();

        // Component details
        ImGui.BeginChild("ComponentDetails");

        if (_selectedEntityId >= 0 && world.IsAlive(new Entity(_selectedEntityId)))
        {
            ImGui.Text($"Entity {_selectedEntityId}");
            ImGui.Separator();

            var entity = new Entity(_selectedEntityId);
            foreach (var (type, storeObj) in world.Stores)
            {
                // Check if this store has a component for our entity via reflection
                var hasMethod = storeObj.GetType().GetMethod("Has");
                if (hasMethod is null) continue;

                bool has = (bool)hasMethod.Invoke(storeObj, [_selectedEntityId])!;
                if (!has) continue;

                if (ImGui.CollapsingHeader(type.Name, ImGuiTreeNodeFlags.DefaultOpen))
                {
                    // Get the component data via reflection
                    var getMethod = storeObj.GetType().GetMethod("Get");
                    if (getMethod is null) continue;

                    try
                    {
                        // Get returns ref T, so we use the non-ref approach by calling via the store's dense array
                        var countProp = storeObj.GetType().GetProperty("Count");
                        var getEntityAtMethod = storeObj.GetType().GetMethod("GetEntityAt");
                        if (countProp is null || getEntityAtMethod is null) continue;

                        int count = (int)countProp.GetValue(storeObj)!;
                        object? component = null;

                        // Find the component via dense iteration
                        for (int i = 0; i < count; i++)
                        {
                            int eid = (int)getEntityAtMethod.Invoke(storeObj, [i])!;
                            if (eid == _selectedEntityId)
                            {
                                // Access _data[i] via reflection
                                var dataField = storeObj.GetType().GetField("_data", BindingFlags.NonPublic | BindingFlags.Instance);
                                if (dataField?.GetValue(storeObj) is Array dataArray)
                                    component = dataArray.GetValue(i);
                                break;
                            }
                        }

                        if (component is not null)
                        {
                            DrawComponentFields(component);
                        }
                    }
                    catch
                    {
                        ImGui.TextColored(new Vector4(1, 0.4f, 0.4f, 1), "(unable to read)");
                    }
                }
            }
        }
        else
        {
            _selectedEntityId = -1;
            ImGui.TextDisabled("Select an entity to inspect.");
        }

        ImGui.EndChild();
        ImGui.End();
    }

    private static void DrawComponentFields(object component)
    {
        var type = component.GetType();
        var fields = type.GetFields(BindingFlags.Public | BindingFlags.Instance);
        var properties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance);

        foreach (var field in fields)
        {
            var value = field.GetValue(component);
            ImGui.Text($"  {field.Name}: {FormatValue(value)}");
        }

        foreach (var prop in properties)
        {
            if (!prop.CanRead || prop.GetIndexParameters().Length > 0) continue;
            try
            {
                var value = prop.GetValue(component);
                ImGui.Text($"  {prop.Name}: {FormatValue(value)}");
            }
            catch
            {
                ImGui.Text($"  {prop.Name}: (error)");
            }
        }
    }

    private static string FormatValue(object? value) => value switch
    {
        null => "null",
        float f => f.ToString("F3"),
        double d => d.ToString("F3"),
        _ => value.ToString() ?? "null"
    };
}
