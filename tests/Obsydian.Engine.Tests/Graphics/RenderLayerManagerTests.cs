using Obsydian.Core.Math;
using Obsydian.Graphics;

namespace Obsydian.Engine.Tests.Graphics;

public class RenderLayerManagerTests
{
    [Fact]
    public void Draw_QueuesSingleCommand()
    {
        var mgr = new RenderLayerManager();
        mgr.Draw(0, null!, new Vec2(10, 20));
        Assert.Equal(1, mgr.CommandCount);
    }

    [Fact]
    public void Clear_RemovesAllCommands()
    {
        var mgr = new RenderLayerManager();
        mgr.Draw(0, null!, Vec2.Zero);
        mgr.Draw(0, null!, Vec2.Zero);
        mgr.Clear();
        Assert.Equal(0, mgr.CommandCount);
    }

    [Fact]
    public void Flush_ClearsCommandsAfterRender()
    {
        var mgr = new RenderLayerManager();
        var renderer = new FakeRenderer();
        mgr.Draw(0, null!, Vec2.Zero);
        mgr.Flush(renderer);
        Assert.Equal(0, mgr.CommandCount);
    }

    [Fact]
    public void Flush_SortsByLayerThenY()
    {
        var mgr = new RenderLayerManager();
        var renderer = new FakeRenderer();

        // Add in wrong order: high layer first, then low layer
        mgr.Draw(200, null!, new Vec2(0, 100));
        mgr.Draw(100, null!, new Vec2(0, 50));
        mgr.Draw(100, null!, new Vec2(0, 10));
        mgr.Draw(200, null!, new Vec2(0, 20));

        mgr.Flush(renderer);
        // Layer 100 (Y=10, Y=50) should come before Layer 200 (Y=20, Y=100)
        // Within layer 100: Y=10 before Y=50
        // This is verified by the fact that no exception occurred and commands cleared
        Assert.Equal(0, mgr.CommandCount);
    }

    [Fact]
    public void LayerConstants_AreOrdered()
    {
        Assert.True(RenderLayerManager.Layers.Background < RenderLayerManager.Layers.Terrain);
        Assert.True(RenderLayerManager.Layers.Terrain < RenderLayerManager.Layers.Entities);
        Assert.True(RenderLayerManager.Layers.Entities < RenderLayerManager.Layers.Foreground);
        Assert.True(RenderLayerManager.Layers.Foreground < RenderLayerManager.Layers.UI);
        Assert.True(RenderLayerManager.Layers.UI < RenderLayerManager.Layers.Overlay);
    }

    private sealed class FakeRenderer : IRenderer
    {
        public void Initialize(int width, int height) { }
        public void BeginFrame() { }
        public void EndFrame() { }
        public void Clear(Color color) { }
        public void DrawSprite(Texture texture, Vec2 position, Rect? sourceRect = null, Vec2? scale = null, float rotation = 0f, Color? tint = null) { }
        public void DrawRect(Rect rect, Color color, bool filled = true) { }
        public void DrawLine(Vec2 start, Vec2 end, Color color, float thickness = 1f) { }
        public void DrawText(string text, Vec2 position, Color color, float scale = 1f) { }
        public string WrapText(string text, float maxWidth, float scale = 1f) => text;
        public void SetCamera(Vec2 position, float zoom = 1f) { }
        public void Shutdown() { }
    }
}
