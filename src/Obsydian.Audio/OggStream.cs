using NVorbis;

namespace Obsydian.Audio;

/// <summary>
/// Streaming OGG/Vorbis decoder for music. Decodes chunks on demand
/// to avoid loading the entire file into memory.
/// </summary>
public sealed class OggStream : IDisposable
{
    private readonly VorbisReader _reader;

    public int Channels => _reader.Channels;
    public int SampleRate => _reader.SampleRate;
    public bool IsEndOfStream => _reader.IsEndOfStream;

    public OggStream(string path)
    {
        _reader = new VorbisReader(path);
    }

    /// <summary>
    /// Read the next chunk of samples and convert to PCM 16-bit.
    /// Returns the number of bytes written to the buffer.
    /// </summary>
    public int ReadSamples(byte[] pcmBuffer, int maxBytes)
    {
        int maxSamples = maxBytes / 2; // 16-bit = 2 bytes per sample
        var floatBuffer = new float[maxSamples];
        int read = _reader.ReadSamples(floatBuffer, 0, maxSamples);

        for (int i = 0; i < read; i++)
        {
            short sample = (short)(System.Math.Clamp(floatBuffer[i], -1f, 1f) * short.MaxValue);
            pcmBuffer[i * 2] = (byte)(sample & 0xFF);
            pcmBuffer[i * 2 + 1] = (byte)((sample >> 8) & 0xFF);
        }

        return read * 2;
    }

    /// <summary>
    /// Seek back to the beginning for looping.
    /// </summary>
    public void Reset()
    {
        _reader.SeekTo(0);
    }

    public void Dispose()
    {
        _reader.Dispose();
    }
}
