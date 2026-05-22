namespace DSharpPlus.Voice.Codec;

/// <summary>
/// Specifies a type of audio to optimize for. This is generally best left at <see cref="Auto"/> 
/// </summary>
public enum AudioType
{
    /// <summary>
    /// Let the codec decide what it thinks is best.
    /// </summary>
    Auto,

    /// <summary>
    /// Optimize for streaming voice.
    /// </summary>
    Voice,

    /// <summary>
    /// Optimize for sending music.
    /// </summary>
    Music
}
