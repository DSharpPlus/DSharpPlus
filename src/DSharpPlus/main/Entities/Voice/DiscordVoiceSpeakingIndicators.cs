using System;

namespace DSharpPlus.Entities;

[Flags]
public enum DiscordVoiceSpeakingIndicators
{
    /// <summary>
    /// Normal transmission of voice audio.
    /// </summary>
    Microphone = 1 << 0,

    /// <summary>
    /// Transmission of context audio for video, no speaking indicator.
    /// </summary>
    Soundshare = 1 << 1,

    /// <summary>
    /// Priority speaker, lowering audio of other speakers.
    /// </summary>
    Priority = 1 << 2
}
