using Obsydian.Audio;
using Obsydian.Core.Logging;
using Silk.NET.OpenAL;

namespace Obsydian.Platform.Desktop.Audio;

/// <summary>
/// IAudioEngine implementation using OpenAL via Silk.NET.
/// Manages device, context, buffers, and sources for sound/music playback.
/// </summary>
public sealed class OpenAlAudioEngine : IAudioEngine
{
    private readonly string _contentRoot;
    private AL? _al;
    private ALContext? _alc;
    private unsafe Device* _device;
    private unsafe Context* _context;

    private readonly Dictionary<string, uint> _bufferCache = [];
    private readonly List<uint> _activeSources = [];
    private uint _musicSource;
    private bool _musicPlaying;

    private float _masterVolume = 1f;
    private float _soundVolume = 1f;
    private float _musicVolume = 1f;

    private int _nextSoundId;
    private int _nextMusicId;
    private bool _initialized;

    public OpenAlAudioEngine(string contentRoot)
    {
        _contentRoot = contentRoot;
    }

    public unsafe void Initialize()
    {
        if (_initialized) return;

        try
        {
            _alc = ALContext.GetApi(true);
            _device = _alc.OpenDevice("");
            if (_device == null)
            {
                Log.Warn("Audio", "No audio device found — audio disabled.");
                return;
            }

            _context = _alc.CreateContext(_device, null);
            if (_context == null)
            {
                Log.Warn("Audio", "Failed to create OpenAL context.");
                return;
            }
            _alc.MakeContextCurrent(_context);

            _al = AL.GetApi(true);

            _al.SetListenerProperty(ListenerFloat.Gain, 1f);

            // Create dedicated music source
            _musicSource = _al.GenSource();
            _al.SetSourceProperty(_musicSource, SourceFloat.Gain, _musicVolume * _masterVolume);

            _initialized = true;
            Log.Info("Audio", "OpenAL initialized.");
        }
        catch (Exception ex)
        {
            Log.Warn("Audio", $"OpenAL init failed: {ex.Message} — audio disabled.");
        }
    }

    public SoundHandle PlaySound(string assetPath, float volume = 1f, float pitch = 0f, bool loop = false)
    {
        if (_al is null) return SoundHandle.None;

        var buffer = LoadOrGetBuffer(assetPath);
        if (buffer == 0) return SoundHandle.None;

        var source = _al.GenSource();
        _al.SetSourceProperty(source, SourceInteger.Buffer, (int)buffer);
        _al.SetSourceProperty(source, SourceFloat.Gain, volume * _soundVolume * _masterVolume);
        _al.SetSourceProperty(source, SourceFloat.Pitch, 1f + pitch);
        _al.SetSourceProperty(source, SourceBoolean.Looping, loop);
        _al.SourcePlay(source);

        _activeSources.Add(source);
        return new SoundHandle(++_nextSoundId);
    }

    public MusicHandle PlayMusic(string assetPath, float volume = 1f, bool loop = true)
    {
        if (_al is null) return MusicHandle.None;

        // Stop current music
        if (_musicPlaying)
        {
            _al.SourceStop(_musicSource);
            _musicPlaying = false;
        }

        var buffer = LoadOrGetBuffer(assetPath);
        if (buffer == 0) return MusicHandle.None;

        _al.SetSourceProperty(_musicSource, SourceInteger.Buffer, (int)buffer);
        _al.SetSourceProperty(_musicSource, SourceFloat.Gain, volume * _musicVolume * _masterVolume);
        _al.SetSourceProperty(_musicSource, SourceBoolean.Looping, loop);
        _al.SourcePlay(_musicSource);
        _musicPlaying = true;

        return new MusicHandle(++_nextMusicId);
    }

    public void StopAll()
    {
        if (_al is null) return;

        foreach (var source in _activeSources)
        {
            _al.SourceStop(source);
            _al.DeleteSource(source);
        }
        _activeSources.Clear();

        if (_musicPlaying)
        {
            _al.SourceStop(_musicSource);
            _musicPlaying = false;
        }
    }

    public void SetMasterVolume(float volume)
    {
        _masterVolume = Math.Clamp(volume, 0f, 1f);
        UpdateMusicGain();
    }

    public void SetSoundVolume(float volume)
    {
        _soundVolume = Math.Clamp(volume, 0f, 1f);
    }

    public void SetMusicVolume(float volume)
    {
        _musicVolume = Math.Clamp(volume, 0f, 1f);
        UpdateMusicGain();
    }

    public void Update()
    {
        if (_al is null) return;

        // Clean up finished sound sources
        for (int i = _activeSources.Count - 1; i >= 0; i--)
        {
            _al.GetSourceProperty(_activeSources[i], GetSourceInteger.SourceState, out int state);
            if (state == (int)SourceState.Stopped)
            {
                _al.DeleteSource(_activeSources[i]);
                _activeSources.RemoveAt(i);
            }
        }
    }

    private uint LoadOrGetBuffer(string assetPath)
    {
        if (_al is null) return 0;

        if (_bufferCache.TryGetValue(assetPath, out var cached))
            return cached;

        var fullPath = Path.Combine(_contentRoot, assetPath);
        if (!File.Exists(fullPath))
        {
            Log.Warn("Audio", $"Audio file not found: {fullPath}");
            return 0;
        }

        try
        {
            var wav = WavReader.Read(fullPath);
            var format = (wav.Channels, wav.BitsPerSample) switch
            {
                (1, 8) => BufferFormat.Mono8,
                (1, 16) => BufferFormat.Mono16,
                (2, 8) => BufferFormat.Stereo8,
                (2, 16) => BufferFormat.Stereo16,
                _ => throw new NotSupportedException($"Unsupported WAV format: {wav.Channels}ch {wav.BitsPerSample}bit")
            };

            var buffer = _al.GenBuffer();
            unsafe
            {
                fixed (byte* ptr = wav.PcmData)
                {
                    _al.BufferData(buffer, format, ptr, wav.PcmData.Length, wav.SampleRate);
                }
            }

            _bufferCache[assetPath] = buffer;
            Log.Debug("Audio", $"Loaded audio: {assetPath} ({wav.Channels}ch, {wav.SampleRate}Hz, {wav.BitsPerSample}bit)");
            return buffer;
        }
        catch (Exception ex)
        {
            Log.Warn("Audio", $"Failed to load {assetPath}: {ex.Message}");
            return 0;
        }
    }

    private void UpdateMusicGain()
    {
        if (_al is null || !_musicPlaying) return;
        _al.SetSourceProperty(_musicSource, SourceFloat.Gain, _musicVolume * _masterVolume);
    }

    public unsafe void Dispose()
    {
        if (!_initialized) return;

        StopAll();

        if (_al is not null)
        {
            _al.DeleteSource(_musicSource);
            foreach (var buffer in _bufferCache.Values)
                _al.DeleteBuffer(buffer);
            _bufferCache.Clear();
        }

        if (_alc is not null)
        {
            _alc.MakeContextCurrent(null);
            if (_context != null) _alc.DestroyContext(_context);
            if (_device != null) _alc.CloseDevice(_device);
        }

        _al?.Dispose();
        _alc?.Dispose();
        _initialized = false;
        Log.Info("Audio", "OpenAL shutdown.");
    }
}
