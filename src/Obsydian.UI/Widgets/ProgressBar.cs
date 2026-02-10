using Obsydian.Core.Math;
using Obsydian.Graphics;
using Obsydian.Input;

namespace Obsydian.UI.Widgets;

/// <summary>
/// Horizontal progress bar. Value ranges 0.0 to 1.0.
/// </summary>
public sealed class ProgressBar : UIElement
{
    public float Value { get; set; } = 1f;
    public Color FillColor { get; set; } = Color.Green;
    public Color BackgroundColor { get; set; } = new(40, 40, 40);
    public Color BorderColor { get; set; } = Color.White;

    public override void Update(float deltaTime, InputManager input) { }

    public override void Draw(IRenderer renderer)
    {
        if (!Visible) return;

        renderer.DrawRect(Bounds, BackgroundColor);

        var fillWidth = Bounds.Width * System.Math.Clamp(Value, 0f, 1f);
        if (fillWidth > 0)
        {
            var fillRect = new Rect(Bounds.X, Bounds.Y, fillWidth, Bounds.Height);
            renderer.DrawRect(fillRect, FillColor);
        }

        renderer.DrawRect(Bounds, BorderColor, filled: false);
    }
}
