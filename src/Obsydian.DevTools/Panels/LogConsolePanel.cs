using System.Numerics;
using ImGuiNET;
using Obsydian.Core.Logging;

namespace Obsydian.DevTools.Panels;

/// <summary>
/// Log console panel — captures engine Log output, supports filtering by level and tag.
/// Ring buffer of the last 500 entries with auto-scroll.
/// </summary>
internal sealed class LogConsolePanel : IDisposable
{
    private const int MaxEntries = 500;

    private readonly record struct LogEntry(LogLevel Level, string Tag, string Message, string Timestamp);

    private readonly LogEntry[] _entries = new LogEntry[MaxEntries];
    private int _head;
    private int _count;
    private bool _autoScroll = true;
    private string _tagFilter = "";
    private readonly bool[] _levelFilters = [true, true, true, true, true, true]; // Trace..Fatal

    private readonly Action<LogLevel, string, string>? _previousOnLog;

    public LogConsolePanel()
    {
        // Chain onto existing OnLog callback
        _previousOnLog = Log.OnLog;
        Log.OnLog = OnLogReceived;
    }

    private void OnLogReceived(LogLevel level, string tag, string message)
    {
        // Forward to previous handler (e.g., console output)
        _previousOnLog?.Invoke(level, tag, message);

        var timestamp = DateTime.Now.ToString("HH:mm:ss.fff");
        _entries[_head] = new LogEntry(level, tag, message, timestamp);
        _head = (_head + 1) % MaxEntries;
        if (_count < MaxEntries) _count++;
    }

    public void Draw()
    {
        ImGui.SetNextWindowSize(new Vector2(600, 300), ImGuiCond.FirstUseEver);
        if (!ImGui.Begin("Log Console"))
        {
            ImGui.End();
            return;
        }

        // Controls row
        if (ImGui.Button("Clear"))
        {
            _count = 0;
            _head = 0;
        }

        ImGui.SameLine();
        ImGui.Checkbox("Auto-scroll", ref _autoScroll);

        ImGui.SameLine();
        ImGui.SetNextItemWidth(150);
        ImGui.InputText("Tag Filter", ref _tagFilter, 64);

        // Level filter checkboxes
        var levelNames = new[] { "Trace", "Debug", "Info", "Warn", "Error", "Fatal" };
        for (int i = 0; i < levelNames.Length; i++)
        {
            ImGui.SameLine();
            ImGui.Checkbox(levelNames[i], ref _levelFilters[i]);
        }

        ImGui.Separator();

        // Log entries
        ImGui.BeginChild("LogScroll", Vector2.Zero, false, ImGuiWindowFlags.HorizontalScrollbar);

        int start = _count < MaxEntries ? 0 : _head;
        for (int i = 0; i < _count; i++)
        {
            int idx = (start + i) % MaxEntries;
            var entry = _entries[idx];

            // Apply filters
            if (!_levelFilters[(int)entry.Level]) continue;
            if (_tagFilter.Length > 0 && !entry.Tag.Contains(_tagFilter, StringComparison.OrdinalIgnoreCase)) continue;

            var color = GetLevelColor(entry.Level);
            ImGui.PushStyleColor(ImGuiCol.Text, color);
            ImGui.TextUnformatted($"[{entry.Timestamp}] [{entry.Level}] [{entry.Tag}] {entry.Message}");
            ImGui.PopStyleColor();
        }

        if (_autoScroll && ImGui.GetScrollY() >= ImGui.GetScrollMaxY())
            ImGui.SetScrollHereY(1.0f);

        ImGui.EndChild();
        ImGui.End();
    }

    private static Vector4 GetLevelColor(LogLevel level) => level switch
    {
        LogLevel.Trace => new Vector4(0.6f, 0.6f, 0.6f, 1f),
        LogLevel.Debug => new Vector4(0.8f, 0.8f, 0.8f, 1f),
        LogLevel.Info => new Vector4(0.4f, 0.9f, 0.4f, 1f),
        LogLevel.Warn => new Vector4(1f, 0.9f, 0.3f, 1f),
        LogLevel.Error => new Vector4(1f, 0.4f, 0.4f, 1f),
        LogLevel.Fatal => new Vector4(1f, 0.2f, 0.2f, 1f),
        _ => new Vector4(1f, 1f, 1f, 1f)
    };

    public void Dispose()
    {
        // Restore previous log handler
        Log.OnLog = _previousOnLog;
    }
}
