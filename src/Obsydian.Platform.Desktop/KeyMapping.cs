using SilkKey = Silk.NET.Input.Key;
using ObsKey = Obsydian.Input.Key;

namespace Obsydian.Platform.Desktop;

/// <summary>
/// Maps Silk.NET keyboard keys to Obsydian engine keys.
/// </summary>
public static class KeyMapping
{
    private static readonly Dictionary<SilkKey, ObsKey> Map = new()
    {
        // Letters
        [SilkKey.A] = ObsKey.A, [SilkKey.B] = ObsKey.B, [SilkKey.C] = ObsKey.C,
        [SilkKey.D] = ObsKey.D, [SilkKey.E] = ObsKey.E, [SilkKey.F] = ObsKey.F,
        [SilkKey.G] = ObsKey.G, [SilkKey.H] = ObsKey.H, [SilkKey.I] = ObsKey.I,
        [SilkKey.J] = ObsKey.J, [SilkKey.K] = ObsKey.K, [SilkKey.L] = ObsKey.L,
        [SilkKey.M] = ObsKey.M, [SilkKey.N] = ObsKey.N, [SilkKey.O] = ObsKey.O,
        [SilkKey.P] = ObsKey.P, [SilkKey.Q] = ObsKey.Q, [SilkKey.R] = ObsKey.R,
        [SilkKey.S] = ObsKey.S, [SilkKey.T] = ObsKey.T, [SilkKey.U] = ObsKey.U,
        [SilkKey.V] = ObsKey.V, [SilkKey.W] = ObsKey.W, [SilkKey.X] = ObsKey.X,
        [SilkKey.Y] = ObsKey.Y, [SilkKey.Z] = ObsKey.Z,

        // Digits
        [SilkKey.Number0] = ObsKey.D0, [SilkKey.Number1] = ObsKey.D1,
        [SilkKey.Number2] = ObsKey.D2, [SilkKey.Number3] = ObsKey.D3,
        [SilkKey.Number4] = ObsKey.D4, [SilkKey.Number5] = ObsKey.D5,
        [SilkKey.Number6] = ObsKey.D6, [SilkKey.Number7] = ObsKey.D7,
        [SilkKey.Number8] = ObsKey.D8, [SilkKey.Number9] = ObsKey.D9,

        // Special
        [SilkKey.Space] = ObsKey.Space, [SilkKey.Enter] = ObsKey.Enter,
        [SilkKey.Escape] = ObsKey.Escape, [SilkKey.Tab] = ObsKey.Tab,
        [SilkKey.Backspace] = ObsKey.Backspace, [SilkKey.Delete] = ObsKey.Delete,

        // Arrows
        [SilkKey.Up] = ObsKey.Up, [SilkKey.Down] = ObsKey.Down,
        [SilkKey.Left] = ObsKey.Left, [SilkKey.Right] = ObsKey.Right,

        // Modifiers
        [SilkKey.ShiftLeft] = ObsKey.LeftShift, [SilkKey.ShiftRight] = ObsKey.RightShift,
        [SilkKey.ControlLeft] = ObsKey.LeftControl, [SilkKey.ControlRight] = ObsKey.RightControl,
        [SilkKey.AltLeft] = ObsKey.LeftAlt, [SilkKey.AltRight] = ObsKey.RightAlt,

        // Function keys
        [SilkKey.F1] = ObsKey.F1, [SilkKey.F2] = ObsKey.F2, [SilkKey.F3] = ObsKey.F3,
        [SilkKey.F4] = ObsKey.F4, [SilkKey.F5] = ObsKey.F5, [SilkKey.F6] = ObsKey.F6,
        [SilkKey.F7] = ObsKey.F7, [SilkKey.F8] = ObsKey.F8, [SilkKey.F9] = ObsKey.F9,
        [SilkKey.F10] = ObsKey.F10, [SilkKey.F11] = ObsKey.F11, [SilkKey.F12] = ObsKey.F12,
    };

    public static ObsKey ToObsydian(SilkKey silkKey)
    {
        return Map.GetValueOrDefault(silkKey, ObsKey.None);
    }

    /// <summary>Returns all mapped Silk.NET keys (for testing coverage).</summary>
    public static IReadOnlyDictionary<SilkKey, ObsKey> All => Map;
}
