using System;

using DSharpPlus.Voice.MemoryServices;

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
    /// <param name="consumed">The amount of <b>elements</b> (not samples!) consumed from the provided PCM data.</param>
    /// <returns>A leased buffer containing the RTP header and encoded audio.</returns>
    public AudioBufferLease Encode(ReadOnlySpan<Int16x2> pcm, out int consumed);

    /// <summary>
    /// Encodes the provided PCM data.
    /// </summary>
    /// <param name="pcm">The 48khz, dual-channel s32le PCM data to encode.</param>
    /// <param name="consumed">The amount of <b>elements</b> (not samples!) consumed from the provided PCM data.</param>
    /// <returns>A leased buffer containing the RTP header and encoded audio.</returns>
    public AudioBufferLease Encode(ReadOnlySpan<Int32x2> pcm, out int consumed);

    /// <summary>
    /// Encodes the provided PCM data.
    /// </summary>
    /// <param name="pcm">The 48khz, dual-channel float32 le PCM data to encode.</param>
    /// <param name="consumed">The amount of <b>elements</b> (not samples!) consumed from the provided PCM data.</param>
    /// <returns>A leased buffer containing the RTP header and encoded audio.</returns>
    public AudioBufferLease Encode(ReadOnlySpan<Singlex2> pcm, out int consumed);

    /// <summary>
    /// Writes a silence frame.
    /// </summary>
    public AudioBufferLease WriteSilenceFrame();

    /// <summary>
    /// Sets what kind of audio this encoder is being used for.
    /// </summary>
    public void SetAudioType(AudioType audioType);

    /// <summary>
    /// Gets the maximum amount of samples this encoder consumes for a single packet.
    /// </summary>
    public int SamplesPerPacket { get; }
}
