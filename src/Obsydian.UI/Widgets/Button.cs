using Obsydian.Core.Math;
using Obsydian.Graphics;
using Obsydian.Input;

namespace Obsydian.UI.Widgets;

/// <summary>
/// Clickable button with text label, hover highlight, and click callback.
/// </summary>
public sealed class Button : UIElement
{
    public string Text { get; set; } = "";
    public Color NormalColor { get; set; } = new(60, 60, 60);
    public Color HoverColor { get; set; } = new(80, 80, 120);
    public Color PressedColor { get; set; } = new(40, 40, 80);
    public Color TextColor { get; set; } = Color.White;
    public Color BorderColor { get; set; } = Color.White;
    public float TextScale { get; set; } = 2f;

    public event Action? OnClick;

    public bool IsHovered { get; private set; }
    public bool IsPressed { get; private set; }

    public override void Update(float deltaTime, InputManager input)
    {
        if (!Visible || !Enabled) return;

        IsHovered = HitTest(input.MousePosition);
        IsPressed = IsHovered && input.IsMouseDown(MouseButton.Left);

        if (IsHovered && input.IsMousePressed(MouseButton.Left))
            OnClick?.Invoke();
    }

    public override void Draw(IRenderer renderer)
    {
        if (!Visible) return;

        var bgColor = IsPressed ? PressedColor : IsHovered ? HoverColor : NormalColor;
        renderer.DrawRect(Bounds, bgColor);
        renderer.DrawRect(Bounds, BorderColor, filled: false);

        if (!string.IsNullOrEmpty(Text))
        {
            // Center text roughly
            var textX = Bounds.X + 8;
            var textY = Bounds.Y + (Bounds.Height - 7 * TextScale) / 2;
            renderer.DrawText(Text, new Vec2(textX, textY), TextColor, TextScale);
        }
    }
}
