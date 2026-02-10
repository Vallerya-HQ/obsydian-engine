using Obsydian.Content;
using Obsydian.Core.Logging;
using Silk.NET.OpenGL;
using StbImageSharp;
using Texture = Obsydian.Graphics.Texture;

namespace Obsydian.Platform.Desktop.Rendering;

/// <summary>
/// Loads PNG/JPG images from disk into OpenGL textures via StbImageSharp.
/// Register with ContentManager to enable Load&lt;Texture&gt;("path").
/// </summary>
public sealed class GlTextureLoader : IAssetLoader<Texture>
{
    private readonly GL _gl;

    public GlTextureLoader(GL gl)
    {
        _gl = gl;
    }

    public Texture Load(string fullPath)
    {
        if (!File.Exists(fullPath))
            throw new FileNotFoundException($"Texture not found: {fullPath}");

        StbImage.stbi_set_flip_vertically_on_load(0);
        var result = ImageResult.FromMemory(File.ReadAllBytes(fullPath), ColorComponents.RedGreenBlueAlpha);

        Log.Debug("TextureLoader", $"Loaded image: {fullPath} ({result.Width}x{result.Height})");
        return GlTexture.Create(_gl, result.Width, result.Height, result.Data, Path.GetFileNameWithoutExtension(fullPath));
    }
}
