using Obsydian.Core.Math;

namespace Obsydian.Input;

public enum Key
{
    None, A, B, C, D, E, F, G, H, I, J, K, L, M, N, O, P, Q, R, S, T, U, V, W, X, Y, Z,
    D0, D1, D2, D3, D4, D5, D6, D7, D8, D9,
    Space, Enter, Escape, Tab, Backspace, Delete,
    Up, Down, Left, Right,
    LeftShift, RightShift, LeftControl, RightControl, LeftAlt, RightAlt,
    F1, F2, F3, F4, F5, F6, F7, F8, F9, F10, F11, F12
}

public enum MouseButton { Left, Right, Middle }

/// <summary>
/// Tracks keyboard and mouse input state each frame.
/// Uses event-buffered press/release detection that works with callback-driven input (Silk.NET/GLFW).
/// </summary>
public sealed class InputManager
{
    private readonly HashSet<Key> _heldKeys = [];
    private readonly HashSet<Key> _pressedThisFrame = [];
    private readonly HashSet<Key> _releasedThisFrame = [];

    private readonly HashSet<MouseButton> _heldMouse = [];
    private readonly HashSet<MouseButton> _pressedMouseThisFrame = [];
    private readonly HashSet<MouseButton> _releasedMouseThisFrame = [];

    public Vec2 MousePosition { get; private set; }
    public Vec2 MouseDelta { get; private set; }
    public float ScrollDelta { get; private set; }

    /// <summary>Gamepad state (first connected controller).</summary>
    public GamepadState Gamepad { get; } = new();

    public bool IsKeyDown(Key key) => _heldKeys.Contains(key);
    public bool IsKeyUp(Key key) => !_heldKeys.Contains(key);
    public bool IsKeyPressed(Key key) => _pressedThisFrame.Contains(key);
    public bool IsKeyReleased(Key key) => _releasedThisFrame.Contains(key);

    public bool IsMouseDown(MouseButton button) => _heldMouse.Contains(button);
    public bool IsMousePressed(MouseButton button) => _pressedMouseThisFrame.Contains(button);
    public bool IsMouseReleased(MouseButton button) => _releasedMouseThisFrame.Contains(button);

    /// <summary>
    /// Call at the start of each frame to clear per-frame press/release buffers.
    /// Input events that arrive after this call (from platform polling) will be
    /// detected as pressed/released for the current frame.
    /// </summary>
    public void BeginFrame()
    {
        _pressedThisFrame.Clear();
        _releasedThisFrame.Clear();
        _pressedMouseThisFrame.Clear();
        _releasedMouseThisFrame.Clear();
        MouseDelta = Vec2.Zero;
        ScrollDelta = 0;
        Gamepad.BeginFrame();
    }

    // Called by the platform layer (SilkInputBridge) as events arrive
    public void SetKeyDown(Key key)
    {
        if (_heldKeys.Add(key))
            _pressedThisFrame.Add(key);
    }

    public void SetKeyUp(Key key)
    {
        if (_heldKeys.Remove(key))
            _releasedThisFrame.Add(key);
    }

    public void SetMouseDown(MouseButton button)
    {
        if (_heldMouse.Add(button))
            _pressedMouseThisFrame.Add(button);
    }

    public void SetMouseUp(MouseButton button)
    {
        if (_heldMouse.Remove(button))
            _releasedMouseThisFrame.Add(button);
    }

    public void SetMousePosition(Vec2 position)
    {
        MouseDelta = position - MousePosition;
        MousePosition = position;
    }

    public void SetScrollDelta(float delta) => ScrollDelta = delta;
}
