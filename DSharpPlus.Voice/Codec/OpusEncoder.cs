using System;
using System.Buffers;
using System.Diagnostics;

using DSharpPlus.Voice.Interop.Opus;
using DSharpPlus.Voice.MemoryServices;

namespace DSharpPlus.Voice.Codec;

/// <summary>
/// Provides an encoder for the opus codec.
/// </summary>
public sealed class OpusEncoder : IAudioEncoder
{
    private readonly AudioBufferPool pool;
    private readonly OpusEncoderHandle encoder;
    private readonly OpusRepacketizerHandle repacketizer;

    private AudioType type;

    /// <inheritdoc/>
    public int SamplesPerPacket => this.type == AudioType.Voice ? 960 : 5760;

    public OpusEncoder(AudioBufferPool pool, int bitrate, AudioType type)
    {
        this.pool = pool;
        this.encoder = new(AudioType.Voice, bitrate);
        this.repacketizer = new();

        this.type = type;
    }

    /// <inheritdoc/>
    public AudioBufferLease Encode(ReadOnlySpan<short> pcm, out int written)
    {
        if (this.type == AudioType.Voice)
        {
            written = int.Min(pcm.Length, 1920);
            return Encode20msCore(pcm[..written]);
        }
        else if (this.type is AudioType.Music or AudioType.Auto)
        {
            written = int.Min(pcm.Length, 11520);
            return Encode120msCore(pcm[..written]);
        }

        throw new UnreachableException("Invalid audio type. The audio type must be a defined enum value.");
    }

    /// <inheritdoc/>
    public AudioBufferLease WriteSilenceFrame()
    {
        AudioBufferLease lease = AudioBufferPool.OpusSilenceFrames.Rent();

        // an opus silence frame.
        lease.Buffer[12] = 0xF8;
        lease.Buffer[13] = 0xFF;
        lease.Buffer[14] = 0xFE;

        lease.Length = 15;
        lease.FrameCount = 1;
        return lease;
    }

    /// <inheritdoc/>
    public void SetAudioType(AudioType audioType)
        => this.type = audioType;

    private AudioBufferLease Encode20msCore(ReadOnlySpan<short> pcm)
    {
        // 0.02s of two-channel 48khz stereo is 960 samples, or 1920 elements
        ArgumentOutOfRangeException.ThrowIfGreaterThan(pcm.Length, 1920, nameof(pcm));

        AudioBufferLease lease = this.pool.Rent();

        int written = this.encoder.EncodeFrame(pcm, lease.Buffer.AsSpan()[12..]);

        // 12 for the rtp header
        lease.Length = written + 12;
        lease.FrameCount = 1;

        return lease;
    }

    private AudioBufferLease Encode120msCore(ReadOnlySpan<short> pcm)
    {
        // 0.02s of two-channel 48khz stereo is 960 samples, or 1920 elements
        ArgumentOutOfRangeException.ThrowIfGreaterThan(pcm.Length, 11520, nameof(pcm));

        AudioBufferLease lease = this.pool.Rent();
        byte[] frame = ArrayPool<byte>.Shared.Rent(960);

        int written;
        int read = 0;

        while (read < pcm.Length)
        {
            // we guarantee above that this won't have more than 120ms to encode.
            int nextTarget = int.Min(read + 1920, pcm.Length);
            written = this.encoder.EncodeFrame(pcm[read..nextTarget], frame);
            this.repacketizer.Emplace(frame.AsSpan()[..written]);

            read = nextTarget;
        }

        written = this.repacketizer.Extract(lease.Buffer.AsSpan()[12..]);

        lease.Length = written + 12;
        lease.FrameCount = 6;

        return lease;
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        this.repacketizer.Dispose();
        this.encoder.Dispose();
    }
}
