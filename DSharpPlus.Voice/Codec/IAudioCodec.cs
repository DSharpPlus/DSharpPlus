using System;

namespace DSharpPlus.Voice.Codec;

/// <summary>
/// Provides a mechanism to obtain audio encoders and decoders.
/// </summary>
public interface IAudioCodec
{
    /// <summary>
    /// Creates a new encoder for a connection.
    /// </summary>
    /// <param name="bitrate">The bitrate permitted for this connection.</param>
    /// <param name="type">The starting audio type for this connection.</param>
    public IAudioEncoder CreateEncoder(int bitrate, AudioType type);

    /// <summary>
    /// Gets the created encoder for this connection.
    /// </summary>
    public IAudioEncoder GetEncoder();

    /// <summary>
    /// Creates a new decoder for a connection.
    /// </summary>
    public IAudioDecoder CreateDecoder();

    /// <summary>
    /// Calculates the length a packet will run for.
    /// </summary>
    public TimeSpan CalculatePacketLength(ReadOnlySpan<byte> packet);

    /// <summary>
    /// Gets the idiomatic silence frame for this codec.
    /// </summary>
    public ReadOnlySpan<byte> SilenceFrame { get; }
}
