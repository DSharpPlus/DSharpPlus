using System;
using System.Buffers;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

using DSharpPlus.Voice.Interop.Opus;
using DSharpPlus.Voice.MemoryServices;

namespace DSharpPlus.Voice.Codec;

/// <summary>
/// Provides an encoder for the opus codec.
/// </summary>
public sealed class OpusEncoder : IAudioEncoder
{
    private readonly OpusEncoderHandle encoder;
    private readonly OpusRepacketizerHandle repacketizer;
    private readonly byte[] buffer120ms = new byte[5760];
    private readonly byte[] buffer20ms = new byte[960];

    private AudioType type;

    /// <inheritdoc/>
    public int SamplesPerPacket => this.type == AudioType.Voice ? 960 : 5760;

    public OpusEncoder(int bitrate, AudioType type)
    {
        this.encoder = new(AudioType.Voice, bitrate);
        this.repacketizer = new();

        this.type = type;
    }

    /// <inheritdoc/>
    public AudioBufferLease Encode(ReadOnlySpan<Int16x2> pcm, out int consumed)
    {
        if (this.type == AudioType.Voice)
        {
            consumed = int.Min(pcm.Length, 960);
            return Encode20msCore(MemoryMarshal.Cast<Int16x2, short>(pcm[..consumed]));
        }
        else if (this.type is AudioType.Music or AudioType.Auto)
        {
            consumed = int.Min(pcm.Length, 5760);
            return Encode120msCore(MemoryMarshal.Cast<Int16x2, short>(pcm[..consumed]));
        }

        throw new UnreachableException("Invalid audio type. The audio type must be a defined enum value.");
    }

    /// <inheritdoc/>
    public AudioBufferLease Encode(ReadOnlySpan<Int32x2> pcm, out int consumed)
    {
        if (this.type == AudioType.Voice)
        {
            consumed = int.Min(pcm.Length, 960);
            return Encode20msCore(MemoryMarshal.Cast<Int32x2, int>(pcm[..consumed]));
        }
        else if (this.type is AudioType.Music or AudioType.Auto)
        {
            consumed = int.Min(pcm.Length, 5760);
            return Encode120msCore(MemoryMarshal.Cast<Int32x2, int>(pcm[..consumed]));
        }

        throw new UnreachableException("Invalid audio type. The audio type must be a defined enum value.");
    }

    /// <inheritdoc/>
    public AudioBufferLease Encode(ReadOnlySpan<Singlex2> pcm, out int consumed)
    {
        if (this.type == AudioType.Voice)
        {
            consumed = int.Min(pcm.Length, 960);
            return Encode20msCore(MemoryMarshal.Cast<Singlex2, float>(pcm[..consumed]));
        }
        else if (this.type is AudioType.Music or AudioType.Auto)
        {
            consumed = int.Min(pcm.Length, 5760);
            return Encode120msCore(MemoryMarshal.Cast<Singlex2, float>(pcm[..consumed]));
        }

        throw new UnreachableException("Invalid audio type. The audio type must be a defined enum value.");
    }

    /// <inheritdoc/>
    public AudioBufferLease WriteSilenceFrame()
    {
        AudioBufferLease lease = AudioBufferManager.OpusSilenceFrames.Rent(3);

        lease.FrameCount = 1;
        return lease;
    }

    /// <inheritdoc/>
    public void SetAudioType(AudioType audioType)
        => this.type = audioType;

    private AudioBufferLease Encode20msCore<T>(ReadOnlySpan<T> pcm)
    {
        // 0.02s of two-channel 48khz stereo is 960 samples, or 1920 elements
        ArgumentOutOfRangeException.ThrowIfGreaterThan(pcm.Length, 1920, nameof(pcm));

        if (pcm.Length < 1920)
        {
            // we need to submit a correctly sized frame, so zero-pad if we reached end of stream
            T[] buffer = new T[1920];

            buffer.AsSpan().Clear();
            pcm.CopyTo(buffer);

            pcm = buffer;
        }

        int written = pcm[0] switch
        {
            short => this.encoder.EncodeFrame(Unsafe.As<ReadOnlySpan<T>, ReadOnlySpan<short>>(ref pcm), this.buffer20ms),
            int => this.encoder.EncodeFrame(Unsafe.As<ReadOnlySpan<T>, ReadOnlySpan<int>>(ref pcm), this.buffer20ms),
            float => this.encoder.EncodeFrame(Unsafe.As<ReadOnlySpan<T>, ReadOnlySpan<float>>(ref pcm), this.buffer20ms),
            _ => throw new InvalidOperationException($"The generic parameter {typeof(T)} is not valid")
        };

        AudioBufferLease lease = AudioBufferManager.Shared.Rent(written);

        this.buffer20ms.AsSpan()[..written].CopyTo(lease.Buffer);
        lease.FrameCount = 1;

        return lease;
    }

    private AudioBufferLease Encode120msCore<T>(ReadOnlySpan<T> pcm)
    {
        // 0.02s of two-channel 48khz stereo is 960 samples, or 1920 elements
        ArgumentOutOfRangeException.ThrowIfGreaterThan(pcm.Length, 11520, nameof(pcm));

        if (pcm.Length < 11520)
        {
            // we need to submit a correctly sized frame, so zero-pad if we reached end of stream
            T[] buffer = new T[11520];

            buffer.AsSpan().Clear();
            pcm.CopyTo(buffer);

            pcm = buffer;
        }

        byte[] frame = ArrayPool<byte>.Shared.Rent(960);

        int written;
        int read = 0;

        while (read < pcm.Length)
        {
            // we guarantee above that this won't have more than 120ms to encode.
            int nextTarget = int.Min(read + 1920, pcm.Length);

            written = pcm[0] switch
            {
                short => this.encoder.EncodeFrame(Unsafe.As<ReadOnlySpan<T>, ReadOnlySpan<short>>(ref pcm)[read..nextTarget], frame),
                int => this.encoder.EncodeFrame(Unsafe.As<ReadOnlySpan<T>, ReadOnlySpan<int>>(ref pcm)[read..nextTarget], frame),
                float => this.encoder.EncodeFrame(Unsafe.As<ReadOnlySpan<T>, ReadOnlySpan<float>>(ref pcm)[read..nextTarget], frame),
                _ => throw new InvalidOperationException($"The generic parameter {typeof(T)} is not valid")
            };

            this.repacketizer.Emplace(frame.AsSpan()[..written]);

            read = nextTarget;
        }

        written = this.repacketizer.Extract(this.buffer120ms);

        AudioBufferLease lease = AudioBufferManager.Shared.Rent(written);

        this.buffer120ms.AsSpan()[..written].CopyTo(lease.Buffer);
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
