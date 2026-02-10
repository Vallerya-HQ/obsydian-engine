using Obsydian.Core.Math;

namespace Obsydian.Graphics;

/// <summary>
/// A packed region within a texture atlas.
/// </summary>
public readonly record struct AtlasRegion(string Name, Rect SourceRect);

/// <summary>
/// Runtime texture atlas packer using shelf/row packing.
/// Add regions, then Build() to produce a combined SpriteSheet.
/// </summary>
public sealed class TextureAtlas
{
    private readonly List<PendingRegion> _pending = [];
    private readonly int _atlasWidth;
    private readonly int _atlasHeight;

    public TextureAtlas(int width = 2048, int height = 2048)
    {
        _atlasWidth = width;
        _atlasHeight = height;
    }

    /// <summary>
    /// Add a named region with pixel data (RGBA, 4 bytes per pixel).
    /// </summary>
    public void AddRegion(string name, int width, int height, byte[] pixels)
    {
        _pending.Add(new PendingRegion(name, width, height, pixels));
    }

    /// <summary>
    /// Pack all regions into a single atlas texture. Returns the combined pixel data
    /// and a list of named regions with their source rectangles.
    /// The caller is responsible for uploading the pixel data to GPU.
    /// </summary>
    public AtlasBuildResult Pack()
    {
        // Sort by height descending for better shelf packing
        var sorted = _pending.OrderByDescending(r => r.Height).ThenByDescending(r => r.Width).ToList();

        var atlasPixels = new byte[_atlasWidth * _atlasHeight * 4];
        var regions = new List<AtlasRegion>();

        int shelfX = 0;
        int shelfY = 0;
        int shelfHeight = 0;

        foreach (var region in sorted)
        {
            // Check if region fits on current shelf
            if (shelfX + region.Width > _atlasWidth)
            {
                // Move to next shelf
                shelfY += shelfHeight;
                shelfX = 0;
                shelfHeight = 0;
            }

            if (shelfY + region.Height > _atlasHeight)
                throw new InvalidOperationException(
                    $"Atlas overflow: region '{region.Name}' ({region.Width}x{region.Height}) doesn't fit in {_atlasWidth}x{_atlasHeight} atlas.");

            // Copy pixels into atlas
            for (int row = 0; row < region.Height; row++)
            {
                var srcOffset = row * region.Width * 4;
                var dstOffset = ((shelfY + row) * _atlasWidth + shelfX) * 4;
                Array.Copy(region.Pixels, srcOffset, atlasPixels, dstOffset, region.Width * 4);
            }

            regions.Add(new AtlasRegion(region.Name,
                new Rect(shelfX, shelfY, region.Width, region.Height)));

            shelfX += region.Width;
            shelfHeight = System.Math.Max(shelfHeight, region.Height);
        }

        return new AtlasBuildResult(atlasPixels, _atlasWidth, _atlasHeight, regions);
    }

    /// <summary>
    /// Pack and create a SpriteSheet from the result. Requires a texture factory
    /// to upload the atlas pixel data to GPU.
    /// </summary>
    public SpriteSheet Build(Func<byte[], int, int, Texture> createTexture)
    {
        var result = Pack();
        var texture = createTexture(result.Pixels, result.Width, result.Height);
        var sheet = new SpriteSheet(texture, 1, 1);

        foreach (var region in result.Regions)
            sheet.DefineRegion(region.Name, region.SourceRect);

        return sheet;
    }

    private sealed record PendingRegion(string Name, int Width, int Height, byte[] Pixels);
}

/// <summary>
/// Result of packing a TextureAtlas: combined pixel data and region list.
/// </summary>
public sealed record AtlasBuildResult(
    byte[] Pixels,
    int Width,
    int Height,
    IReadOnlyList<AtlasRegion> Regions);
