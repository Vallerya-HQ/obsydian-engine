using Obsydian.Core.Math;
using Silk.NET.OpenGL;
using Texture = Obsydian.Graphics.Texture;

namespace Obsydian.Platform.Desktop.Rendering;

/// <summary>
/// Batches sprite draw calls into a single dynamic vertex buffer.
/// Auto-flushes on texture change or when the buffer is full (1024 quads).
/// Vertex format: x, y, u, v, r, g, b, a (8 floats = 32 bytes per vertex).
/// </summary>
public sealed class SpriteBatch : IDisposable
{
    private const int MaxQuads = 1024;
    private const int VerticesPerQuad = 4;
    private const int IndicesPerQuad = 6;
    private const int FloatsPerVertex = 8; // x, y, u, v, r, g, b, a

    private readonly GL _gl;
    private readonly uint _vao;
    private readonly uint _vbo;
    private readonly uint _ebo;

    private readonly float[] _vertices = new float[MaxQuads * VerticesPerQuad * FloatsPerVertex];
    private int _quadCount;
    private int _currentTextureId;

    public int DrawCallCount { get; private set; }
    public int QuadCount => _quadCount;

    public SpriteBatch(GL gl)
    {
        _gl = gl;

        _vao = _gl.GenVertexArray();
        _vbo = _gl.GenBuffer();
        _ebo = _gl.GenBuffer();

        _gl.BindVertexArray(_vao);

        // Vertex buffer — allocated once at max capacity, updated with BufferSubData each flush
        _gl.BindBuffer(BufferTargetARB.ArrayBuffer, _vbo);
        var vertexBufferSize = (nuint)(MaxQuads * VerticesPerQuad * FloatsPerVertex * sizeof(float));
        unsafe { _gl.BufferData(BufferTargetARB.ArrayBuffer, vertexBufferSize, null, BufferUsageARB.DynamicDraw); }

        // Index buffer — static pattern: 0,1,2, 2,3,0 for each quad
        var indices = GenerateIndices();
        unsafe
        {
            fixed (uint* ptr = indices)
            {
                _gl.BindBuffer(BufferTargetARB.ElementArrayBuffer, _ebo);
                _gl.BufferData(BufferTargetARB.ElementArrayBuffer, (nuint)(indices.Length * sizeof(uint)), ptr, BufferUsageARB.StaticDraw);
            }
        }

        // Vertex attributes
        var stride = (uint)(FloatsPerVertex * sizeof(float));

        // position: vec2 (x, y)
        _gl.EnableVertexAttribArray(0);
        _gl.VertexAttribPointer(0, 2, VertexAttribPointerType.Float, false, stride, 0);

        // texcoord: vec2 (u, v)
        _gl.EnableVertexAttribArray(1);
        _gl.VertexAttribPointer(1, 2, VertexAttribPointerType.Float, false, stride, 2 * sizeof(float));

        // color: vec4 (r, g, b, a)
        _gl.EnableVertexAttribArray(2);
        _gl.VertexAttribPointer(2, 4, VertexAttribPointerType.Float, false, stride, 4 * sizeof(float));

        _gl.BindVertexArray(0);
    }

    public void Begin()
    {
        _quadCount = 0;
        _currentTextureId = 0;
        DrawCallCount = 0;
    }

