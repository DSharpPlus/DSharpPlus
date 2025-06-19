using System;

namespace DSharpPlus.Voice.Protocol.DiscordRtp;

/// <summary>
/// Describes the layout of a discord-flavoured RTP frame. The frame is not stored along with this type.
/// </summary>
internal readonly record struct DiscordRtpFrameInfo
{
    /// <summary>
    /// Gets a range specifying the size of the header.
    /// </summary>
    public Range Header { get; init; }

    /// <summary>
    /// Gets a range specifying the size and location of the audio data.
    /// </summary>
    public Range VoiceData { get; init; }

    /// <summary>
    /// Gets a range specifying the size and location of the nonce.
    /// </summary>
    public Range Nonce { get; init; }

    /// <summary>
    /// Gets the length of the extension header, which is part of the encrypted voice data but must be stripped before decoding.
    /// </summary>
    public int ExtensionHeaderLength { get; init; }
}
