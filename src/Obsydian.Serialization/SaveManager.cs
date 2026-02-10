using System.Text.Json;
using Obsydian.Core.Logging;

namespace Obsydian.Serialization;

/// <summary>
/// Handles saving and loading game state with versioned JSON serialization.
/// </summary>
public sealed class SaveManager
{
    private readonly string _saveDirectory;
    private readonly JsonSerializerOptions _options;

    public SaveManager(string saveDirectory)
    {
        _saveDirectory = saveDirectory;
        _options = new JsonSerializerOptions
        {
            WriteIndented = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        };
    }

    public void Save<T>(string slotName, T data) where T : class
    {
        Directory.CreateDirectory(_saveDirectory);
        var path = GetSavePath(slotName);
        var wrapper = new SaveWrapper<T>
        {
            Version = 1,
            Timestamp = DateTime.UtcNow,
            Data = data
        };

        var json = JsonSerializer.Serialize(wrapper, _options);
        File.WriteAllText(path, json);
        Log.Info("Save", $"Saved to {slotName}");
    }

    public T? Load<T>(string slotName) where T : class
    {
        var path = GetSavePath(slotName);
        if (!File.Exists(path))
        {
            Log.Warn("Save", $"Save file not found: {slotName}");
            return null;
        }

        var json = File.ReadAllText(path);
        var wrapper = JsonSerializer.Deserialize<SaveWrapper<T>>(json, _options);
        Log.Info("Save", $"Loaded {slotName} (v{wrapper?.Version})");
        return wrapper?.Data;
    }

    public bool Exists(string slotName) => File.Exists(GetSavePath(slotName));

    public void Delete(string slotName)
    {
        var path = GetSavePath(slotName);
        if (File.Exists(path))
            File.Delete(path);
    }

    public IEnumerable<string> ListSaves()
    {
        if (!Directory.Exists(_saveDirectory))
            yield break;

        foreach (var file in Directory.GetFiles(_saveDirectory, "*.json"))
            yield return Path.GetFileNameWithoutExtension(file);
    }

    private string GetSavePath(string slotName) =>
        Path.Combine(_saveDirectory, $"{slotName}.json");
}

internal sealed class SaveWrapper<T>
{
    public int Version { get; set; }
    public DateTime Timestamp { get; set; }
    public T? Data { get; set; }
}
