namespace Obsydian.Input;

/// <summary>
/// Maps named actions to physical input bindings. Supports rebinding, group enable/disable,
/// and serialization for save/load. Inspired by CryEngine's ActionMap system.
/// </summary>
public sealed class ActionMap
{
    private readonly Dictionary<string, InputAction> _actions = [];
    private readonly Dictionary<string, List<InputBinding>> _bindings = [];
    private readonly HashSet<string> _disabledGroups = [];
    private InputManager? _input;

    /// <summary>
    /// Attach this ActionMap to an InputManager for state queries.
    /// </summary>
    public void Attach(InputManager input) => _input = input;

    public void RegisterAction(string group, string name, params InputBinding[] defaults)
    {
        var action = new InputAction(name, group);
        _actions[name] = action;
        _bindings[name] = [..defaults];
    }

    public void Rebind(string actionName, InputBinding[] newBindings)
    {
        if (!_actions.ContainsKey(actionName))
            throw new KeyNotFoundException($"Action '{actionName}' not registered.");

        _bindings[actionName] = [..newBindings];
    }

    public void EnableGroup(string group) => _disabledGroups.Remove(group);

    public void DisableGroup(string group) => _disabledGroups.Add(group);

    public bool IsGroupEnabled(string group) => !_disabledGroups.Contains(group);

    public bool IsActionActive(string name)
    {
        if (_input is null) return false;
        if (!_actions.TryGetValue(name, out var action)) return false;
        if (_disabledGroups.Contains(action.Group)) return false;

        if (!_bindings.TryGetValue(name, out var bindings)) return false;

        foreach (var binding in bindings)
        {
            if (IsBindingActive(binding))
                return true;
        }
        return false;
    }

    public bool IsActionPressed(string name) => CheckBinding(name, ActivationMode.OnPress);
    public bool IsActionReleased(string name) => CheckBinding(name, ActivationMode.OnRelease);
    public bool IsActionHeld(string name) => CheckBinding(name, ActivationMode.WhileHeld);

    public IReadOnlyList<InputBinding> GetBindingsForAction(string name)
    {
        return _bindings.TryGetValue(name, out var bindings) ? bindings : [];
    }

    public IReadOnlyDictionary<string, InputAction> Actions => _actions;

    /// <summary>
    /// Export all bindings as a simple dictionary for serialization.
    /// </summary>
    public Dictionary<string, List<InputBinding>> ExportBindings()
    {
        var result = new Dictionary<string, List<InputBinding>>();
        foreach (var (name, bindings) in _bindings)
            result[name] = [..bindings];
        return result;
    }

    /// <summary>
    /// Import bindings from a saved dictionary. Only updates actions that are registered.
    /// </summary>
    public void ImportBindings(Dictionary<string, List<InputBinding>> saved)
    {
        foreach (var (name, bindings) in saved)
        {
            if (_actions.ContainsKey(name))
                _bindings[name] = [..bindings];
        }
    }

    private bool CheckBinding(string name, ActivationMode mode)
    {
        if (_input is null) return false;
        if (!_actions.TryGetValue(name, out var action)) return false;
        if (_disabledGroups.Contains(action.Group)) return false;

        if (!_bindings.TryGetValue(name, out var bindings)) return false;

        foreach (var binding in bindings)
        {
            if (binding.Activation != mode) continue;
            if (IsBindingActive(binding))
                return true;
        }
        return false;
    }

    private bool IsBindingActive(InputBinding binding)
    {
        if (_input is null) return false;

        return binding.Source switch
        {
            InputSource.Key => binding.Activation switch
            {
                ActivationMode.OnPress => _input.IsKeyPressed(binding.Key),
                ActivationMode.OnRelease => _input.IsKeyReleased(binding.Key),
                ActivationMode.WhileHeld => _input.IsKeyDown(binding.Key),
                _ => false
            },
            InputSource.MouseButton => binding.Activation switch
            {
                ActivationMode.OnPress => _input.IsMousePressed(binding.MouseButton),
                ActivationMode.OnRelease => _input.IsMouseReleased(binding.MouseButton),
                ActivationMode.WhileHeld => _input.IsMouseDown(binding.MouseButton),
                _ => false
            },
            InputSource.GamepadButton => binding.Activation switch
            {
                ActivationMode.OnPress => _input.Gamepad.IsButtonPressed(binding.GamepadButton),
                ActivationMode.OnRelease => _input.Gamepad.IsButtonReleased(binding.GamepadButton),
                ActivationMode.WhileHeld => _input.Gamepad.IsButtonDown(binding.GamepadButton),
                _ => false
            },
            _ => false
        };
    }
}
