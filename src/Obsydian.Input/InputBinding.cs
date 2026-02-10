namespace Obsydian.Input;

/// <summary>
/// The type of physical input device a binding targets.
/// </summary>
public enum InputSource
{
    Key,
    MouseButton,
    GamepadButton
}

/// <summary>
/// Maps a physical input (key, mouse button, or gamepad button) to an activation mode.
/// </summary>
public readonly record struct InputBinding(
    InputSource Source,
    Key Key,
    MouseButton MouseButton,
    GamepadButton GamepadButton,
    ActivationMode Activation = ActivationMode.OnPress)
{
    public static InputBinding FromKey(Key key, ActivationMode activation = ActivationMode.OnPress) =>
        new(InputSource.Key, key, default, default, activation);

    public static InputBinding FromMouse(MouseButton button, ActivationMode activation = ActivationMode.OnPress) =>
        new(InputSource.MouseButton, default, button, default, activation);

    public static InputBinding FromGamepad(GamepadButton button, ActivationMode activation = ActivationMode.OnPress) =>
        new(InputSource.GamepadButton, default, default, button, activation);
}
