namespace Obsydian.Platform.Desktop.Audio;

/// <summary>
/// Reads WAV file data for OpenAL playback. Supports 8-bit and 16-bit PCM, mono and stereo.
/// </summary>
public static class WavReader
{
    public readonly record struct WavData(byte[] PcmData, int Channels, int BitsPerSample, int SampleRate);

    public static WavData Read(string path)
    {
        using var stream = File.OpenRead(path);
        using var reader = new BinaryReader(stream);

        // RIFF header
        var riff = new string(reader.ReadChars(4));
        if (riff != "RIFF")
            throw new InvalidDataException($"Not a WAV file: {path}");

        reader.ReadInt32(); // chunk size
        var wave = new string(reader.ReadChars(4));
        if (wave != "WAVE")
            throw new InvalidDataException($"Not a WAV file: {path}");

        // Find fmt and data chunks
        int channels = 0, sampleRate = 0, bitsPerSample = 0;
        byte[]? pcmData = null;

        while (stream.Position < stream.Length)
        {
            var chunkId = new string(reader.ReadChars(4));
            var chunkSize = reader.ReadInt32();

            switch (chunkId)
            {
                case "fmt ":
                    var audioFormat = reader.ReadInt16(); // 1 = PCM
                    if (audioFormat != 1)
                        throw new NotSupportedException($"Only PCM WAV supported, got format {audioFormat}");
                    channels = reader.ReadInt16();
                    sampleRate = reader.ReadInt32();
                    reader.ReadInt32(); // byte rate
                    reader.ReadInt16(); // block align
                    bitsPerSample = reader.ReadInt16();
                    // Skip any extra fmt bytes
                    if (chunkSize > 16)
                        reader.ReadBytes(chunkSize - 16);
                    break;

                case "data":
                    pcmData = reader.ReadBytes(chunkSize);
                    break;

                default:
                    // Skip unknown chunks
                    reader.ReadBytes(chunkSize);
                    break;
            }
        }

        if (pcmData is null)
            throw new InvalidDataException($"No data chunk found in WAV: {path}");

        return new WavData(pcmData, channels, bitsPerSample, sampleRate);
    }
}
