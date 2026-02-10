using Obsydian.Core.Math;

namespace Obsydian.Graphics;

/// <summary>
/// State of a screen transition.
/// </summary>
public enum TransitionState { None, FadingOut, FadingIn, Complete }

/// <summary>
/// Smooth screen transition (fade to color, then fade in).
/// Use with SceneManager: start FadeOut, switch scene at midpoint, then FadeIn.
/// </summary>
public sealed class ScreenTransition
{
    public TransitionState State { get; private set; } = TransitionState.None;
    public float Progress { get; private set; }
    public Color OverlayColor { get; set; } = Color.Black;
    public float FadeOutDuration { get; set; } = 0.3f;
    public float FadeInDuration { get; set; } = 0.3f;

    /// <summary>Alpha of the overlay (0 = transparent, 1 = fully opaque).</summary>
    public float Alpha { get; private set; }

    private float _timer;
    private Action? _onMidpoint;
    private Action? _onComplete;

    /// <summary>
    /// Begin a full fade-out → callback → fade-in transition.
    /// </summary>
    public void Start(Action? onMidpoint = null, Action? onComplete = null)
    {
        _onMidpoint = onMidpoint;
        _onComplete = onComplete;
        _timer = 0;
        State = TransitionState.FadingOut;
        Alpha = 0;
    }

    /// <summary>Begin only a fade-in (e.g., when entering a new scene).</summary>
    public void FadeIn(Action? onComplete = null)
    {
        _onComplete = onComplete;
        _onMidpoint = null;
        _timer = 0;
        State = TransitionState.FadingIn;
        Alpha = 1;
    }

    public void Update(float deltaTime)
    {
        if (State == TransitionState.None || State == TransitionState.Complete)
            return;

        _timer += deltaTime;

        if (State == TransitionState.FadingOut)
        {
            Progress = System.Math.Clamp(_timer / FadeOutDuration, 0f, 1f);
            Alpha = Progress;

            if (_timer >= FadeOutDuration)
            {
                Alpha = 1f;
                _onMidpoint?.Invoke();
                _timer = 0;
                State = TransitionState.FadingIn;
            }
        }
        else if (State == TransitionState.FadingIn)
        {
            Progress = System.Math.Clamp(_timer / FadeInDuration, 0f, 1f);
            Alpha = 1f - Progress;

            if (_timer >= FadeInDuration)
            {
                Alpha = 0f;
                State = TransitionState.Complete;
                _onComplete?.Invoke();
                State = TransitionState.None;
            }
        }
    }

    /// <summary>
    /// Draw the fade overlay. Call after all scene rendering.
    /// </summary>
    public void Draw(IRenderer renderer, float screenWidth, float screenHeight)
    {
        if (State == TransitionState.None || Alpha <= 0f)
            return;

        var color = OverlayColor.WithAlpha((byte)(Alpha * 255));
        renderer.DrawRect(new Rect(0, 0, screenWidth, screenHeight), color);
    }

    public bool IsActive => State != TransitionState.None && State != TransitionState.Complete;
}
