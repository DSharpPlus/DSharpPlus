namespace DSharpPlus.Voice.MemoryServices.Channels;

/// <summary>
/// Provides meta-information about the current state of an audio channel.
/// </summary>
public enum AudioChannelState
{
    /// <summary>
    /// The channel is open and can process audio.
    /// </summary>
    Open,

    /// <summary>
    /// Transmission is currently paused, but may resume later.
    /// </summary>
    Paused,

    /// <summary>
    /// Transmission is terminated and no more audio will be accepted.
    /// </summary>
    Terminated,
}
