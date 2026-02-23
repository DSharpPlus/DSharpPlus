namespace DSharpPlus.Voice;

/// <summary>
/// Indicates how audio from a user should be treated by the library.
/// </summary>
public enum AudioReceiveMode
{
    /// <summary>
    /// Discard audio from the specified user.
    /// </summary>
    Discard,
    
    /// <summary>
    /// Audio from this user will be processed immediately.
    /// </summary>
    Process,
    
    /// <summary>
    /// Instructs the library to retain audio from this user for future processing.
    /// </summary>
    Retain
}
