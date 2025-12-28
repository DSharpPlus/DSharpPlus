namespace DSharpPlus.Voice.Codec;

/// <summary>
/// Specifies a type of audio to optimize for.
/// </summary>
public enum AudioType
{
    /// <summary>
    /// Let the library decide what it thinks is best.
    /// </summary>
    Auto,

    /// <summary>
    /// Optimize for sending voice.
    /// </summary>
    Voice,

    /// <summary>
    /// Optimize for sending music.
    /// </summary>
    Music
}
