using Obsydian.Core.Logging;
using Obsydian.Input;
using Silk.NET.Input;

namespace Obsydian.Platform.Desktop.Input;

/// <summary>
/// Bridges Silk.NET gamepad input to the engine's GamepadState.
/// Polls connected gamepads each frame for button/stick/trigger state.
/// </summary>
public sealed class SilkGamepadBridge
{
    private readonly GamepadState _state;
    private IGamepad? _gamepad;

    public SilkGamepadBridge(GamepadState state)
    {
        _state = state;
    }

    /// <summary>
    /// Bind to the first available gamepad from the input context.
    /// Call after Silk.NET window is created.
    /// </summary>
    public void Initialize(IInputContext inputContext)
    {
        inputContext.ConnectionChanged += OnConnectionChanged;
        BindFirstGamepad(inputContext);
    }

    /// <summary>
    /// Poll gamepad state. Call once per frame before game update.
    /// </summary>
    public void Update()
    {
        if (_gamepad is null || !_state.IsConnected) return;

        // Buttons
        foreach (var button in _gamepad.Buttons)
        {
            var mapped = MapButton(button.Name);
            if (mapped is null) continue;

            if (button.Pressed)
                _state.SetButtonDown(mapped.Value);
            else
                _state.SetButtonUp(mapped.Value);
        }

        // Thumbsticks
        if (_gamepad.Thumbsticks.Count >= 1)
        {
            var left = _gamepad.Thumbsticks[0];
            _state.SetLeftStick(left.X, left.Y);
        }
        if (_gamepad.Thumbsticks.Count >= 2)
        {
            var right = _gamepad.Thumbsticks[1];
            _state.SetRightStick(right.X, right.Y);
        }

        // Triggers
        if (_gamepad.Triggers.Count >= 2)
            _state.SetTriggers(_gamepad.Triggers[0].Position, _gamepad.Triggers[1].Position);
    }

    private void BindFirstGamepad(IInputContext context)
    {
        _gamepad = context.Gamepads.Count > 0 ? context.Gamepads[0] : null;
        _state.SetConnected(_gamepad is not null);

        if (_gamepad is not null)
            Log.Info("Input", $"Gamepad connected: {_gamepad.Name}");
    }

    private void OnConnectionChanged(IInputDevice device, bool connected)
    {
        if (device is IGamepad gp)
        {
            if (connected)
            {
                _gamepad = gp;
                _state.SetConnected(true);
                Log.Info("Input", $"Gamepad connected: {gp.Name}");
            }
            else if (_gamepad == gp)
            {
                _gamepad = null;
                _state.SetConnected(false);
                Log.Info("Input", "Gamepad disconnected.");
            }
        }
    }

    private static GamepadButton? MapButton(ButtonName name) => name switch
    {
        ButtonName.A => GamepadButton.A,
        ButtonName.B => GamepadButton.B,
        ButtonName.X => GamepadButton.X,
        ButtonName.Y => GamepadButton.Y,
        ButtonName.LeftBumper => GamepadButton.LeftBumper,
        ButtonName.RightBumper => GamepadButton.RightBumper,
        ButtonName.Back => GamepadButton.Back,
        ButtonName.Start => GamepadButton.Start,
        ButtonName.Home => GamepadButton.Guide,
        ButtonName.LeftStick => GamepadButton.LeftStick,
        ButtonName.RightStick => GamepadButton.RightStick,
        ButtonName.DPadUp => GamepadButton.DPadUp,
        ButtonName.DPadDown => GamepadButton.DPadDown,
        ButtonName.DPadLeft => GamepadButton.DPadLeft,
        ButtonName.DPadRight => GamepadButton.DPadRight,
        _ => null
    };
}
