namespace DSharpPlus.Voice.MemoryServices.Channels;

/// <summary>
/// Represents the result of a reading operation from a <see cref="AudioChannelReader"/>. 
/// </summary>
public readonly record struct AudioChannelReadResult
{
        /// <summary>
    /// The current state of this audio channel.
    /// </summary>
    public required AudioChannelState State { get; init; }

    /// <summary>
    /// Indicates whether a read was able to succeed.
    /// </summary>
    public required bool IsAvailable { get; init; }

    /// <summary>
    /// The buffer read from the channel, if one could be read.
    /// </summary>
    public AudioBufferLease? Buffer { get; init; }
}