    public void Draw(Texture texture, Rect destRect, Rect srcRect, Color tint, float rotation = 0f)
    {
        // Flush if texture changed or buffer is full
        if (_quadCount > 0 && texture.Id != _currentTextureId)
            Flush();
        if (_quadCount >= MaxQuads)
            Flush();

        _currentTextureId = texture.Id;

        // Normalize source rect to UV coordinates
        float u0 = srcRect.X / texture.Width;
        float v0 = srcRect.Y / texture.Height;
        float u1 = (srcRect.X + srcRect.Width) / texture.Width;
        float v1 = (srcRect.Y + srcRect.Height) / texture.Height;

        // Destination corners
        float x0 = destRect.X;
        float y0 = destRect.Y;
        float x1 = destRect.X + destRect.Width;
        float y1 = destRect.Y + destRect.Height;

        // Color as normalized floats
        float r = tint.R / 255f;
        float g = tint.G / 255f;
        float b = tint.B / 255f;
        float a = tint.A / 255f;

        if (rotation != 0f)
        {
            // Rotate around the center of the destination rect
            float cx = (x0 + x1) * 0.5f;
            float cy = (y0 + y1) * 0.5f;
            float cos = MathF.Cos(rotation);
            float sin = MathF.Sin(rotation);

            WriteRotatedVertex(x0, y0, cx, cy, cos, sin, u0, v0, r, g, b, a, 0);
            WriteRotatedVertex(x1, y0, cx, cy, cos, sin, u1, v0, r, g, b, a, 1);
            WriteRotatedVertex(x1, y1, cx, cy, cos, sin, u1, v1, r, g, b, a, 2);
            WriteRotatedVertex(x0, y1, cx, cy, cos, sin, u0, v1, r, g, b, a, 3);
        }
        else
        {
            int baseIndex = _quadCount * VerticesPerQuad * FloatsPerVertex;

            // Top-left
            _vertices[baseIndex + 0] = x0; _vertices[baseIndex + 1] = y0;
            _vertices[baseIndex + 2] = u0; _vertices[baseIndex + 3] = v0;
            _vertices[baseIndex + 4] = r;  _vertices[baseIndex + 5] = g;
            _vertices[baseIndex + 6] = b;  _vertices[baseIndex + 7] = a;

            // Top-right
            _vertices[baseIndex + 8] = x1;  _vertices[baseIndex + 9] = y0;
            _vertices[baseIndex + 10] = u1; _vertices[baseIndex + 11] = v0;
            _vertices[baseIndex + 12] = r;  _vertices[baseIndex + 13] = g;
            _vertices[baseIndex + 14] = b;  _vertices[baseIndex + 15] = a;

            // Bottom-right
            _vertices[baseIndex + 16] = x1; _vertices[baseIndex + 17] = y1;
            _vertices[baseIndex + 18] = u1; _vertices[baseIndex + 19] = v1;
            _vertices[baseIndex + 20] = r;  _vertices[baseIndex + 21] = g;
            _vertices[baseIndex + 22] = b;  _vertices[baseIndex + 23] = a;

            // Bottom-left
            _vertices[baseIndex + 24] = x0; _vertices[baseIndex + 25] = y1;
            _vertices[baseIndex + 26] = u0; _vertices[baseIndex + 27] = v1;
            _vertices[baseIndex + 28] = r;  _vertices[baseIndex + 29] = g;
            _vertices[baseIndex + 30] = b;  _vertices[baseIndex + 31] = a;
        }

        _quadCount++;
    }

    public void End()
    {
        if (_quadCount > 0)
            Flush();
    }

    public void Flush()
    {
        if (_quadCount == 0) return;

        _gl.BindVertexArray(_vao);
        _gl.BindTexture(TextureTarget.Texture2D, (uint)_currentTextureId);

        // Upload only the used portion of the vertex buffer
        var byteCount = (nuint)(_quadCount * VerticesPerQuad * FloatsPerVertex * sizeof(float));
        unsafe
        {
            fixed (float* ptr = _vertices)
            {
                _gl.BindBuffer(BufferTargetARB.ArrayBuffer, _vbo);
                _gl.BufferSubData(BufferTargetARB.ArrayBuffer, 0, byteCount, ptr);
            }
        }

        unsafe
        {
            _gl.DrawElements(PrimitiveType.Triangles, (uint)(_quadCount * IndicesPerQuad), DrawElementsType.UnsignedInt, (void*)0);
        }

        DrawCallCount++;
        _quadCount = 0;
    }

    private void WriteRotatedVertex(float x, float y, float cx, float cy, float cos, float sin,
        float u, float v, float r, float g, float b, float a, int vertexOffset)
    {
        float dx = x - cx;
        float dy = y - cy;
        float rx = cx + dx * cos - dy * sin;
        float ry = cy + dx * sin + dy * cos;

        int baseIndex = (_quadCount * VerticesPerQuad + vertexOffset) * FloatsPerVertex;
        _vertices[baseIndex + 0] = rx; _vertices[baseIndex + 1] = ry;
        _vertices[baseIndex + 2] = u;  _vertices[baseIndex + 3] = v;
        _vertices[baseIndex + 4] = r;  _vertices[baseIndex + 5] = g;
        _vertices[baseIndex + 6] = b;  _vertices[baseIndex + 7] = a;
    }

    private static uint[] GenerateIndices()
    {
        var indices = new uint[MaxQuads * IndicesPerQuad];
        for (uint i = 0; i < MaxQuads; i++)
        {
            uint baseVertex = i * VerticesPerQuad;
            uint baseIndex = i * IndicesPerQuad;
            indices[baseIndex + 0] = baseVertex + 0;
            indices[baseIndex + 1] = baseVertex + 1;
            indices[baseIndex + 2] = baseVertex + 2;
            indices[baseIndex + 3] = baseVertex + 2;
            indices[baseIndex + 4] = baseVertex + 3;
            indices[baseIndex + 5] = baseVertex + 0;
        }
        return indices;
    }

    public void Dispose()
    {
        _gl.DeleteVertexArray(_vao);
        _gl.DeleteBuffer(_vbo);
        _gl.DeleteBuffer(_ebo);
    }
}
