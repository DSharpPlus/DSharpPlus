namespace DSharpPlus.Voice;

/// <summary>
/// Specifies why we were disconnected from voice. 
/// </summary>
public enum VoiceDisconnectReason
{
    /// <summary>
    /// We don't know why were were disconnected.
    /// </summary>
    Unknown,

    /// <summary>
    /// A disconnect was explicitly initiated by the bot.
    /// </summary>
    Disconnected,

    /// <summary>
    /// A library issue was encountered and the connection was terminated.
    /// </summary>
    LibraryIssue,

    /// <summary>
    /// The voice endpoint was unavailable. 
    /// </summary>
    VoiceEndpointUnavailable,

    /// <summary>
    /// The voice session was invalidated.
    /// </summary>
    SessionInvalid,

    /// <summary>
    /// The bot was ratelimited on attempting to join voice channels.
    /// </summary>
    Ratelimited,

    /// <summary>
    /// The bot was kicked from the voice channel.
    /// </summary>
    Kicked,

    /// <summary>
    /// The voice channel was deleted, terminating the call.
    /// </summary>
    CallTerminated
}