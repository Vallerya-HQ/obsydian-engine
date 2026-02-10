using NVorbis;

namespace Obsydian.Audio;

/// <summary>
/// Decoded audio data ready for playback.
/// </summary>
public sealed record AudioData(byte[] PcmData, int Channels, int SampleRate, int BitsPerSample);

/// <summary>
/// OGG/Vorbis decoder using NVorbis. Supports full decode for SFX and streaming for music.
/// </summary>
public static class OggDecoder
{
    /// <summary>
    /// Fully decode an OGG file into PCM 16-bit data. Suitable for short sound effects.
    /// </summary>
    public static AudioData Decode(string path)
    {
        using var reader = new VorbisReader(path);
        int channels = reader.Channels;
        int sampleRate = reader.SampleRate;
        int totalSamples = (int)reader.TotalSamples * channels;

        var floatBuffer = new float[totalSamples];
        int read = reader.ReadSamples(floatBuffer, 0, totalSamples);

        // Convert float [-1,1] to PCM 16-bit
        var pcm = new byte[read * 2];
        for (int i = 0; i < read; i++)
        {
            short sample = (short)(System.Math.Clamp(floatBuffer[i], -1f, 1f) * short.MaxValue);
            pcm[i * 2] = (byte)(sample & 0xFF);
            pcm[i * 2 + 1] = (byte)((sample >> 8) & 0xFF);
        }

        return new AudioData(pcm, channels, sampleRate, 16);
    }
}
