using Obsydian.Core.Math;

namespace Obsydian.Input;

/// <summary>
/// Gamepad button identifiers matching common controller layouts.
/// </summary>
public enum GamepadButton
{
    A, B, X, Y,
    LeftBumper, RightBumper,
    Back, Start, Guide,
    LeftStick, RightStick,
    DPadUp, DPadDown, DPadLeft, DPadRight
}

/// <summary>
/// Tracks gamepad input state each frame. Supports one gamepad.
/// Platform layer (SilkGamepadBridge) feeds events into this state.
/// </summary>
public sealed class GamepadState
{
    private readonly HashSet<GamepadButton> _held = [];
    private readonly HashSet<GamepadButton> _pressedThisFrame = [];
    private readonly HashSet<GamepadButton> _releasedThisFrame = [];

    public Vec2 LeftStick { get; private set; }
    public Vec2 RightStick { get; private set; }
    public float LeftTrigger { get; private set; }
    public float RightTrigger { get; private set; }
    public bool IsConnected { get; private set; }

    /// <summary>Deadzone threshold for analog sticks.</summary>
    public float Deadzone { get; set; } = 0.15f;

    public bool IsButtonDown(GamepadButton button) => _held.Contains(button);
    public bool IsButtonPressed(GamepadButton button) => _pressedThisFrame.Contains(button);
    public bool IsButtonReleased(GamepadButton button) => _releasedThisFrame.Contains(button);

    public void BeginFrame()
    {
        _pressedThisFrame.Clear();
        _releasedThisFrame.Clear();
    }

    public void SetButtonDown(GamepadButton button)
    {
        if (_held.Add(button))
            _pressedThisFrame.Add(button);
    }

    public void SetButtonUp(GamepadButton button)
    {
        if (_held.Remove(button))
            _releasedThisFrame.Add(button);
    }

    public void SetLeftStick(float x, float y)
    {
        LeftStick = ApplyDeadzone(x, y);
    }

    public void SetRightStick(float x, float y)
    {
        RightStick = ApplyDeadzone(x, y);
    }

    public void SetTriggers(float left, float right)
    {
        LeftTrigger = left;
        RightTrigger = right;
    }

    public void SetConnected(bool connected)
    {
        IsConnected = connected;
        if (!connected)
        {
            _held.Clear();
            LeftStick = Vec2.Zero;
            RightStick = Vec2.Zero;
            LeftTrigger = 0;
            RightTrigger = 0;
        }
    }

    private Vec2 ApplyDeadzone(float x, float y)
    {
        var vec = new Vec2(x, y);
        return vec.Length < Deadzone ? Vec2.Zero : vec;
    }
}
