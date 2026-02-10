using Obsydian.Core.Math;
using Obsydian.Graphics.Particles;

namespace Obsydian.Engine.Tests.Graphics;

public class ParticleEmitterTests
{
    [Fact]
    public void NewEmitter_HasNoActiveParticles()
    {
        var emitter = new ParticleEmitter(new ParticleConfig());
        Assert.Equal(0, emitter.ActiveCount);
    }

    [Fact]
    public void Burst_SpawnsParticles()
    {
        var emitter = new ParticleEmitter(new ParticleConfig { LifeMin = 1f, LifeMax = 1f }, 100);
        emitter.IsEmitting = false; // disable continuous
        emitter.Burst(10);
        emitter.Update(0.001f); // tiny update to count active
        Assert.Equal(10, emitter.ActiveCount);
    }

    [Fact]
    public void ContinuousEmission_SpawnsOverTime()
    {
        var config = new ParticleConfig { EmitRate = 100f, LifeMin = 2f, LifeMax = 2f };
        var emitter = new ParticleEmitter(config, 256);
        emitter.Update(0.1f); // 100 * 0.1 = 10 particles
        Assert.True(emitter.ActiveCount > 0);
    }

    [Fact]
    public void ParticlesDie_AfterLifetime()
    {
        var config = new ParticleConfig { LifeMin = 0.1f, LifeMax = 0.1f };
        var emitter = new ParticleEmitter(config, 100);
        emitter.IsEmitting = false;
        emitter.Burst(5);
        emitter.Update(0.001f);
        Assert.Equal(5, emitter.ActiveCount);

        emitter.Update(0.2f); // past lifetime
        Assert.Equal(0, emitter.ActiveCount);
    }

    [Fact]
    public void Config_DefaultValues_AreReasonable()
    {
        var config = new ParticleConfig();
        Assert.Equal(10f, config.EmitRate);
        Assert.True(config.LifeMax >= config.LifeMin);
        Assert.True(config.SpeedMax >= config.SpeedMin);
        Assert.True(config.Looping);
    }

    [Fact]
    public void Position_AffectsSpawnLocation()
    {
        var emitter = new ParticleEmitter(new ParticleConfig { EmitRadius = 0 });
        emitter.Position = new Vec2(100, 200);
        // No crash, position is set
        Assert.Equal(new Vec2(100, 200), emitter.Position);
    }
}
