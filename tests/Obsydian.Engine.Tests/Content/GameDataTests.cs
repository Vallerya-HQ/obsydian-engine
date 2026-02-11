using Obsydian.Content;
using Obsydian.Content.Data;

namespace Obsydian.Engine.Tests.Content;

public class GameDataTests
{
    private sealed class ItemData : IGameData
    {
        public string Id { get; init; } = "";
        public string Name { get; init; } = "";
        public int Damage { get; init; }
    }

    [Fact]
    public void GameDataLoader_LoadsJsonArray()
    {
        var tmpDir = Path.Combine(Path.GetTempPath(), "obsydian_data_test_" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(tmpDir);
        File.WriteAllText(Path.Combine(tmpDir, "items.json"), """
            [
                { "id": "sword", "name": "Iron Sword", "damage": 10 },
                { "id": "axe", "name": "Battle Axe", "damage": 15 }
            ]
            """);

        try
        {
            using var content = new ContentManager(tmpDir);
            content.RegisterLoader(new GameDataLoader<ItemData>());

            var items = content.Load<GameDataCollection<ItemData>>("items.json");
            Assert.Equal(2, items.Count);

            var sword = items.GetById("sword");
            Assert.NotNull(sword);
            Assert.Equal("Iron Sword", sword.Name);
            Assert.Equal(10, sword.Damage);

            Assert.True(items.Contains("axe"));
            Assert.False(items.Contains("staff"));
        }
        finally
        {
            Directory.Delete(tmpDir, true);
        }
    }

    [Fact]
    public void DataLoader_RegisterAndLoad()
    {
        var tmpDir = Path.Combine(Path.GetTempPath(), "obsydian_data_test_" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(tmpDir);
        File.WriteAllText(Path.Combine(tmpDir, "items.json"), """
            [{ "id": "potion", "name": "Health Potion", "damage": 0 }]
            """);

        try
        {
            using var content = new ContentManager(tmpDir);
            content.RegisterLoader(new GameDataLoader<ItemData>());

            var dataLoader = new DataLoader(content);
            dataLoader.Register<GameDataCollection<ItemData>>("items.json");

            Assert.True(dataLoader.IsRegistered<GameDataCollection<ItemData>>());

            var items = dataLoader.Load<GameDataCollection<ItemData>>();
            Assert.Equal(1, items.Count);
            Assert.Equal("Health Potion", items.GetById("potion")!.Name);
        }
        finally
        {
            Directory.Delete(tmpDir, true);
        }
    }

    [Fact]
    public void DataLoader_Reload_GetsNewData()
    {
        var tmpDir = Path.Combine(Path.GetTempPath(), "obsydian_data_test_" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(tmpDir);
        var filePath = Path.Combine(tmpDir, "items.json");
        File.WriteAllText(filePath, """[{ "id": "v1", "name": "Version 1", "damage": 1 }]""");

        try
        {
            using var content = new ContentManager(tmpDir);
            content.RegisterLoader(new GameDataLoader<ItemData>());

            var dataLoader = new DataLoader(content);
            dataLoader.Register<GameDataCollection<ItemData>>("items.json");

            var v1 = dataLoader.Load<GameDataCollection<ItemData>>();
            Assert.Equal("Version 1", v1.GetById("v1")!.Name);

            // Modify file
            File.WriteAllText(filePath, """[{ "id": "v2", "name": "Version 2", "damage": 2 }]""");

            var v2 = dataLoader.Reload<GameDataCollection<ItemData>>();
            Assert.Equal("Version 2", v2.GetById("v2")!.Name);
        }
        finally
        {
            Directory.Delete(tmpDir, true);
        }
    }
}
