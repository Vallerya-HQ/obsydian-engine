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
/// Supports "just pressed", "held", and "just released" queries.
/// </summary>
public sealed class InputManager
{
    private readonly HashSet<Key> _currentKeys = [];
    private readonly HashSet<Key> _previousKeys = [];
    private readonly HashSet<MouseButton> _currentMouse = [];
    private readonly HashSet<MouseButton> _previousMouse = [];

    public Vec2 MousePosition { get; private set; }
    public Vec2 MouseDelta { get; private set; }
    public float ScrollDelta { get; private set; }

    public bool IsKeyDown(Key key) => _currentKeys.Contains(key);
    public bool IsKeyUp(Key key) => !_currentKeys.Contains(key);
    public bool IsKeyPressed(Key key) => _currentKeys.Contains(key) && !_previousKeys.Contains(key);
    public bool IsKeyReleased(Key key) => !_currentKeys.Contains(key) && _previousKeys.Contains(key);

    public bool IsMouseDown(MouseButton button) => _currentMouse.Contains(button);
    public bool IsMousePressed(MouseButton button) => _currentMouse.Contains(button) && !_previousMouse.Contains(button);
    public bool IsMouseReleased(MouseButton button) => !_currentMouse.Contains(button) && _previousMouse.Contains(button);

    /// <summary>
    /// Called at the start of each frame to snapshot the previous state.
    /// </summary>
    public void BeginFrame()
    {
        _previousKeys.Clear();
        foreach (var key in _currentKeys)
            _previousKeys.Add(key);

        _previousMouse.Clear();
        foreach (var button in _currentMouse)
            _previousMouse.Add(button);

        MouseDelta = Vec2.Zero;
        ScrollDelta = 0;
    }

    // These are called by the platform layer to feed raw input events
    public void SetKeyDown(Key key) => _currentKeys.Add(key);
    public void SetKeyUp(Key key) => _currentKeys.Remove(key);
    public void SetMouseDown(MouseButton button) => _currentMouse.Add(button);
    public void SetMouseUp(MouseButton button) => _currentMouse.Remove(button);
    public void SetMousePosition(Vec2 position)
    {
        MouseDelta = position - MousePosition;
        MousePosition = position;
    }
    public void SetScrollDelta(float delta) => ScrollDelta = delta;
}
