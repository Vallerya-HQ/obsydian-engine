namespace Obsydian.Serialization;

/// <summary>
/// Defines a migration step from one save version to the next.
/// Each migrator transforms the raw JSON structure to the new format.
/// </summary>
public interface ISaveMigrator
{
    /// <summary>The version this migrator upgrades FROM.</summary>
    int FromVersion { get; }

    /// <summary>The version this migrator upgrades TO.</summary>
    int ToVersion { get; }

    /// <summary>
    /// Transform the raw JSON data from FromVersion to ToVersion.
    /// Receives the data section of the save wrapper as a JsonElement.
    /// Returns the migrated JSON string for the data section.
    /// </summary>
    string Migrate(string dataJson);
}

/// <summary>
/// Manages a chain of save migrators to upgrade old saves to current version.
/// </summary>
public sealed class SaveMigrationChain
{
    private readonly SortedList<int, ISaveMigrator> _migrators = [];

    /// <summary>The latest version this chain can migrate to.</summary>
    public int LatestVersion { get; }

    public SaveMigrationChain(int latestVersion)
    {
        LatestVersion = latestVersion;
    }

    /// <summary>Register a migrator.</summary>
    public void Register(ISaveMigrator migrator)
    {
        _migrators[migrator.FromVersion] = migrator;
    }

    /// <summary>
    /// Migrate JSON data from the given version to the latest version.
    /// Returns the migrated JSON string.
    /// </summary>
    public string MigrateToLatest(string dataJson, int fromVersion)
    {
        var current = dataJson;
        var version = fromVersion;

        while (version < LatestVersion)
        {
            if (!_migrators.TryGetValue(version, out var migrator))
                throw new InvalidOperationException(
                    $"No migrator found for version {version}. Cannot upgrade to {LatestVersion}.");

            current = migrator.Migrate(current);
            version = migrator.ToVersion;
        }

        return current;
    }

    /// <summary>Check if migration is possible from the given version.</summary>
    public bool CanMigrate(int fromVersion)
    {
        var version = fromVersion;
        while (version < LatestVersion)
        {
            if (!_migrators.ContainsKey(version))
                return false;
            version = _migrators[version].ToVersion;
        }
        return true;
    }
}
