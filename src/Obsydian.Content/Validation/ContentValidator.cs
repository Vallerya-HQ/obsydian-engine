using Obsydian.Core.Logging;

namespace Obsydian.Content.Validation;

/// <summary>
/// Validates content integrity at load time and during build.
/// Checks for missing files, broken references, and schema conformance.
/// </summary>
public sealed class ContentValidator
{
    private readonly List<ValidationIssue> _issues = [];

    /// <summary>All issues found during validation.</summary>
    public IReadOnlyList<ValidationIssue> Issues => _issues;

    /// <summary>Whether any errors (not warnings) were found.</summary>
    public bool HasErrors => _issues.Any(i => i.Severity == IssueSeverity.Error);

    /// <summary>
    /// Validate a manifest against a content root directory.
    /// Checks that all files exist and dependencies are satisfied.
    /// </summary>
    public void ValidateManifest(ContentManifest manifest, string rootPath)
    {
        foreach (var entry in manifest.Entries.Values)
        {
            var fullPath = Path.Combine(rootPath, entry.Path);

            // Check file exists
            if (!File.Exists(fullPath))
            {
                _issues.Add(new ValidationIssue(
                    IssueSeverity.Error,
                    entry.Path,
                    "File not found on disk"));
                continue;
            }

            // Check file size matches
            var actualSize = new FileInfo(fullPath).Length;
            if (entry.SizeBytes > 0 && actualSize != entry.SizeBytes)
            {
                _issues.Add(new ValidationIssue(
                    IssueSeverity.Warning,
                    entry.Path,
                    $"Size mismatch: manifest says {entry.SizeBytes} bytes, actual is {actualSize} bytes"));
            }

            // Check dependencies exist in manifest
            foreach (var dep in entry.Dependencies)
            {
                if (!manifest.Contains(dep))
                {
                    _issues.Add(new ValidationIssue(
                        IssueSeverity.Error,
                        entry.Path,
                        $"Missing dependency: {dep}"));
                }
            }
        }

        // Check for orphaned files (on disk but not in manifest)
        if (Directory.Exists(rootPath))
        {
            foreach (var file in Directory.EnumerateFiles(rootPath, "*", SearchOption.AllDirectories))
            {
                var relativePath = Path.GetRelativePath(rootPath, file).Replace('\\', '/');
                if (!manifest.Contains(relativePath) && !relativePath.EndsWith(".manifest.json"))
                {
                    _issues.Add(new ValidationIssue(
                        IssueSeverity.Warning,
                        relativePath,
                        "File exists on disk but is not in manifest"));
                }
            }
        }

        if (HasErrors)
            Log.Warn("Validator", $"Validation found {_issues.Count(i => i.Severity == IssueSeverity.Error)} errors, {_issues.Count(i => i.Severity == IssueSeverity.Warning)} warnings");
        else
            Log.Info("Validator", $"Validation passed with {_issues.Count} warnings");
    }

    /// <summary>
    /// Validate that a content directory has no broken Tiled map references.
    /// </summary>
    public void ValidateTiledMaps(string rootPath)
    {
        foreach (var mapFile in Directory.EnumerateFiles(rootPath, "*.tmj", SearchOption.AllDirectories))
        {
            try
            {
                var json = File.ReadAllText(mapFile);
                var relativePath = Path.GetRelativePath(rootPath, mapFile).Replace('\\', '/');

                // Check for tileset references
                if (json.Contains("\"source\""))
                {
                    // Extract external tileset references and verify they exist
                    var mapDir = Path.GetDirectoryName(mapFile) ?? "";
                    // Simple regex-free check: look for .tsj/.tsx references
                    foreach (var ext in new[] { ".tsj", ".tsx" })
                    {
                        var idx = 0;
                        while ((idx = json.IndexOf(ext, idx, StringComparison.Ordinal)) >= 0)
                        {
                            // Walk backwards to find the start of the string value
                            var start = json.LastIndexOf('"', idx);
                            if (start >= 0)
                            {
                                var tilesetRef = json[(start + 1)..json.IndexOf('"', idx)];
                                var tilesetPath = Path.Combine(mapDir, tilesetRef);
                                if (!File.Exists(tilesetPath))
                                {
                                    _issues.Add(new ValidationIssue(
                                        IssueSeverity.Error,
                                        relativePath,
                                        $"Missing tileset reference: {tilesetRef}"));
                                }
                            }
                            idx += ext.Length;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                var rel = Path.GetRelativePath(rootPath, mapFile).Replace('\\', '/');
                _issues.Add(new ValidationIssue(IssueSeverity.Error, rel, $"Failed to parse: {ex.Message}"));
            }
        }
    }

    /// <summary>Clear all recorded issues.</summary>
    public void Clear() => _issues.Clear();
}

public enum IssueSeverity
{
    Warning,
    Error,
}

public sealed record ValidationIssue(IssueSeverity Severity, string AssetPath, string Message)
{
    public override string ToString() => $"[{Severity}] {AssetPath}: {Message}";
}
