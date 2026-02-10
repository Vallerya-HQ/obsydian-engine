using System.Numerics;
using Obsydian.Core.Logging;
using Silk.NET.OpenGL;

namespace Obsydian.Platform.Desktop.Rendering;

/// <summary>
/// Compiles and links a vertex + fragment shader pair.
/// Provides uniform setters for projection, view, and texture sampler.
/// </summary>
public sealed class ShaderProgram : IDisposable
{
    private readonly GL _gl;
    private readonly uint _handle;
    private readonly Dictionary<string, int> _uniformLocations = new();

    public ShaderProgram(GL gl, string vertexSource, string fragmentSource)
    {
        _gl = gl;

        var vertex = CompileShader(ShaderType.VertexShader, vertexSource);
        var fragment = CompileShader(ShaderType.FragmentShader, fragmentSource);

        _handle = _gl.CreateProgram();
        _gl.AttachShader(_handle, vertex);
        _gl.AttachShader(_handle, fragment);
        _gl.LinkProgram(_handle);

        _gl.GetProgram(_handle, ProgramPropertyARB.LinkStatus, out var status);
        if (status == 0)
        {
            var log = _gl.GetProgramInfoLog(_handle);
            throw new Exception($"Shader link error: {log}");
        }

        _gl.DeleteShader(vertex);
        _gl.DeleteShader(fragment);

        Log.Debug("Shader", "Shader program compiled and linked.");
    }

    public void Use() => _gl.UseProgram(_handle);

    public void SetUniform(string name, int value)
    {
        _gl.Uniform1(GetLocation(name), value);
    }

    public unsafe void SetUniform(string name, Matrix4x4 value)
    {
        _gl.UniformMatrix4(GetLocation(name), 1, false, (float*)&value);
    }

    private int GetLocation(string name)
    {
        if (!_uniformLocations.TryGetValue(name, out var location))
        {
            location = _gl.GetUniformLocation(_handle, name);
            _uniformLocations[name] = location;
        }
        return location;
    }

    private uint CompileShader(ShaderType type, string source)
    {
        var shader = _gl.CreateShader(type);
        _gl.ShaderSource(shader, source);
        _gl.CompileShader(shader);

        _gl.GetShader(shader, ShaderParameterName.CompileStatus, out var status);
        if (status == 0)
        {
            var log = _gl.GetShaderInfoLog(shader);
            throw new Exception($"{type} compile error: {log}");
        }

        return shader;
    }

    public void Dispose()
    {
        _gl.DeleteProgram(_handle);
    }
}
