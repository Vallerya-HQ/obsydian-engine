# Obsydian Engine

Modular ECS-based 2D game engine in C# / .NET 9. Built for Vallerya.

## Modules

| Module | Description |
|--------|-------------|
| **Core** | ECS (World, Entity, ComponentStore), math (Vec2, Rect, Color), events, logging, time |
| **Graphics** | IRenderer, SpriteBatch, BitmapFont, SpriteSheet, TextureAtlas, Animation, Camera2D, Tilemap, RenderLayers, Particles, Screen Transitions |
| **Input** | Keyboard, mouse, gamepad state tracking, ActionMap input abstraction with rebinding |
| **Audio** | IAudioEngine interface, OGG/Vorbis decoding via NVorbis, streaming playback |
| **Physics** | AABB collision, TileCollision resolver, SpatialHashGrid broad phase |
| **Content** | ContentManager with sync/async loading, IAssetLoader pipeline, AsyncContentQueue |
| **UI** | UIElement hierarchy, StackContainer, GridContainer, AnchorContainer, dialogue system, debug overlay |
| **Serialization** | Save/load with versioned migration chain |
| **Platform** | Platform abstraction layer |
| **Platform.Desktop** | Silk.NET integration (OpenGL 3.3, OpenAL, GLFW windowing, gamepad bridge) |
| **Network** | Stub (future) |

## Build & Test

```bash
dotnet build
dotnet test    # 145 tests across 3 test suites
```

## Architecture

- **ECS over inheritance** — Components are `record struct : IComponent`, systems extend `GameSystem`
- **Dense-array ComponentStore** — Zero-boxing, cache-friendly iteration via `AsSpan()`
- **Platform.Desktop never leaks into core** — only the platform layer depends on Silk.NET
- **Spatial hash grid** for broad-phase collision (O(1) insert/query)
- **Batched text rendering** — single draw call per string via `DrawTextBatch`
- **Texture atlas packing** — shelf/row algorithm for runtime sprite atlas generation

## Dependencies

- .NET 9 / C# 13
- Silk.NET 2.21.0 (windowing, OpenGL, OpenAL, input)
- StbImageSharp 2.27.14 (texture loading)
- NVorbis 0.10.5 (OGG/Vorbis decoding)
- xUnit 2.9.3 (testing)

## License

See [LICENSE](LICENSE).
