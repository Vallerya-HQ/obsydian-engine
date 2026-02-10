using Obsydian.Core.Math;

namespace Obsydian.Graphics;

/// <summary>
/// Direction for character-facing animations.
/// </summary>
public enum FacingDirection { Down, Up, Left, Right }

/// <summary>
/// A set of animations keyed by name and direction.
/// Typical usage: "walk" with Up/Down/Left/Right variants, "idle" with same.
/// </summary>
public sealed class AnimationSet
{
    private readonly Dictionary<string, Dictionary<FacingDirection, Animation>> _animations = [];

    /// <summary>Add a directional animation variant.</summary>
    public void Add(string name, FacingDirection direction, Animation animation)
    {
        if (!_animations.TryGetValue(name, out var dirMap))
        {
            dirMap = [];
            _animations[name] = dirMap;
        }
        dirMap[direction] = animation;
    }

    /// <summary>Get animation for a name and direction. Falls back to Down if direction missing.</summary>
    public Animation? Get(string name, FacingDirection direction)
    {
        if (!_animations.TryGetValue(name, out var dirMap))
            return null;

        if (dirMap.TryGetValue(direction, out var anim))
            return anim;

        // Fallback to Down, then first available
        if (dirMap.TryGetValue(FacingDirection.Down, out anim))
            return anim;

        return dirMap.Values.FirstOrDefault();
    }

    /// <summary>Check if an animation exists.</summary>
    public bool Has(string name) => _animations.ContainsKey(name);

    /// <summary>
    /// Helper: create a 4-direction animation set from a sprite sheet.
    /// Assumes rows = directions (Down=0, Up=1, Left=2, Right=3) and columns = frames.
    /// </summary>
    public static AnimationSet FromGrid(string name, SpriteSheet sheet, int startRow, int frameCount, float frameDuration, bool looping = true)
    {
        var set = new AnimationSet();
        var directions = new[] { FacingDirection.Down, FacingDirection.Up, FacingDirection.Left, FacingDirection.Right };

        for (int d = 0; d < directions.Length && startRow + d < sheet.Rows; d++)
        {
            var frames = new List<Rect>();
            for (int f = 0; f < frameCount && f < sheet.Columns; f++)
                frames.Add(sheet.GetCell(f, startRow + d));

            set.Add(name, directions[d], new Animation($"{name}_{directions[d]}", sheet, frames, frameDuration, looping));
        }

        return set;
    }
}
