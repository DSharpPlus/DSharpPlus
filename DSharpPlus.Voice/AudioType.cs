namespace DSharpPlus.Voice;

/// <summary>
/// Specifies a type of audio to optimize for. This is generally best left at <see cref="Auto"/>.
/// </summary>
public enum AudioType
{
    /// <summary>
    /// Let the codec decide what it thinks is best.
    /// </summary>
    Auto,

    /// <summary>
    /// Optimize for streaming real-time audio.
    /// </summary>
    Realtime,

    /// <summary>
    /// Optimize for sending pre-collected audio.
    /// </summary>
    Music
}
