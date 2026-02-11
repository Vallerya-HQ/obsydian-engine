namespace Obsydian.Audio;

/// <summary>
/// Platform-agnostic decoded audio data. Can be loaded via ContentManager
/// and then handed to IAudioEngine for playback.
/// </summary>
public sealed class AudioClip
{
    public byte[] PcmData { get; }
    public int Channels { get; }
    public int SampleRate { get; }
    public int BitsPerSample { get; }
    public string Name { get; }

    /// <summary>Duration in seconds.</summary>
    public float Duration { get; }

    public AudioClip(byte[] pcmData, int channels, int sampleRate, int bitsPerSample, string name = "")
    {
        PcmData = pcmData;
        Channels = channels;
        SampleRate = sampleRate;
        BitsPerSample = bitsPerSample;
        Name = name;

        int bytesPerSample = bitsPerSample / 8;
        int totalSamples = pcmData.Length / (bytesPerSample * channels);
        Duration = (float)totalSamples / sampleRate;
    }
}
