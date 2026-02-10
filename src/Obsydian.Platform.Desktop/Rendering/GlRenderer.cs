using System.Numerics;
using Obsydian.Core.Logging;
using Obsydian.Core.Math;
using Obsydian.Graphics;
using Silk.NET.OpenGL;
using Texture = Obsydian.Graphics.Texture;

namespace Obsydian.Platform.Desktop.Rendering;

/// <summary>
/// IRenderer implementation using OpenGL 3.3 core profile via Silk.NET.
/// Uses a SpriteBatch for efficient 2D sprite rendering.
/// </summary>
public sealed class GlRenderer : IRenderer, IDisposable
{
    private GL _gl = null!;
    private ShaderProgram _shader = null!;
    private SpriteBatch _batch = null!;
    private Texture _whitePixel = null!;

    private Matrix4x4 _projection;
    private Matrix4x4 _view = Matrix4x4.Identity;
    private int _viewportWidth;
    private int _viewportHeight;

    /// <summary>The underlying GL context. Available after Initialize().</summary>
    public GL Gl => _gl;

    /// <summary>Number of draw calls in the last frame.</summary>
    public int LastDrawCallCount { get; private set; }

    private const string VertexShader = """
        #version 330 core
        layout (location = 0) in vec2 aPosition;
        layout (location = 1) in vec2 aTexCoord;
        layout (location = 2) in vec4 aColor;

        out vec2 vTexCoord;
        out vec4 vColor;

        uniform mat4 uProjection;
        uniform mat4 uView;

        void main()
        {
            gl_Position = uProjection * uView * vec4(aPosition, 0.0, 1.0);
            vTexCoord = aTexCoord;
            vColor = aColor;
        }
        """;

    private const string FragmentShader = """
        #version 330 core
        in vec2 vTexCoord;
        in vec4 vColor;

        out vec4 FragColor;

        uniform sampler2D uTexture;

        void main()
        {
            FragColor = texture(uTexture, vTexCoord) * vColor;
        }
        """;

    public void Initialize(int width, int height)
    {
        // GL context must be provided via InitializeWithGl() — this is the IRenderer path
        throw new InvalidOperationException(
            "Use InitializeWithGl(GL, int, int) to provide the Silk.NET GL context.");
    }

    public void InitializeWithGl(GL gl, int width, int height)
    {
        _gl = gl;
        _viewportWidth = width;
        _viewportHeight = height;

        _gl.Enable(EnableCap.Blend);
        _gl.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);

        _shader = new ShaderProgram(_gl, VertexShader, FragmentShader);
        _batch = new SpriteBatch(_gl);
        _whitePixel = GlTexture.CreateWhitePixel(_gl);

        _projection = Matrix4x4.CreateOrthographicOffCenter(0, width, height, 0, -1f, 1f);

        _shader.Use();
        _shader.SetUniform("uTexture", 0);
        _shader.SetUniform("uProjection", _projection);
        _shader.SetUniform("uView", _view);

        Log.Info("Renderer", $"OpenGL {_gl.GetStringS(StringName.Version)}");
        Log.Info("Renderer", $"Viewport: {width}x{height}");
    }

    public void OnResize(int width, int height)
    {
        _viewportWidth = width;
        _viewportHeight = height;
        _gl.Viewport(0, 0, (uint)width, (uint)height);
        _projection = Matrix4x4.CreateOrthographicOffCenter(0, width, height, 0, -1f, 1f);
    }

    public void BeginFrame()
    {
        _shader.Use();
        _shader.SetUniform("uProjection", _projection);
        _shader.SetUniform("uView", _view);
        _gl.ActiveTexture(TextureUnit.Texture0);
        _batch.Begin();
    }

    public void EndFrame()
    {
        _batch.End();
        LastDrawCallCount = _batch.DrawCallCount;
    }

    public void Clear(Color color)
    {
        _gl.ClearColor(color.R / 255f, color.G / 255f, color.B / 255f, color.A / 255f);
        _gl.Clear(ClearBufferMask.ColorBufferBit);
    }

    public void DrawSprite(Texture texture, Vec2 position, Rect? sourceRect = null, Vec2? scale = null, float rotation = 0f, Color? tint = null)
    {
        var src = sourceRect ?? new Rect(0, 0, texture.Width, texture.Height);
        var s = scale ?? Vec2.One;
        var c = tint ?? Color.White;

        var dest = new Rect(position.X, position.Y, src.Width * s.X, src.Height * s.Y);
        _batch.Draw(texture, dest, src, c, rotation);
    }

    public void DrawRect(Rect rect, Color color, bool filled = true)
    {
        if (filled)
        {
            var src = new Rect(0, 0, 1, 1);
            _batch.Draw(_whitePixel, rect, src, color);
        }
        else
        {
            float t = 1f; // 1 pixel line thickness
            // Top
            DrawRect(new Rect(rect.X, rect.Y, rect.Width, t), color);
            // Bottom
            DrawRect(new Rect(rect.X, rect.Bottom - t, rect.Width, t), color);
            // Left
            DrawRect(new Rect(rect.X, rect.Y + t, t, rect.Height - 2 * t), color);
            // Right
            DrawRect(new Rect(rect.Right - t, rect.Y + t, t, rect.Height - 2 * t), color);
        }
    }

    public void DrawLine(Vec2 start, Vec2 end, Color color, float thickness = 1f)
    {
        var dir = end - start;
        var len = dir.Length;
        if (len == 0) return;

        var norm = new Vec2(-dir.Y / len, dir.X / len) * (thickness * 0.5f);

        // Build a thin quad along the line
        float x0 = start.X + norm.X;
        float y0 = start.Y + norm.Y;
        float x1 = start.X - norm.X;
        float y1 = start.Y - norm.Y;
        float x2 = end.X - norm.X;
        float y2 = end.Y - norm.Y;
        float x3 = end.X + norm.X;
        float y3 = end.Y + norm.Y;

        // Use the white pixel texture; the rect approach doesn't work for arbitrary lines,
        // so we draw it as a filled rect with rotation
        var angle = MathF.Atan2(dir.Y, dir.X);
        var dest = new Rect(start.X - thickness * 0.5f * MathF.Sin(angle),
                            start.Y + thickness * 0.5f * MathF.Cos(angle) - thickness * 0.5f,
                            len, thickness);
        // Simplified: just draw as a rotated rect from start point
        var src = new Rect(0, 0, 1, 1);
        var rectDest = new Rect(start.X, start.Y - thickness * 0.5f, len, thickness);
        _batch.Draw(_whitePixel, rectDest, src, color, angle);
    }

    public void DrawText(string text, Vec2 position, Color color, float scale = 1f)
    {
        // Text rendering requires a BitmapFont (Phase 5).
        // For now, this is a no-op — text is silently skipped.
    }

    public void SetCamera(Vec2 position, float zoom = 1f)
    {
        _view = Matrix4x4.CreateTranslation(-position.X, -position.Y, 0) *
                Matrix4x4.CreateScale(zoom, zoom, 1f);
    }

    public void Shutdown()
    {
        _whitePixel.Dispose();
        _batch.Dispose();
        _shader.Dispose();
        Log.Info("Renderer", "Shutdown complete.");
    }

    public void Dispose() => Shutdown();
}
