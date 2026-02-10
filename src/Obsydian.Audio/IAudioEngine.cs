namespace Obsydian.Audio;

/// <summary>
/// Audio engine interface for playing sounds and music.
/// </summary>
public interface IAudioEngine : IDisposable
{
    void Initialize();

    SoundHandle PlaySound(string assetPath, float volume = 1f, float pitch = 0f, bool loop = false);
    MusicHandle PlayMusic(string assetPath, float volume = 1f, bool loop = true);

    void StopAll();
    void SetMasterVolume(float volume);
    void SetSoundVolume(float volume);
    void SetMusicVolume(float volume);

    void Update();
}

public readonly record struct SoundHandle(int Id)
{
    public static readonly SoundHandle None = new(-1);
}

public readonly record struct MusicHandle(int Id)
{
    public static readonly MusicHandle None = new(-1);
}
