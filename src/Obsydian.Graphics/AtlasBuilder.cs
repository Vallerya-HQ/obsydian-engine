namespace Obsydian.Graphics;

/// <summary>
/// Content-pipeline helper for building texture atlases from image files.
/// Use during loading screens to batch-pack sprites into a single texture.
/// </summary>
public sealed class AtlasBuilder
{
    private readonly TextureAtlas _atlas;
    private readonly List<(string name, string filePath)> _images = [];

    public AtlasBuilder(int atlasWidth = 2048, int atlasHeight = 2048)
    {
        _atlas = new TextureAtlas(atlasWidth, atlasHeight);
    }

    /// <summary>
    /// Queue an image file to be included in the atlas.
    /// The actual loading happens during Build().
    /// </summary>
    public void AddImage(string name, string filePath)
    {
        _images.Add((name, filePath));
    }

    /// <summary>
    /// Load all queued images and pack them into an atlas.
    /// Requires an image loader function and a texture factory.
    /// </summary>
    public SpriteSheet Build(
        Func<string, (byte[] pixels, int width, int height)> loadImage,
        Func<byte[], int, int, Texture> createTexture)
    {
        foreach (var (name, filePath) in _images)
        {
            var (pixels, width, height) = loadImage(filePath);
            _atlas.AddRegion(name, width, height, pixels);
        }

        return _atlas.Build(createTexture);
    }
}
