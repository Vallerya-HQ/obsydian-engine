using Obsydian.Platform;

namespace Obsydian.Platform.Desktop;

/// <summary>
/// IFileSystem implementation using System.IO for desktop platforms.
/// </summary>
public sealed class DesktopFileSystem : IFileSystem
{
    private readonly string _contentRoot;

    public DesktopFileSystem(string? contentRoot = null)
    {
        _contentRoot = contentRoot ?? AppDomain.CurrentDomain.BaseDirectory;
    }

    public byte[] ReadAllBytes(string path) => File.ReadAllBytes(ResolvePath(path));
    public string ReadAllText(string path) => File.ReadAllText(ResolvePath(path));
    public void WriteAllBytes(string path, byte[] data) => File.WriteAllBytes(ResolvePath(path), data);
    public void WriteAllText(string path, string text) => File.WriteAllText(ResolvePath(path), text);
    public bool Exists(string path) => File.Exists(ResolvePath(path));

    public IEnumerable<string> EnumerateFiles(string directory, string pattern = "*")
    {
        var fullDir = ResolvePath(directory);
        return Directory.Exists(fullDir)
            ? Directory.EnumerateFiles(fullDir, pattern, SearchOption.AllDirectories)
            : [];
    }

    public string GetContentRoot() => _contentRoot;

    private string ResolvePath(string path)
    {
        return Path.IsPathRooted(path) ? path : Path.Combine(_contentRoot, path);
    }
}
