namespace Obsydian.Input;

/// <summary>
/// A named input action with a group for enable/disable filtering.
/// </summary>
public sealed record InputAction(string Name, string Group);

/// <summary>
/// When an input binding should trigger its action.
/// </summary>
public enum ActivationMode
{
    OnPress,
    OnRelease,
    WhileHeld
}
