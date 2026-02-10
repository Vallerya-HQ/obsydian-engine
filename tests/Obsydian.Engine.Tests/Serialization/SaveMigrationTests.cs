using Obsydian.Serialization;

namespace Obsydian.Engine.Tests.Serialization;

public class SaveMigrationTests
{
    private sealed class V1ToV2Migrator : ISaveMigrator
    {
        public int FromVersion => 1;
        public int ToVersion => 2;
        public string Migrate(string dataJson) => dataJson.Replace("\"name\"", "\"displayName\"");
    }

    private sealed class V2ToV3Migrator : ISaveMigrator
    {
        public int FromVersion => 2;
        public int ToVersion => 3;
        public string Migrate(string dataJson) => dataJson.Replace("\"score\"", "\"totalScore\"");
    }

    [Fact]
    public void SingleMigration_TransformsData()
    {
        var chain = new SaveMigrationChain(2);
        chain.Register(new V1ToV2Migrator());

        var result = chain.MigrateToLatest("{\"name\":\"test\"}", 1);
        Assert.Contains("\"displayName\"", result);
        Assert.DoesNotContain("\"name\"", result);
    }

    [Fact]
    public void ChainedMigration_AppliesInOrder()
    {
        var chain = new SaveMigrationChain(3);
        chain.Register(new V1ToV2Migrator());
        chain.Register(new V2ToV3Migrator());

        var result = chain.MigrateToLatest("{\"name\":\"test\",\"score\":100}", 1);
        Assert.Contains("\"displayName\"", result);
        Assert.Contains("\"totalScore\"", result);
    }

    [Fact]
    public void CanMigrate_ReturnsTrueForValidChain()
    {
        var chain = new SaveMigrationChain(3);
        chain.Register(new V1ToV2Migrator());
        chain.Register(new V2ToV3Migrator());

        Assert.True(chain.CanMigrate(1));
        Assert.True(chain.CanMigrate(2));
        Assert.True(chain.CanMigrate(3)); // already at latest
    }

    [Fact]
    public void CanMigrate_ReturnsFalseForBrokenChain()
    {
        var chain = new SaveMigrationChain(3);
        chain.Register(new V1ToV2Migrator());
        // Missing V2->V3

        Assert.False(chain.CanMigrate(1));
    }

    [Fact]
    public void MigrateToLatest_ThrowsForMissingMigrator()
    {
        var chain = new SaveMigrationChain(3);
        chain.Register(new V1ToV2Migrator());

        Assert.Throws<InvalidOperationException>(() =>
            chain.MigrateToLatest("{}", 1));
    }

    [Fact]
    public void MigrateToLatest_NoOpForCurrentVersion()
    {
        var chain = new SaveMigrationChain(1);
        var result = chain.MigrateToLatest("{\"data\":1}", 1);
        Assert.Equal("{\"data\":1}", result);
    }

    [Fact]
    public void SaveManager_HasMigrationChainProperty()
    {
        var tmpDir = Path.Combine(Path.GetTempPath(), "obsydian_test_" + Guid.NewGuid().ToString("N"));
        try
        {
            var mgr = new SaveManager(tmpDir);
            Assert.Null(mgr.MigrationChain);
            Assert.Equal(1, mgr.CurrentVersion);

            mgr.CurrentVersion = 2;
            mgr.MigrationChain = new SaveMigrationChain(2);
            Assert.NotNull(mgr.MigrationChain);
        }
        finally
        {
            if (Directory.Exists(tmpDir))
                Directory.Delete(tmpDir, true);
        }
    }

    [Fact]
    public void SaveManager_SaveAndLoad_WithVersion()
    {
        var tmpDir = Path.Combine(Path.GetTempPath(), "obsydian_test_" + Guid.NewGuid().ToString("N"));
        try
        {
            var mgr = new SaveManager(tmpDir) { CurrentVersion = 5 };
            mgr.Save("slot1", new TestData { Name = "Hero", Level = 10 });

            var loaded = mgr.Load<TestData>("slot1");
            Assert.NotNull(loaded);
            Assert.Equal("Hero", loaded.Name);
            Assert.Equal(10, loaded.Level);
        }
        finally
        {
            if (Directory.Exists(tmpDir))
                Directory.Delete(tmpDir, true);
        }
    }

    public sealed class TestData
    {
        public string Name { get; set; } = "";
        public int Level { get; set; }
    }
}
