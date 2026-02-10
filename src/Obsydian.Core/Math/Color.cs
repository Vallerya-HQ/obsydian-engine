namespace Obsydian.Core.Math;

/// <summary>
/// RGBA color with byte precision per channel.
/// </summary>
public readonly record struct Color(byte R, byte G, byte B, byte A = 255)
{
    public static readonly Color White = new(255, 255, 255);
    public static readonly Color Black = new(0, 0, 0);
    public static readonly Color Red = new(255, 0, 0);
    public static readonly Color Green = new(0, 255, 0);
    public static readonly Color Blue = new(0, 0, 255);
    public static readonly Color Transparent = new(0, 0, 0, 0);
    public static readonly Color CornflowerBlue = new(100, 149, 237);
    public static readonly Color Gold = new(255, 215, 0);
    public static readonly Color Magenta = new(255, 0, 255);

    public uint PackedValue => (uint)(R | (G << 8) | (B << 16) | (A << 24));

    public static Color FromHex(string hex)
    {
        hex = hex.TrimStart('#');
        return hex.Length switch
        {
            6 => new Color(
                Convert.ToByte(hex[..2], 16),
                Convert.ToByte(hex[2..4], 16),
                Convert.ToByte(hex[4..6], 16)),
            8 => new Color(
                Convert.ToByte(hex[..2], 16),
                Convert.ToByte(hex[2..4], 16),
                Convert.ToByte(hex[4..6], 16),
                Convert.ToByte(hex[6..8], 16)),
            _ => throw new ArgumentException($"Invalid hex color: #{hex}")
        };
    }

    public static Color Lerp(Color a, Color b, float t) => new(
        (byte)(a.R + (b.R - a.R) * t),
        (byte)(a.G + (b.G - a.G) * t),
        (byte)(a.B + (b.B - a.B) * t),
        (byte)(a.A + (b.A - a.A) * t));

    /// <summary>Return a copy with modified alpha (0-255).</summary>
    public Color WithAlpha(byte alpha) => new(R, G, B, alpha);

    public override string ToString() => $"#{R:X2}{G:X2}{B:X2}{(A < 255 ? A.ToString("X2") : "")}";
}
