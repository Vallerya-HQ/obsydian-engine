using Obsydian.Graphics;

namespace Obsydian.Engine.Tests.Graphics;

public class TextureAtlasTests
{
    [Fact]
    public void Pack_SingleRegion_CorrectPlacement()
    {
        var atlas = new TextureAtlas(256, 256);
        var pixels = new byte[32 * 32 * 4];
        atlas.AddRegion("test", 32, 32, pixels);

        var result = atlas.Pack();

        Assert.Single(result.Regions);
        Assert.Equal("test", result.Regions[0].Name);
        Assert.Equal(0, result.Regions[0].SourceRect.X);
        Assert.Equal(0, result.Regions[0].SourceRect.Y);
        Assert.Equal(32, result.Regions[0].SourceRect.Width);
        Assert.Equal(32, result.Regions[0].SourceRect.Height);
    }

    [Fact]
    public void Pack_MultipleRegions_NoOverlap()
    {
        var atlas = new TextureAtlas(256, 256);
        atlas.AddRegion("a", 64, 64, new byte[64 * 64 * 4]);
        atlas.AddRegion("b", 64, 64, new byte[64 * 64 * 4]);
        atlas.AddRegion("c", 64, 64, new byte[64 * 64 * 4]);

        var result = atlas.Pack();

        Assert.Equal(3, result.Regions.Count);

        // Verify no regions overlap
        for (int i = 0; i < result.Regions.Count; i++)
        {
            for (int j = i + 1; j < result.Regions.Count; j++)
            {
                Assert.False(result.Regions[i].SourceRect.Intersects(result.Regions[j].SourceRect),
                    $"Regions {result.Regions[i].Name} and {result.Regions[j].Name} overlap");
            }
        }
    }

    [Fact]
    public void Pack_Overflow_Throws()
    {
        var atlas = new TextureAtlas(32, 32);
        atlas.AddRegion("too_big", 64, 64, new byte[64 * 64 * 4]);

        Assert.Throws<InvalidOperationException>(() => atlas.Pack());
    }

    [Fact]
    public void Build_ProducesSpriteSheet()
    {
        var atlas = new TextureAtlas(256, 256);
        atlas.AddRegion("player", 32, 32, new byte[32 * 32 * 4]);
        atlas.AddRegion("enemy", 16, 16, new byte[16 * 16 * 4]);

        var sheet = atlas.Build((pixels, w, h) => new Texture { Id = 1, Width = w, Height = h });

        Assert.NotNull(sheet);
        var playerRect = sheet.GetRegion("player");
        Assert.Equal(32, playerRect.Width);
    }

    [Fact]
    public void Pack_CopiesPixelData()
    {
        var atlas = new TextureAtlas(64, 64);
        var pixels = new byte[4 * 4 * 4]; // 4x4 image, RGBA
        pixels[0] = 255; // Red channel of first pixel

        atlas.AddRegion("test", 4, 4, pixels);
        var result = atlas.Pack();

        // First pixel should be red
        Assert.Equal(255, result.Pixels[0]);
    }
}
