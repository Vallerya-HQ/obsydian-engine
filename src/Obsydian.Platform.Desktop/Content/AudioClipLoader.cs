using Obsydian.Audio;
using Obsydian.Content;
using Obsydian.Core.Logging;
using Obsydian.Platform.Desktop.Audio;

namespace Obsydian.Platform.Desktop.Content;

/// <summary>
/// Loads WAV and OGG audio files into AudioClip objects via ContentManager.
/// Decodes fully into PCM — suitable for sound effects.
/// For long music tracks, consider streaming via OggStream directly.
/// </summary>
public sealed class AudioClipLoader : IAssetLoader<AudioClip>
{
    public AudioClip Load(string fullPath)
    {
        if (!File.Exists(fullPath))
            throw new FileNotFoundException($"Audio file not found: {fullPath}");

        var ext = Path.GetExtension(fullPath).ToLowerInvariant();
        var name = Path.GetFileNameWithoutExtension(fullPath);

        if (ext == ".ogg")
        {
            var decoded = OggDecoder.Decode(fullPath);
            Log.Debug("AudioClipLoader", $"Loaded OGG: {fullPath} ({decoded.Channels}ch, {decoded.SampleRate}Hz)");
            return new AudioClip(decoded.PcmData, decoded.Channels, decoded.SampleRate, decoded.BitsPerSample, name);
        }
        else
        {
            var wav = WavReader.Read(fullPath);
            Log.Debug("AudioClipLoader", $"Loaded WAV: {fullPath} ({wav.Channels}ch, {wav.SampleRate}Hz)");
            return new AudioClip(wav.PcmData, wav.Channels, wav.SampleRate, wav.BitsPerSample, name);
        }
    }
}
