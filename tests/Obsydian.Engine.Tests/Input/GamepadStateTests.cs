using Obsydian.Core.Math;
using Obsydian.Input;

namespace Obsydian.Engine.Tests.Input;

public class GamepadStateTests
{
    [Fact]
    public void InitialState_NothingPressed()
    {
        var gp = new GamepadState();
        Assert.False(gp.IsButtonDown(GamepadButton.A));
        Assert.False(gp.IsButtonPressed(GamepadButton.A));
        Assert.Equal(Vec2.Zero, gp.LeftStick);
    }

    [Fact]
    public void SetButtonDown_TracksHeldAndPressed()
    {
        var gp = new GamepadState();
        gp.BeginFrame();
        gp.SetButtonDown(GamepadButton.A);

        Assert.True(gp.IsButtonDown(GamepadButton.A));
        Assert.True(gp.IsButtonPressed(GamepadButton.A));
    }

    [Fact]
    public void BeginFrame_ClearsPressedState()
    {
        var gp = new GamepadState();
        gp.SetButtonDown(GamepadButton.A);
        gp.BeginFrame();

        Assert.True(gp.IsButtonDown(GamepadButton.A)); // still held
        Assert.False(gp.IsButtonPressed(GamepadButton.A)); // press cleared
    }

    [Fact]
    public void SetButtonUp_TracksReleased()
    {
        var gp = new GamepadState();
        gp.SetButtonDown(GamepadButton.B);
        gp.BeginFrame();
        gp.SetButtonUp(GamepadButton.B);

        Assert.False(gp.IsButtonDown(GamepadButton.B));
        Assert.True(gp.IsButtonReleased(GamepadButton.B));
    }

    [Fact]
    public void Deadzone_FiltersSmallInput()
    {
        var gp = new GamepadState { Deadzone = 0.2f };
        gp.SetLeftStick(0.1f, 0.05f); // below deadzone

        Assert.Equal(Vec2.Zero, gp.LeftStick);
    }

    [Fact]
    public void Deadzone_PassesLargeInput()
    {
        var gp = new GamepadState { Deadzone = 0.1f };
        gp.SetLeftStick(0.5f, 0.5f);

        Assert.NotEqual(Vec2.Zero, gp.LeftStick);
    }

    [Fact]
    public void SetConnected_False_ClearsState()
    {
        var gp = new GamepadState();
        gp.SetButtonDown(GamepadButton.X);
        gp.SetLeftStick(1f, 0f);
        gp.SetConnected(false);

        Assert.False(gp.IsConnected);
        Assert.False(gp.IsButtonDown(GamepadButton.X));
        Assert.Equal(Vec2.Zero, gp.LeftStick);
    }

    [Fact]
    public void Triggers_SetAndRead()
    {
        var gp = new GamepadState();
        gp.SetTriggers(0.75f, 0.25f);

        Assert.Equal(0.75f, gp.LeftTrigger);
        Assert.Equal(0.25f, gp.RightTrigger);
    }

    [Fact]
    public void InputManager_Gamepad_IsAccessible()
    {
        var input = new InputManager();
        Assert.NotNull(input.Gamepad);
        Assert.False(input.Gamepad.IsConnected);
    }

    [Fact]
    public void InputManager_BeginFrame_ClearsGamepad()
    {
        var input = new InputManager();
        input.Gamepad.SetButtonDown(GamepadButton.A);
        input.BeginFrame();

        Assert.True(input.Gamepad.IsButtonDown(GamepadButton.A)); // still held
        Assert.False(input.Gamepad.IsButtonPressed(GamepadButton.A)); // press cleared
    }
}
