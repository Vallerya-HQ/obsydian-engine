using Obsydian.Core.Math;
using Obsydian.Graphics;
using Obsydian.Input;

namespace Obsydian.UI.Widgets;

/// <summary>
/// Text label. Draws text at the element's position using the renderer's DrawText.
/// </summary>
public sealed class Label : UIElement
{
    public string Text { get; set; } = "";
    public Color TextColor { get; set; } = Color.White;
    public float Scale { get; set; } = 2f;

    public override void Update(float deltaTime, InputManager input) { }

    public override void Draw(IRenderer renderer)
    {
        if (!Visible || string.IsNullOrEmpty(Text)) return;
        renderer.DrawText(Text, Bounds.Position, TextColor, Scale);
    }
}
