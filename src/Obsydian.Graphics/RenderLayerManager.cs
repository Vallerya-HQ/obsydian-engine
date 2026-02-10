using Obsydian.Core.Math;

namespace Obsydian.Graphics;

/// <summary>
/// A draw command queued for deferred rendering.
/// </summary>
public readonly record struct DrawCommand(
    int Layer,
    float SortY,
    Texture? Texture,
    Vec2 Position,
    Rect? SourceRect,
    Vec2? Scale,
    float Rotation,
    Color? Tint);

/// <summary>
/// Manages render ordering by collecting draw commands into layers,
/// then flushing them in order. Within a layer, commands sort by Y position
/// for depth-correct sprite rendering (painter's algorithm).
/// </summary>
public sealed class RenderLayerManager
{
    /// <summary>Standard layer constants. Custom layers can use any int.</summary>
    public static class Layers
    {
        public const int Background = 0;
        public const int Terrain = 100;
        public const int Entities = 200;
        public const int Foreground = 300;
        public const int Weather = 400;
        public const int UI = 500;
        public const int Overlay = 600;
    }

    private readonly List<DrawCommand> _commands = [];

    /// <summary>Queue a sprite to be drawn later at the specified layer.</summary>
    public void Draw(int layer, Texture texture, Vec2 position, Rect? sourceRect = null,
        Vec2? scale = null, float rotation = 0f, Color? tint = null, float? sortY = null)
    {
        _commands.Add(new DrawCommand(layer, sortY ?? position.Y, texture, position, sourceRect, scale, rotation, tint));
    }

    /// <summary>
    /// Flush all queued commands to the renderer, sorted by layer then Y.
    /// Clears the queue after flushing.
    /// </summary>
    public void Flush(IRenderer renderer)
    {
        _commands.Sort((a, b) =>
        {
            int layerCmp = a.Layer.CompareTo(b.Layer);
            return layerCmp != 0 ? layerCmp : a.SortY.CompareTo(b.SortY);
        });

        foreach (var cmd in _commands)
        {
            if (cmd.Texture is not null)
                renderer.DrawSprite(cmd.Texture, cmd.Position, cmd.SourceRect, cmd.Scale, cmd.Rotation, cmd.Tint);
        }

        _commands.Clear();
    }

    /// <summary>Number of queued draw commands.</summary>
    public int CommandCount => _commands.Count;

    /// <summary>Clear without rendering.</summary>
    public void Clear() => _commands.Clear();
}
