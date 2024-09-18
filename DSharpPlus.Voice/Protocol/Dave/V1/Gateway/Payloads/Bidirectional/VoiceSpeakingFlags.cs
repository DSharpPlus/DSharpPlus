namespace DSharpPlus.Voice.Protocol.Dave.V1.Gateway.Payloads.Bidirectional;

/// <summary>
/// The following flags can be used to control how the application is able to speak.
/// </summary>
internal enum VoiceSpeakingFlags
{
    /// <summary>
    /// Disables transmitting voice from the application to Discord.
    /// </summary>
    None = 0,

    /// <summary>
    /// Enables standard transmission of voice audio.
    /// </summary>
    Microphone = 1 << 0,

    /// <summary>
    /// Enables transmitting context audio for video without speaking indicator.
    /// </summary>
    Soundshare = 1 << 1,

    /// <summary>
    /// Enables priority speaker mode, lowering the audio volume of other speakers while the application is speaking.
    /// </summary>
    Priority = 1 << 2
}
