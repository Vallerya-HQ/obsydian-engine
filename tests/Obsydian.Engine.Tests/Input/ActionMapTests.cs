using Obsydian.Input;

namespace Obsydian.Engine.Tests.Input;

public class ActionMapTests
{
    private static InputManager CreateInputWithKeyPress(Key key)
    {
        var input = new InputManager();
        input.BeginFrame();
        input.SetKeyDown(key);
        return input;
    }

    [Fact]
    public void RegisterAction_CanQueryBindings()
    {
        var input = new InputManager();
        input.Actions.RegisterAction("Movement", "MoveUp", InputBinding.FromKey(Key.W, ActivationMode.WhileHeld));

        var bindings = input.Actions.GetBindingsForAction("MoveUp");

        Assert.Single(bindings);
        Assert.Equal(Key.W, bindings[0].Key);
    }

    [Fact]
    public void IsActionPressed_DetectsKeyPress()
    {
        var input = CreateInputWithKeyPress(Key.Space);
        input.Actions.RegisterAction("Combat", "Attack", InputBinding.FromKey(Key.Space, ActivationMode.OnPress));

        Assert.True(input.Actions.IsActionPressed("Attack"));
    }

    [Fact]
    public void IsActionHeld_DetectsHeldKey()
    {
        var input = CreateInputWithKeyPress(Key.W);
        input.Actions.RegisterAction("Movement", "MoveUp", InputBinding.FromKey(Key.W, ActivationMode.WhileHeld));

        Assert.True(input.Actions.IsActionHeld("MoveUp"));
    }

    [Fact]
    public void IsActionReleased_DetectsKeyRelease()
    {
        var input = new InputManager();
        input.Actions.RegisterAction("Combat", "Attack", InputBinding.FromKey(Key.Space, ActivationMode.OnRelease));

        // Press and hold
        input.BeginFrame();
        input.SetKeyDown(Key.Space);
        Assert.False(input.Actions.IsActionReleased("Attack"));

        // Release
        input.BeginFrame();
        input.SetKeyUp(Key.Space);
        Assert.True(input.Actions.IsActionReleased("Attack"));
    }

    [Fact]
    public void DisableGroup_PreventsActionDetection()
    {
        var input = CreateInputWithKeyPress(Key.W);
        input.Actions.RegisterAction("Movement", "MoveUp", InputBinding.FromKey(Key.W, ActivationMode.WhileHeld));

        input.Actions.DisableGroup("Movement");

        Assert.False(input.Actions.IsActionHeld("MoveUp"));
    }

    [Fact]
    public void EnableGroup_RestoresActionDetection()
    {
        var input = CreateInputWithKeyPress(Key.W);
        input.Actions.RegisterAction("Movement", "MoveUp", InputBinding.FromKey(Key.W, ActivationMode.WhileHeld));

        input.Actions.DisableGroup("Movement");
        input.Actions.EnableGroup("Movement");

        Assert.True(input.Actions.IsActionHeld("MoveUp"));
    }

    [Fact]
    public void Rebind_ChangesBindings()
    {
        var input = CreateInputWithKeyPress(Key.Up);
        input.Actions.RegisterAction("Movement", "MoveUp",
            InputBinding.FromKey(Key.W, ActivationMode.WhileHeld));

        // W is pressed but Up isn't yet — action should not trigger
        Assert.False(input.Actions.IsActionHeld("MoveUp"));

        // Rebind to Up arrow
        input.Actions.Rebind("MoveUp", [InputBinding.FromKey(Key.Up, ActivationMode.WhileHeld)]);

        Assert.True(input.Actions.IsActionHeld("MoveUp"));
    }

    [Fact]
    public void Rebind_UnregisteredAction_Throws()
    {
        var input = new InputManager();

        Assert.Throws<KeyNotFoundException>(() =>
            input.Actions.Rebind("NonExistent", [InputBinding.FromKey(Key.A)]));
    }

    [Fact]
    public void ExportImport_PreservesBindings()
    {
        var input = new InputManager();
        input.Actions.RegisterAction("UI", "Confirm", InputBinding.FromKey(Key.Enter, ActivationMode.OnPress));
        input.Actions.RegisterAction("UI", "Cancel", InputBinding.FromKey(Key.Escape, ActivationMode.OnPress));

        var exported = input.Actions.ExportBindings();

        // Rebind to something else
        input.Actions.Rebind("Confirm", [InputBinding.FromKey(Key.Space, ActivationMode.OnPress)]);

        // Import original bindings
        input.Actions.ImportBindings(exported);

        var bindings = input.Actions.GetBindingsForAction("Confirm");
        Assert.Equal(Key.Enter, bindings[0].Key);
    }

    [Fact]
    public void MultipleBindings_AnyCanTrigger()
    {
        var input = CreateInputWithKeyPress(Key.Up);
        input.Actions.RegisterAction("Movement", "MoveUp",
            InputBinding.FromKey(Key.W, ActivationMode.WhileHeld),
            InputBinding.FromKey(Key.Up, ActivationMode.WhileHeld));

        Assert.True(input.Actions.IsActionHeld("MoveUp"));
    }

    [Fact]
    public void MouseBinding_Works()
    {
        var input = new InputManager();
        input.Actions.RegisterAction("Combat", "Attack",
            InputBinding.FromMouse(MouseButton.Left, ActivationMode.OnPress));

        input.BeginFrame();
        input.SetMouseDown(MouseButton.Left);

        Assert.True(input.Actions.IsActionPressed("Attack"));
    }

    [Fact]
    public void IsActionActive_ChecksActivationMode()
    {
        var input = CreateInputWithKeyPress(Key.Space);
        input.Actions.RegisterAction("Combat", "Attack",
            InputBinding.FromKey(Key.Space, ActivationMode.OnPress));

        Assert.True(input.Actions.IsActionActive("Attack"));
    }
}
