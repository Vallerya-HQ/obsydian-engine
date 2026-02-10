using Obsydian.Core.Math;
using Obsydian.Input;
using Silk.NET.Input;

namespace Obsydian.Platform.Desktop;

/// <summary>
/// Bridges Silk.NET input events into the Obsydian InputManager.
/// Call Connect() once the Silk.NET window is loaded and input context is available.
/// </summary>
public sealed class SilkInputBridge
{
    private readonly InputManager _input;

    public SilkInputBridge(InputManager input)
    {
        _input = input;
    }

    public void Connect(IInputContext inputContext)
    {
        foreach (var keyboard in inputContext.Keyboards)
        {
            keyboard.KeyDown += OnKeyDown;
            keyboard.KeyUp += OnKeyUp;
        }

        foreach (var mouse in inputContext.Mice)
        {
            mouse.MouseDown += OnMouseDown;
            mouse.MouseUp += OnMouseUp;
            mouse.MouseMove += OnMouseMove;
            mouse.Scroll += OnScroll;
        }
    }

    private void OnKeyDown(IKeyboard keyboard, Silk.NET.Input.Key key, int scanCode)
    {
        var mapped = KeyMapping.ToObsydian(key);
        if (mapped != Obsydian.Input.Key.None)
            _input.SetKeyDown(mapped);
    }

    private void OnKeyUp(IKeyboard keyboard, Silk.NET.Input.Key key, int scanCode)
    {
        var mapped = KeyMapping.ToObsydian(key);
        if (mapped != Obsydian.Input.Key.None)
            _input.SetKeyUp(mapped);
    }

    private void OnMouseDown(IMouse mouse, Silk.NET.Input.MouseButton button)
    {
        var mapped = MapMouseButton(button);
        if (mapped.HasValue)
            _input.SetMouseDown(mapped.Value);
    }

    private void OnMouseUp(IMouse mouse, Silk.NET.Input.MouseButton button)
    {
        var mapped = MapMouseButton(button);
        if (mapped.HasValue)
            _input.SetMouseUp(mapped.Value);
    }

    private void OnMouseMove(IMouse mouse, System.Numerics.Vector2 position)
    {
        _input.SetMousePosition(new Vec2(position.X, position.Y));
    }

    private void OnScroll(IMouse mouse, ScrollWheel wheel)
    {
        _input.SetScrollDelta(wheel.Y);
    }

    private static Obsydian.Input.MouseButton? MapMouseButton(Silk.NET.Input.MouseButton button) => button switch
    {
        Silk.NET.Input.MouseButton.Left => Obsydian.Input.MouseButton.Left,
        Silk.NET.Input.MouseButton.Right => Obsydian.Input.MouseButton.Right,
        Silk.NET.Input.MouseButton.Middle => Obsydian.Input.MouseButton.Middle,
        _ => null
    };
}
