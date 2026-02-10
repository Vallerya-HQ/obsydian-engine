using Obsydian.Core;
using Obsydian.Core.ECS;
using Obsydian.Core.Math;
using Obsydian.Graphics;
using Obsydian.Input;

namespace Obsydian.UI;

/// <summary>
/// Debug overlay showing FPS, entity count, and custom debug lines.
/// Toggle with F3 key. Rendered on top of everything.
/// </summary>
public sealed class DebugOverlay
{
    public bool IsVisible { get; set; }
    public Key ToggleKey { get; set; } = Key.F3;
    public Color TextColor { get; set; } = Color.Green;
    public Color BackgroundColor { get; set; } = new(0, 0, 0, 140);
    public float TextScale { get; set; } = 1.5f;
    public float Padding { get; set; } = 6f;
    public float LineHeight { get; set; } = 14f;

    private readonly List<string> _customLines = [];
    private readonly List<Func<string>> _dynamicLines = [];

    /// <summary>Add a static debug line.</summary>
    public void AddLine(string line) => _customLines.Add(line);

    /// <summary>Add a dynamic debug line evaluated each frame.</summary>
    public void AddDynamic(Func<string> lineFunc) => _dynamicLines.Add(lineFunc);

    /// <summary>Clear all custom/dynamic lines.</summary>
    public void ClearLines()
    {
        _customLines.Clear();
        _dynamicLines.Clear();
    }

    public void Update(InputManager input)
    {
        if (input.IsKeyPressed(ToggleKey))
            IsVisible = !IsVisible;
    }

    public void Draw(IRenderer renderer, GameTime gameTime, World? world = null)
    {
        if (!IsVisible) return;

        var lines = new List<string>
        {
            $"FPS: {gameTime.Fps:F0}",
            $"Frame: {gameTime.FrameCount}",
            $"Time: {gameTime.TotalTime:F1}s",
            $"DT: {gameTime.DeltaTime * 1000:F1}ms",
        };

        if (world is not null)
            lines.Add($"Entities: {world.EntityCount}");

        lines.Add($"GC: {GC.GetTotalMemory(false) / 1024:N0} KB");

        lines.AddRange(_customLines);
        foreach (var fn in _dynamicLines)
            lines.Add(fn());

        float width = 200;
        float height = Padding * 2 + lines.Count * LineHeight;

        renderer.DrawRect(new Rect(Padding, Padding, width, height), BackgroundColor);

        float y = Padding + Padding;
        foreach (var line in lines)
        {
            renderer.DrawText(line, new Vec2(Padding + Padding, y), TextColor, TextScale);
            y += LineHeight;
        }
    }
}
