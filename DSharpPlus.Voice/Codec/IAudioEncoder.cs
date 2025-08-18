using System;

using DSharpPlus.Voice.RuntimeServices.Memory;

namespace DSharpPlus.Voice.Codec;

/// <summary>
/// Represents a mechanism to encode audio for the current connection. Encoders are not thread-safe.
/// </summary>
public interface IAudioEncoder : IDisposable
{
    /// <summary>
    /// Encodes the provided PCM data.
    /// </summary>
    /// <param name="pcm">The 48khz, dual-channel s16le PCM data to encode.</param>
    /// <param name="sequence">The sequence number of this packet.</param>
    /// <param name="timestamp">The RTP timestamp for this packet.</param>
    /// <param name="written">The amount of <b>elements</b> (not samples!) consumed from the provided PCM data.</param>
    /// <returns>A leased buffer containing the RTP header and encoded audio.</returns>
    public AudioBufferLease Encode(ReadOnlySpan<short> pcm, ushort sequence, uint timestamp, out int written);

    /// <summary>
    /// Writes a silence frame.
    /// </summary>
    /// <param name="sequence">The sequence number of this packet.</param>
    /// <param name="timestamp">The RTP timestamp for this packet.</param>
    public AudioBufferLease WriteSilenceFrame(ushort sequence, uint timestamp);

    /// <summary>
    /// Sets what kind of audio this encoder is being used for.
    /// </summary>
    public void SetAudioType(AudioType audioType);
}
