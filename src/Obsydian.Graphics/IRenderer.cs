using Obsydian.Core.Math;

namespace Obsydian.Graphics;

/// <summary>
/// Core 2D rendering interface. Implementations can use OpenGL, Vulkan, Metal, etc.
/// </summary>
public interface IRenderer
{
    void Initialize(int width, int height);
    void BeginFrame();
    void EndFrame();

    void Clear(Color color);
    void DrawSprite(Texture texture, Vec2 position, Rect? sourceRect = null, Vec2? scale = null, float rotation = 0f, Color? tint = null);
    void DrawRect(Rect rect, Color color, bool filled = true);
    void DrawLine(Vec2 start, Vec2 end, Color color, float thickness = 1f);
    void DrawText(string text, Vec2 position, Color color, float scale = 1f);
    string WrapText(string text, float maxWidth, float scale = 1f);

    void SetCamera(Vec2 position, float zoom = 1f);
    void Shutdown();
}
