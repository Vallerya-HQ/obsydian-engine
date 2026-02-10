using Obsydian.Audio;

namespace Obsydian.Engine.Tests.Audio;

public class AudioTests
{
    [Fact]
    public void AudioData_Properties_MatchConstructor()
    {
        var pcm = new byte[] { 1, 2, 3, 4 };
        var data = new AudioData(pcm, Channels: 2, SampleRate: 44100, BitsPerSample: 16);

        Assert.Same(pcm, data.PcmData);
        Assert.Equal(2, data.Channels);
        Assert.Equal(44100, data.SampleRate);
        Assert.Equal(16, data.BitsPerSample);
    }

    [Fact]
    public void SoundHandle_None_HasNegativeId()
    {
        Assert.Equal(-1, SoundHandle.None.Id);
    }

    [Fact]
    public void MusicHandle_None_HasNegativeId()
    {
        Assert.Equal(-1, MusicHandle.None.Id);
    }

    [Fact]
    public void OggDecoder_FileNotFound_Throws()
    {
        Assert.ThrowsAny<Exception>(() => OggDecoder.Decode("nonexistent_file.ogg"));
    }

    [Fact]
    public void OggStream_FileNotFound_Throws()
    {
        Assert.ThrowsAny<Exception>(() => new OggStream("nonexistent_file.ogg"));
    }
}
