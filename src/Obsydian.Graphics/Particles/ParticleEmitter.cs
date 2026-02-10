using Obsydian.Core.Math;

namespace Obsydian.Graphics.Particles;

/// <summary>
/// A single particle in the system.
/// </summary>
public struct Particle
{
    public Vec2 Position;
    public Vec2 Velocity;
    public float Life;
    public float MaxLife;
    public float Rotation;
    public float RotationSpeed;
    public float Scale;
    public float ScaleEnd;
    public Color ColorStart;
    public Color ColorEnd;
    public bool Active;
}

/// <summary>
/// Configuration for particle emission behavior.
/// </summary>
public sealed class ParticleConfig
{
    public Vec2 EmitOffset { get; set; }
    public float EmitRadius { get; set; }
    public float EmitRate { get; set; } = 10f;
    public int BurstCount { get; set; }

    public float LifeMin { get; set; } = 0.5f;
    public float LifeMax { get; set; } = 2f;

    public float SpeedMin { get; set; } = 20f;
    public float SpeedMax { get; set; } = 60f;

    public float AngleMin { get; set; }
    public float AngleMax { get; set; } = 360f;

    public float ScaleStart { get; set; } = 1f;
    public float ScaleEnd { get; set; } = 0f;

    public float RotationSpeedMin { get; set; }
    public float RotationSpeedMax { get; set; }

    public Color ColorStart { get; set; } = Color.White;
    public Color ColorEnd { get; set; } = Color.Transparent;

    public Vec2 Gravity { get; set; }

    public bool Looping { get; set; } = true;
}

/// <summary>
/// 2D particle emitter. Spawns, updates, and renders particles based on configuration.
/// Supports continuous emission, bursts, gravity, color/scale interpolation.
/// </summary>
public sealed class ParticleEmitter
{
    public Vec2 Position { get; set; }
    public ParticleConfig Config { get; }
    public bool IsEmitting { get; set; } = true;
    public Texture? Texture { get; set; }

    private readonly Particle[] _particles;
    private readonly Random _rng = new();
    private float _emitAccumulator;
    private int _activeCount;

    public int ActiveCount => _activeCount;

    public ParticleEmitter(ParticleConfig config, int maxParticles = 256)
    {
        Config = config;
        _particles = new Particle[maxParticles];
    }

    /// <summary>Emit a burst of particles immediately.</summary>
    public void Burst(int count)
    {
        for (int i = 0; i < count; i++)
            Emit();
    }

    public void Update(float deltaTime)
    {
        // Continuous emission
        if (IsEmitting && Config.EmitRate > 0)
        {
            _emitAccumulator += deltaTime * Config.EmitRate;
            while (_emitAccumulator >= 1f)
            {
                _emitAccumulator -= 1f;
                Emit();
            }
        }

        // Update active particles
        _activeCount = 0;
        for (int i = 0; i < _particles.Length; i++)
        {
            ref var p = ref _particles[i];
            if (!p.Active) continue;

            p.Life -= deltaTime;
            if (p.Life <= 0)
            {
                p.Active = false;
                continue;
            }

            p.Velocity += Config.Gravity * deltaTime;
            p.Position += p.Velocity * deltaTime;
            p.Rotation += p.RotationSpeed * deltaTime;
            _activeCount++;
        }
    }

    public void Draw(IRenderer renderer)
    {
        for (int i = 0; i < _particles.Length; i++)
        {
            ref var p = ref _particles[i];
            if (!p.Active) continue;

            float t = 1f - (p.Life / p.MaxLife);
            var color = Color.Lerp(p.ColorStart, p.ColorEnd, t);
            float scale = p.Scale + (p.ScaleEnd - p.Scale) * t;

            if (Texture is not null)
            {
                renderer.DrawSprite(Texture, p.Position, null, new Vec2(scale, scale), p.Rotation, color);
            }
            else
            {
                float size = System.Math.Max(1, 4 * scale);
                renderer.DrawRect(Rect.FromCenter(p.Position, new Vec2(size, size)), color);
            }
        }
    }

    private void Emit()
    {
        // Find inactive slot
        for (int i = 0; i < _particles.Length; i++)
        {
            if (_particles[i].Active) continue;

            ref var p = ref _particles[i];
            float angle = Lerp(Config.AngleMin, Config.AngleMax, (float)_rng.NextDouble()) * MathF.PI / 180f;
            float speed = Lerp(Config.SpeedMin, Config.SpeedMax, (float)_rng.NextDouble());

            p.Active = true;
            p.Life = Lerp(Config.LifeMin, Config.LifeMax, (float)_rng.NextDouble());
            p.MaxLife = p.Life;
            p.Position = Position + Config.EmitOffset + RandomInRadius(Config.EmitRadius);
            p.Velocity = new Vec2(MathF.Cos(angle) * speed, MathF.Sin(angle) * speed);
            p.Scale = Config.ScaleStart;
            p.ScaleEnd = Config.ScaleEnd;
            p.Rotation = 0;
            p.RotationSpeed = Lerp(Config.RotationSpeedMin, Config.RotationSpeedMax, (float)_rng.NextDouble());
            p.ColorStart = Config.ColorStart;
            p.ColorEnd = Config.ColorEnd;
            return;
        }
    }

    private Vec2 RandomInRadius(float radius)
    {
        if (radius <= 0) return Vec2.Zero;
        float a = (float)(_rng.NextDouble() * System.Math.PI * 2);
        float r = (float)(_rng.NextDouble() * radius);
        return new Vec2(MathF.Cos(a) * r, MathF.Sin(a) * r);
    }

    private static float Lerp(float a, float b, float t) => a + (b - a) * t;
}
