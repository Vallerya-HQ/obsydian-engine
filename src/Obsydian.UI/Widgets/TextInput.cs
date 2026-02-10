using Obsydian.Core.Math;
using Obsydian.Graphics;
using Obsydian.Input;

namespace Obsydian.UI.Widgets;

/// <summary>
/// Simple text input field. Click to focus, type to enter text.
/// Supports backspace and basic editing.
/// </summary>
public sealed class TextInput : UIElement
{
    public string Text { get; set; } = "";
    public string Placeholder { get; set; } = "";
    public int MaxLength { get; set; } = 256;
    public bool IsFocused { get; set; }

    public Color BackgroundColor { get; set; } = new(40, 40, 40);
    public Color FocusedBorderColor { get; set; } = Color.Gold;
    public Color UnfocusedBorderColor { get; set; } = new(100, 100, 100);
    public Color TextColor { get; set; } = Color.White;
    public Color PlaceholderColor { get; set; } = new(120, 120, 120);
    public Color CursorColor { get; set; } = Color.White;
    public float TextScale { get; set; } = 2f;

    private float _cursorBlink;

    public event Action<string>? OnTextChanged;
    public event Action<string>? OnSubmit;

    public override void Update(float deltaTime, InputManager input)
    {
        if (!Visible || !Enabled) return;

        // Focus/unfocus on click
        if (input.IsMousePressed(MouseButton.Left))
            IsFocused = HitTest(input.MousePosition);

        if (!IsFocused) return;

        _cursorBlink += deltaTime;
        if (_cursorBlink > 1f) _cursorBlink = 0;

        // Handle backspace
        if (input.IsKeyPressed(Key.Backspace) && Text.Length > 0)
        {
            Text = Text[..^1];
            OnTextChanged?.Invoke(Text);
        }

        // Handle enter/submit
        if (input.IsKeyPressed(Key.Enter))
        {
            OnSubmit?.Invoke(Text);
        }

        // Handle escape to unfocus
        if (input.IsKeyPressed(Key.Escape))
        {
            IsFocused = false;
        }

        // Handle character input — we check common printable keys
        TryTypeKey(input, Key.A, 'a', 'A');
        TryTypeKey(input, Key.B, 'b', 'B');
        TryTypeKey(input, Key.C, 'c', 'C');
        TryTypeKey(input, Key.D, 'd', 'D');
        TryTypeKey(input, Key.E, 'e', 'E');
        TryTypeKey(input, Key.F, 'f', 'F');
        TryTypeKey(input, Key.G, 'g', 'G');
        TryTypeKey(input, Key.H, 'h', 'H');
        TryTypeKey(input, Key.I, 'i', 'I');
        TryTypeKey(input, Key.J, 'j', 'J');
        TryTypeKey(input, Key.K, 'k', 'K');
        TryTypeKey(input, Key.L, 'l', 'L');
        TryTypeKey(input, Key.M, 'm', 'M');
        TryTypeKey(input, Key.N, 'n', 'N');
        TryTypeKey(input, Key.O, 'o', 'O');
        TryTypeKey(input, Key.P, 'p', 'P');
        TryTypeKey(input, Key.Q, 'q', 'Q');
        TryTypeKey(input, Key.R, 'r', 'R');
        TryTypeKey(input, Key.S, 's', 'S');
        TryTypeKey(input, Key.T, 't', 'T');
        TryTypeKey(input, Key.U, 'u', 'U');
        TryTypeKey(input, Key.V, 'v', 'V');
        TryTypeKey(input, Key.W, 'w', 'W');
        TryTypeKey(input, Key.X, 'x', 'X');
        TryTypeKey(input, Key.Y, 'y', 'Y');
        TryTypeKey(input, Key.Z, 'z', 'Z');
        TryTypeKey(input, Key.D0, '0', ')');
        TryTypeKey(input, Key.D1, '1', '!');
        TryTypeKey(input, Key.D2, '2', '@');
        TryTypeKey(input, Key.D3, '3', '#');
        TryTypeKey(input, Key.D4, '4', '$');
        TryTypeKey(input, Key.D5, '5', '%');
        TryTypeKey(input, Key.D6, '6', '^');
        TryTypeKey(input, Key.D7, '7', '&');
        TryTypeKey(input, Key.D8, '8', '*');
        TryTypeKey(input, Key.D9, '9', '(');
        TryTypeKey(input, Key.Space, ' ', ' ');
    }

    private void TryTypeKey(InputManager input, Key key, char lower, char upper)
    {
        if (!input.IsKeyPressed(key) || Text.Length >= MaxLength) return;

        bool shift = input.IsKeyDown(Key.LeftShift) || input.IsKeyDown(Key.RightShift);
        Text += shift ? upper : lower;
        OnTextChanged?.Invoke(Text);
    }

    public override void Draw(IRenderer renderer)
    {
        if (!Visible) return;

        renderer.DrawRect(Bounds, BackgroundColor);
        renderer.DrawRect(Bounds, IsFocused ? FocusedBorderColor : UnfocusedBorderColor, filled: false);

        float textX = Bounds.X + 6;
        float textY = Bounds.Y + (Bounds.Height - 7 * TextScale) / 2;

        if (string.IsNullOrEmpty(Text) && !IsFocused)
        {
            renderer.DrawText(Placeholder, new Vec2(textX, textY), PlaceholderColor, TextScale);
        }
        else
        {
            renderer.DrawText(Text, new Vec2(textX, textY), TextColor, TextScale);

            // Cursor
            if (IsFocused && _cursorBlink < 0.5f)
            {
                float cursorX = textX + Text.Length * 7 * TextScale;
                renderer.DrawLine(
                    new Vec2(cursorX, Bounds.Y + 4),
                    new Vec2(cursorX, Bounds.Bottom - 4),
                    CursorColor, 2f);
            }
        }
    }
}
