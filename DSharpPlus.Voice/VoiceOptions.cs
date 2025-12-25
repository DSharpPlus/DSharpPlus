namespace DSharpPlus.Voice;

/// <summary>
/// Contains settings to control the behaviour of the voice extension.
/// </summary>
public sealed class VoiceOptions
{
    /// <summary>
    /// Indicates whether to log debugging messages from native MLS code. Defaults to false.
    /// </summary>
    /// <remarks>
    /// Please note that native code doesn't log what session a message was sent from, so any logs sent through this
    /// mechanism are fairly useless if the bot is in two voice connections at once.
    /// </remarks>
    public bool LogNativeMlsDebugMessages { get; set; } = false;
}
