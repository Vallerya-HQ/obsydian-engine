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

    [Fact]
    public void SaveManager_RoundTrip_PreservesComplexData()
    {
        var tmpDir = Path.Combine(Path.GetTempPath(), "obsydian_test_" + Guid.NewGuid().ToString("N"));
        try
        {
            var mgr = new SaveManager(tmpDir);
            var data = new ComplexTestData
            {
                Name = "Hero",
                Score = 42,
                Ratio = 3.14f,
                Items = ["Sword", "Shield", "Potion"]
            };

            mgr.Save("complex", data);
            var loaded = mgr.Load<ComplexTestData>("complex");

            Assert.NotNull(loaded);
            Assert.Equal("Hero", loaded.Name);
            Assert.Equal(42, loaded.Score);
            Assert.Equal(3.14f, loaded.Ratio);
            Assert.Equal(3, loaded.Items.Count);
            Assert.Equal("Shield", loaded.Items[1]);
        }
        finally
        {
            if (Directory.Exists(tmpDir))
                Directory.Delete(tmpDir, true);
        }
    }

    [Fact]
    public void SaveManager_ListSaves_ReturnsSlotNames()
    {
        var tmpDir = Path.Combine(Path.GetTempPath(), "obsydian_test_" + Guid.NewGuid().ToString("N"));
        try
        {
            var mgr = new SaveManager(tmpDir);
            mgr.Save("slot_a", new TestData { Name = "A" });
            mgr.Save("slot_b", new TestData { Name = "B" });

            var saves = mgr.ListSaves().OrderBy(s => s).ToList();

            Assert.Equal(2, saves.Count);
            Assert.Equal("slot_a", saves[0]);
            Assert.Equal("slot_b", saves[1]);
        }
        finally
        {
            if (Directory.Exists(tmpDir))
                Directory.Delete(tmpDir, true);
        }
    }

    [Fact]
    public void SaveManager_Delete_RemovesSave()
    {
        var tmpDir = Path.Combine(Path.GetTempPath(), "obsydian_test_" + Guid.NewGuid().ToString("N"));
        try
        {
            var mgr = new SaveManager(tmpDir);
            mgr.Save("doomed", new TestData { Name = "X" });
            Assert.True(mgr.Exists("doomed"));

            mgr.Delete("doomed");

            Assert.False(mgr.Exists("doomed"));
            Assert.Null(mgr.Load<TestData>("doomed"));
        }
        finally
        {
            if (Directory.Exists(tmpDir))
                Directory.Delete(tmpDir, true);
        }
    }

    [Fact]
    public void SaveManager_Load_NonExistentSlot_ReturnsNull()
    {
        var tmpDir = Path.Combine(Path.GetTempPath(), "obsydian_test_" + Guid.NewGuid().ToString("N"));
        try
        {
            var mgr = new SaveManager(tmpDir);
            var result = mgr.Load<TestData>("ghost_slot");
            Assert.Null(result);
        }
        finally
        {
            if (Directory.Exists(tmpDir))
                Directory.Delete(tmpDir, true);
        }
    }

    [Fact]
    public void SaveManager_MigratedLoad_AppliesChain()
    {
        var tmpDir = Path.Combine(Path.GetTempPath(), "obsydian_test_" + Guid.NewGuid().ToString("N"));
        try
        {
            // Save at version 1
            var mgrV1 = new SaveManager(tmpDir) { CurrentVersion = 1 };
            mgrV1.Save("migrated", new TestData { Name = "OldHero", Level = 5 });

            // Load at version 2 with migration chain
            var chain = new SaveMigrationChain(2);
            chain.Register(new V1ToV2Migrator());
            var mgrV2 = new SaveManager(tmpDir) { CurrentVersion = 2, MigrationChain = chain };

            var loaded = mgrV2.Load<MigratedTestData>("migrated");

            Assert.NotNull(loaded);
            Assert.Equal("OldHero", loaded.DisplayName); // "name" → "displayName"
            Assert.Equal(5, loaded.Level);
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

    public sealed class ComplexTestData
    {
        public string Name { get; set; } = "";
        public int Score { get; set; }
        public float Ratio { get; set; }
        public List<string> Items { get; set; } = [];
    }

    public sealed class MigratedTestData
    {
        public string DisplayName { get; set; } = "";
        public int Level { get; set; }
    }
}
