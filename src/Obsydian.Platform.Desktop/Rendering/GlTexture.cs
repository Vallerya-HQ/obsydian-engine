using Silk.NET.OpenGL;
using Texture = Obsydian.Graphics.Texture;

namespace Obsydian.Platform.Desktop.Rendering;

/// <summary>
/// Creates and manages OpenGL textures from RGBA pixel data.
/// Returns Obsydian Texture handles that reference the GL texture ID.
/// </summary>
public static class GlTexture
{
    /// <summary>
    /// Creates an OpenGL texture from raw RGBA byte data.
    /// Uses nearest-neighbor filtering (pixel-perfect for 2D).
    /// </summary>
    public static unsafe Texture Create(GL gl, int width, int height, byte[] rgba, string? name = null)
    {
        var handle = gl.GenTexture();
        gl.BindTexture(TextureTarget.Texture2D, handle);

        fixed (byte* ptr = rgba)
        {
            gl.TexImage2D(TextureTarget.Texture2D, 0, InternalFormat.Rgba8,
                (uint)width, (uint)height, 0,
                PixelFormat.Rgba, PixelType.UnsignedByte, ptr);
        }

        // Pixel-art friendly: nearest-neighbor, no mipmaps
        gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Nearest);
        gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Nearest);
        gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.ClampToEdge);
        gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.ClampToEdge);

        gl.BindTexture(TextureTarget.Texture2D, 0);

        return new Texture
        {
            Id = (int)handle,
            Width = width,
            Height = height,
            Name = name,
            OnDispose = id => gl.DeleteTexture((uint)id)
        };
    }

    /// <summary>
    /// Creates a 1x1 white texture (useful for drawing colored rects without a texture).
    /// </summary>
    public static Texture CreateWhitePixel(GL gl)
    {
        return Create(gl, 1, 1, [255, 255, 255, 255], "__white_pixel");
    }
}
