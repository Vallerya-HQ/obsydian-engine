namespace Obsydian.Platform;

/// <summary>
/// Abstraction over file system operations for cross-platform asset loading.
/// </summary>
public interface IFileSystem
{
    byte[] ReadAllBytes(string path);
    string ReadAllText(string path);
    void WriteAllBytes(string path, byte[] data);
    void WriteAllText(string path, string text);
    bool Exists(string path);
    IEnumerable<string> EnumerateFiles(string directory, string pattern = "*");
    string GetContentRoot();
}
