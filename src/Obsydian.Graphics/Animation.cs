using Obsydian.Core.Math;

namespace Obsydian.Graphics;

/// <summary>
/// Frame-based sprite animation. References cells from a SpriteSheet.
/// </summary>
public sealed class Animation
{
    public string Name { get; }
    public SpriteSheet SpriteSheet { get; }
    public IReadOnlyList<Rect> Frames { get; }
    public float FrameDuration { get; }
    public bool Looping { get; }

    public Animation(string name, SpriteSheet spriteSheet, IReadOnlyList<Rect> frames, float frameDuration, bool looping = true)
    {
        Name = name;
        SpriteSheet = spriteSheet;
        Frames = frames;
        FrameDuration = frameDuration;
        Looping = looping;
    }
}

/// <summary>
/// Plays an Animation, tracking current frame and elapsed time.
/// </summary>
public sealed class AnimationPlayer
{
    public Animation? Current { get; private set; }
    public int CurrentFrame { get; private set; }
    public bool IsPlaying { get; private set; }
    public bool IsFinished { get; private set; }

    private float _elapsed;

    public void Play(Animation animation)
    {
        if (Current == animation && IsPlaying) return;
        Current = animation;
        CurrentFrame = 0;
        _elapsed = 0;
        IsPlaying = true;
        IsFinished = false;
    }

    public void Stop()
    {
        IsPlaying = false;
    }

    public void Update(float deltaTime)
    {
        if (!IsPlaying || Current is null) return;

        _elapsed += deltaTime;
        if (_elapsed >= Current.FrameDuration)
        {
            _elapsed -= Current.FrameDuration;
            CurrentFrame++;

            if (CurrentFrame >= Current.Frames.Count)
            {
                if (Current.Looping)
                    CurrentFrame = 0;
                else
                {
                    CurrentFrame = Current.Frames.Count - 1;
                    IsPlaying = false;
                    IsFinished = true;
                }
            }
        }
    }

    public Rect? CurrentSourceRect => Current?.Frames[CurrentFrame];
}
