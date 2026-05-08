using System;

using DSharpPlus.Voice.Codec;
using DSharpPlus.Voice.Receivers;

namespace DSharpPlus.Voice;

/// <summary>
/// Provides per-connection behavioural configuration.
/// </summary>
public sealed class VoiceConnectionOptions
{
    /// <summary>
    /// Indicates whether the bot should mute itself. Defaults to false.
    /// </summary>
    public bool SelfMute { get; set; } = false;

    /// <summary>
    /// Indicates whether the bot should deafen itself. Defaults to false.
    /// </summary>
    public bool SelfDeafen { get; set; } = false;

    /// <summary>
    /// Indicates how the library should generally treat sending audio.
    /// </summary>
    public AudioType AudioType { get; set; } = AudioType.Auto;

    /// <summary>
    /// Indicates whether the library should pause transmission of audio when the bot is alone in a voice channel.
    /// </summary>
    public bool PauseTransmissionIfAlone { get; set; } = false;

    /// <summary>
    /// Sets up the audio receiver, if one was specified.
    /// </summary>
    public Action<AudioReceiver>? ReceiverSetup { get; set; }
}
