namespace DSharpPlus.Voice.AudioWriters.Ogg;

/// <summary>
/// Provides a general indication of how many audio channels a given ogg/opus track has.
/// </summary>
internal enum OggOpusChannelMappingFamily : byte
{
    /// <summary>
    /// The input audio was mono or stereo audio, we can just pass it to Discord without issue.
    /// This is the only channel mapping type DSharpPlus supports.
    /// </summary>
    Basic = 0,

    /// <summary>
    /// The input audio has 1-8 channels.
    /// </summary>
    Complex = 1
}
