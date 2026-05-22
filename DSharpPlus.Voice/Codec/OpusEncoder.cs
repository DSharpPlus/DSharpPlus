using System;
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
    private readonly byte[] buffer20ms = new byte[960];

    /// <inheritdoc/>
    public int SamplesPerPacket => 960;

    public OpusEncoder(int bitrate, AudioType type) 
        => this.encoder = new(type, bitrate);

    /// <inheritdoc/>
    public AudioBufferLease Encode(ReadOnlySpan<Int16x2> pcm, out int consumed)
    {
        consumed = int.Min(pcm.Length, 960);
        return EncodeCore(MemoryMarshal.Cast<Int16x2, short>(pcm[..consumed]));

        throw new UnreachableException("Invalid audio type. The audio type must be a defined enum value.");
    }

    /// <inheritdoc/>
    public AudioBufferLease Encode(ReadOnlySpan<Int32x2> pcm, out int consumed)
    {
        consumed = int.Min(pcm.Length, 960);
        return EncodeCore(MemoryMarshal.Cast<Int32x2, int>(pcm[..consumed]));

        throw new UnreachableException("Invalid audio type. The audio type must be a defined enum value.");
    }

    /// <inheritdoc/>
    public AudioBufferLease Encode(ReadOnlySpan<Singlex2> pcm, out int consumed)
    {
        consumed = int.Min(pcm.Length, 960);
        return EncodeCore(MemoryMarshal.Cast<Singlex2, float>(pcm[..consumed]));

        throw new UnreachableException("Invalid audio type. The audio type must be a defined enum value.");
    }

    /// <inheritdoc/>
    public AudioBufferLease WriteSilenceFrame()
    {
        AudioBufferLease lease = AudioBufferManager.OpusSilenceFrames.Rent(3);

        lease.FrameCount = 1;
        return lease;
    }

    private AudioBufferLease EncodeCore<T>(ReadOnlySpan<T> pcm)
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

    /// <inheritdoc/>
    public void Dispose() 
        => this.encoder.Dispose();
}
