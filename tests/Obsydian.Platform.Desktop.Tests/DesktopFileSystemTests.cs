using Obsydian.Platform.Desktop;

namespace Obsydian.Platform.Desktop.Tests;

public class DesktopFileSystemTests : IDisposable
{
    private readonly string _tempDir;
    private readonly DesktopFileSystem _fs;

    public DesktopFileSystemTests()
    {
        _tempDir = Path.Combine(Path.GetTempPath(), $"obsydian_test_{Guid.NewGuid():N}");
        Directory.CreateDirectory(_tempDir);
        _fs = new DesktopFileSystem(_tempDir);
    }

    [Fact]
    public void GetContentRoot_Returns_Configured_Root()
    {
        Assert.Equal(_tempDir, _fs.GetContentRoot());
    }

    [Fact]
    public void WriteAndReadText_Roundtrips()
    {
        _fs.WriteAllText("test.txt", "hello obsydian");
        var result = _fs.ReadAllText("test.txt");
        Assert.Equal("hello obsydian", result);
    }

    [Fact]
    public void WriteAndReadBytes_Roundtrips()
    {
        byte[] data = [1, 2, 3, 4, 5];
        _fs.WriteAllBytes("test.bin", data);
        var result = _fs.ReadAllBytes("test.bin");
        Assert.Equal(data, result);
    }

    [Fact]
    public void Exists_Returns_True_For_Existing_File()
    {
        _fs.WriteAllText("exists.txt", "yes");
        Assert.True(_fs.Exists("exists.txt"));
    }

    [Fact]
    public void Exists_Returns_False_For_Missing_File()
    {
        Assert.False(_fs.Exists("nope.txt"));
    }

    [Fact]
    public void EnumerateFiles_Finds_Written_Files()
    {
        _fs.WriteAllText("a.txt", "a");
        _fs.WriteAllText("b.txt", "b");

        var files = _fs.EnumerateFiles("", "*.txt").ToList();
        Assert.Equal(2, files.Count);
    }

    [Fact]
    public void EnumerateFiles_Returns_Empty_For_Missing_Directory()
    {
        var files = _fs.EnumerateFiles("nonexistent", "*.txt").ToList();
        Assert.Empty(files);
    }

    [Fact]
    public void Absolute_Path_Bypasses_ContentRoot()
    {
        var absolutePath = Path.Combine(_tempDir, "absolute.txt");
        _fs.WriteAllText(absolutePath, "absolute");
        Assert.True(_fs.Exists(absolutePath));
        Assert.Equal("absolute", _fs.ReadAllText(absolutePath));
    }

    public void Dispose()
    {
        if (Directory.Exists(_tempDir))
            Directory.Delete(_tempDir, recursive: true);
    }
}
