using System.Text.Json;
using Obsydian.Content;
using Obsydian.Core.Logging;
using Obsydian.Core.Math;
using Obsydian.Graphics;
using Obsydian.Graphics.Tilemap;
using Texture = Obsydian.Graphics.Texture;

namespace Obsydian.Platform.Desktop.Content;

/// <summary>
/// Loads Tiled JSON maps (.tmj) into engine Tilemap objects.
/// Handles tile layers, object layers (for spawn/warp data), and tileset resolution.
/// </summary>
public sealed class TiledMapLoader : IAssetLoader<Tilemap>
{
    private readonly ContentManager _content;
    private static readonly JsonSerializerOptions JsonOpts = new() { PropertyNameCaseInsensitive = true };

    public TiledMapLoader(ContentManager content)
    {
        _content = content;
    }

    public Tilemap Load(string fullPath)
    {
        var json = File.ReadAllText(fullPath);
        var mapData = JsonSerializer.Deserialize<TiledMap>(json, JsonOpts)
            ?? throw new InvalidDataException($"Failed to parse Tiled map: {fullPath}");

        var mapDir = Path.GetDirectoryName(fullPath) ?? "";

        // Load tileset texture
        Texture tileset;
        if (mapData.Tilesets is { Count: > 0 })
        {
            var ts = mapData.Tilesets[0];
            var imagePath = ts.Image ?? ts.Source ?? throw new InvalidDataException("Tileset has no image or source");

            // If source is a .tsj reference, load it and get the image path
            if (imagePath.EndsWith(".tsj") || imagePath.EndsWith(".tsx"))
            {
                var tsjPath = Path.Combine(mapDir, imagePath);
                var tsjJson = File.ReadAllText(tsjPath);
                var tsjData = JsonSerializer.Deserialize<TiledTileset>(tsjJson, JsonOpts);
                imagePath = tsjData?.Image ?? throw new InvalidDataException($"Tileset file has no image: {tsjPath}");
                var tsjDir = Path.GetDirectoryName(tsjPath) ?? "";
                imagePath = Path.Combine(tsjDir, imagePath);
            }
            else
            {
                imagePath = Path.Combine(mapDir, imagePath);
            }

            tileset = _content.Load<Texture>(Path.GetRelativePath(_content.RootPath, imagePath));
        }
        else
        {
            throw new InvalidDataException($"No tilesets defined in map: {fullPath}");
        }

        var tilemap = new Tilemap(tileset, mapData.Tilewidth, mapData.Tileheight, mapData.Width, mapData.Height);

        // Process layers
        foreach (var layerData in mapData.Layers ?? [])
        {
            if (layerData.Type == "tilelayer" && layerData.Data is not null)
            {
                var layer = tilemap.AddLayer(layerData.Name ?? "unnamed");
                layer.Visible = layerData.Visible;

                for (int i = 0; i < layerData.Data.Count; i++)
                {
                    int gid = layerData.Data[i];
                    if (gid <= 0) continue;

                    int x = i % mapData.Width;
                    int y = i / mapData.Width;

                    // Strip flip flags from GID (Tiled uses high bits for flipping)
                    bool flipX = (gid & 0x80000000) != 0;
                    bool flipY = (gid & 0x40000000) != 0;
                    int tileId = gid & 0x1FFFFFFF;

                    // Check if tile is solid (via tileset tile properties)
                    bool solid = IsTileSolid(mapData.Tilesets[0], tileId);

                    layer.SetTile(x, y, new Tile(tileId, solid, flipX, flipY));
                }
            }
            else if (layerData.Type == "objectgroup")
            {
                // Object layers stored as metadata on the tilemap
                foreach (var obj in layerData.Objects ?? [])
                {
                    tilemap.SetObjectData(layerData.Name ?? "objects", new TilemapObject(
                        obj.Name ?? "",
                        obj.Type ?? obj.Class ?? "",
                        new Vec2((float)obj.X, (float)obj.Y),
                        new Vec2((float)obj.Width, (float)obj.Height),
                        obj.Properties?.ToDictionary(p => p.Name, p => p.Value?.ToString() ?? "") ?? []
                    ));
                }
            }
        }

        Log.Info("Content", $"Loaded Tiled map: {fullPath} ({mapData.Width}x{mapData.Height}, {tilemap.Layers.Count} layers)");
        return tilemap;
    }

    private static bool IsTileSolid(TiledTileset tileset, int tileId)
    {
        if (tileset.Tiles is null) return false;
        var localId = tileId - (tileset.Firstgid > 0 ? tileset.Firstgid : 1);
        var tileDef = tileset.Tiles.FirstOrDefault(t => t.Id == localId);
        return tileDef?.Properties?.Any(p => p.Name == "solid" && p.Value?.ToString() == "true") ?? false;
    }

    // Tiled JSON data model
    private sealed class TiledMap
    {
        public int Width { get; set; }
        public int Height { get; set; }
        public int Tilewidth { get; set; }
        public int Tileheight { get; set; }
        public List<TiledLayer>? Layers { get; set; }
        public List<TiledTileset>? Tilesets { get; set; }
    }

    private sealed class TiledLayer
    {
        public string? Name { get; set; }
        public string Type { get; set; } = "";
        public bool Visible { get; set; } = true;
        public List<int>? Data { get; set; }
        public List<TiledObject>? Objects { get; set; }
    }

    private sealed class TiledTileset
    {
        public int Firstgid { get; set; }
        public string? Source { get; set; }
        public string? Image { get; set; }
        public int Tilewidth { get; set; }
        public int Tileheight { get; set; }
        public int Columns { get; set; }
        public int Tilecount { get; set; }
        public List<TiledTileDef>? Tiles { get; set; }
    }

    private sealed class TiledTileDef
    {
        public int Id { get; set; }
        public List<TiledProperty>? Properties { get; set; }
    }

    private sealed class TiledObject
    {
        public string? Name { get; set; }
        public string? Type { get; set; }
        public string? Class { get; set; }
        public double X { get; set; }
        public double Y { get; set; }
        public double Width { get; set; }
        public double Height { get; set; }
        public List<TiledProperty>? Properties { get; set; }
    }

    private sealed class TiledProperty
    {
        public string Name { get; set; } = "";
        public object? Value { get; set; }
    }
}
