using Obsydian.Platform.Desktop;
using SilkKey = Silk.NET.Input.Key;
using ObsKey = Obsydian.Input.Key;

namespace Obsydian.Platform.Desktop.Tests;

public class KeyMappingTests
{
    [Fact]
    public void All_Letters_Are_Mapped()
    {
        var letters = new[]
        {
            SilkKey.A, SilkKey.B, SilkKey.C, SilkKey.D, SilkKey.E, SilkKey.F,
            SilkKey.G, SilkKey.H, SilkKey.I, SilkKey.J, SilkKey.K, SilkKey.L,
            SilkKey.M, SilkKey.N, SilkKey.O, SilkKey.P, SilkKey.Q, SilkKey.R,
            SilkKey.S, SilkKey.T, SilkKey.U, SilkKey.V, SilkKey.W, SilkKey.X,
            SilkKey.Y, SilkKey.Z
        };

        foreach (var letter in letters)
        {
            var mapped = KeyMapping.ToObsydian(letter);
            Assert.NotEqual(ObsKey.None, mapped);
        }
    }

    [Fact]
    public void All_Digits_Are_Mapped()
    {
        var digits = new[]
        {
            SilkKey.Number0, SilkKey.Number1, SilkKey.Number2, SilkKey.Number3,
            SilkKey.Number4, SilkKey.Number5, SilkKey.Number6, SilkKey.Number7,
            SilkKey.Number8, SilkKey.Number9
        };

        foreach (var digit in digits)
        {
            var mapped = KeyMapping.ToObsydian(digit);
            Assert.NotEqual(ObsKey.None, mapped);
        }
    }

    [Fact]
    public void Arrow_Keys_Are_Mapped()
    {
        Assert.Equal(ObsKey.Up, KeyMapping.ToObsydian(SilkKey.Up));
        Assert.Equal(ObsKey.Down, KeyMapping.ToObsydian(SilkKey.Down));
        Assert.Equal(ObsKey.Left, KeyMapping.ToObsydian(SilkKey.Left));
        Assert.Equal(ObsKey.Right, KeyMapping.ToObsydian(SilkKey.Right));
    }

    [Fact]
    public void Special_Keys_Are_Mapped()
    {
        Assert.Equal(ObsKey.Space, KeyMapping.ToObsydian(SilkKey.Space));
        Assert.Equal(ObsKey.Enter, KeyMapping.ToObsydian(SilkKey.Enter));
        Assert.Equal(ObsKey.Escape, KeyMapping.ToObsydian(SilkKey.Escape));
        Assert.Equal(ObsKey.Tab, KeyMapping.ToObsydian(SilkKey.Tab));
        Assert.Equal(ObsKey.Backspace, KeyMapping.ToObsydian(SilkKey.Backspace));
        Assert.Equal(ObsKey.Delete, KeyMapping.ToObsydian(SilkKey.Delete));
    }

    [Fact]
    public void Modifier_Keys_Are_Mapped()
    {
        Assert.Equal(ObsKey.LeftShift, KeyMapping.ToObsydian(SilkKey.ShiftLeft));
        Assert.Equal(ObsKey.RightShift, KeyMapping.ToObsydian(SilkKey.ShiftRight));
        Assert.Equal(ObsKey.LeftControl, KeyMapping.ToObsydian(SilkKey.ControlLeft));
        Assert.Equal(ObsKey.RightControl, KeyMapping.ToObsydian(SilkKey.ControlRight));
        Assert.Equal(ObsKey.LeftAlt, KeyMapping.ToObsydian(SilkKey.AltLeft));
        Assert.Equal(ObsKey.RightAlt, KeyMapping.ToObsydian(SilkKey.AltRight));
    }

    [Fact]
    public void Function_Keys_Are_Mapped()
    {
        Assert.Equal(ObsKey.F1, KeyMapping.ToObsydian(SilkKey.F1));
        Assert.Equal(ObsKey.F6, KeyMapping.ToObsydian(SilkKey.F6));
        Assert.Equal(ObsKey.F12, KeyMapping.ToObsydian(SilkKey.F12));
    }

    [Fact]
    public void Unknown_Key_Returns_None()
    {
        // Silk.NET has keys we don't map (like numpad)
        Assert.Equal(ObsKey.None, KeyMapping.ToObsydian(SilkKey.Keypad0));
    }

    [Fact]
    public void Mapping_Is_Complete_For_All_ObsydianKeys()
    {
        // Every non-None Obsydian key should appear in the mapping values
        var mappedObsKeys = KeyMapping.All.Values.ToHashSet();
        var allObsKeys = Enum.GetValues<ObsKey>().Where(k => k != ObsKey.None);

        foreach (var key in allObsKeys)
        {
            Assert.Contains(key, mappedObsKeys);
        }
    }
}
