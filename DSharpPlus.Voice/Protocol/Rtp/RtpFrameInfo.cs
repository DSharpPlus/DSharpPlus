using System;

namespace DSharpPlus.Voice.Protocol.Rtp;

/// <summary>
/// Describes the layout of a discord-flavoured RTP frame. The frame is not stored along with this type.
/// </summary>
public readonly record struct RtpFrameInfo
{
    /// <summary>
    /// Gets a range specifying the size of the header.
    /// </summary>
    public Range Header { get; internal init; }

    /// <summary>
    /// Gets a range specifying the size and location of the audio data.
    /// </summary>
    public Range VoiceData { get; internal init; }

    /// <summary>
    /// Gets a range specifying the size and location of the nonce.
    /// </summary>
    public Range Nonce { get; internal init; }

    /// <summary>
    /// Gets the length of the extension header, which is part of the encrypted voice data but must be stripped before decoding.
    /// </summary>
    public int ExtensionHeaderLength { get; internal init; }

    /// <summary>
    /// Gets the SSRC of the user who sent this frame.
    /// </summary>
    public uint Ssrc { get; internal init; }

    /// <summary>
    /// Gets the timestamp associated with this frame.
    /// </summary>
    public uint Timestamp { get; internal init; }

    /// <summary>
    /// Gets the sequence number of this frame.
    /// </summary>
    public ushort Sequence { get; internal init; }
}
