namespace DSharpPlus.Voice.Receivers;

/// <summary>
/// Indicates how audio from a user should be treated by the library.
/// </summary>
public enum UserAudioReceiveMode
{
    /// <summary>
    /// Discard audio from the specified user.
    /// </summary>
    Discard,
    
    /// <summary>
    /// Audio from this user will be processed immediately.
    /// </summary>
    Process
}
