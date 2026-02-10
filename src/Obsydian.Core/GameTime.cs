namespace Obsydian.Core;

/// <summary>
/// Tracks elapsed time, delta time, and frame count for the game loop.
/// </summary>
public sealed class GameTime
{
    private long _startTicks;
    private long _previousTicks;

    /// <summary>Time in seconds since the last frame.</summary>
    public float DeltaTime { get; private set; }

    /// <summary>Total elapsed time in seconds since the engine started.</summary>
    public double TotalTime { get; private set; }

    /// <summary>Total number of frames rendered.</summary>
    public long FrameCount { get; private set; }

    /// <summary>Current frames per second (smoothed).</summary>
    public float Fps { get; private set; }

    private int _fpsFrameCount;
    private float _fpsTimer;

    public void Start()
    {
        _startTicks = Environment.TickCount64;
        _previousTicks = _startTicks;
    }

    public void Tick()
    {
        var now = Environment.TickCount64;
        DeltaTime = (now - _previousTicks) / 1000f;
        TotalTime = (now - _startTicks) / 1000.0;
        _previousTicks = now;
        FrameCount++;

        // Smooth FPS calculation
        _fpsTimer += DeltaTime;
        _fpsFrameCount++;
        if (_fpsTimer >= 1f)
        {
            Fps = _fpsFrameCount / _fpsTimer;
            _fpsTimer = 0;
            _fpsFrameCount = 0;
        }
    }
}
