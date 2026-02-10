using Obsydian.Core.Math;
using Obsydian.Graphics;
using Obsydian.Input;

namespace Obsydian.UI.Widgets;

/// <summary>
/// Colored rectangular panel. Can act as a container for child elements.
/// </summary>
public sealed class Panel : UIElement
{
    public Color BackgroundColor { get; set; } = new(0, 0, 0, 180);
    public Color BorderColor { get; set; } = Color.White;
    public bool ShowBorder { get; set; } = true;

    public override void Update(float deltaTime, InputManager input)
    {
        base.Update(deltaTime, input);
    }

    public override void Draw(IRenderer renderer)
    {
        if (!Visible) return;
        renderer.DrawRect(Bounds, BackgroundColor);
        if (ShowBorder)
            renderer.DrawRect(Bounds, BorderColor, filled: false);
        base.Draw(renderer);
    }
}
