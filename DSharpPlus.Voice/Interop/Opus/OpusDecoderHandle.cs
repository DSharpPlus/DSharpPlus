using System;

using Microsoft.Win32.SafeHandles;

namespace DSharpPlus.Voice.Interop.Opus;

/// <summary>
/// Represents a convenience wrapper around a <see cref="NativeOpusDecoder"/>. This wrapper is not thread-safe
/// and may only be used synchronized.
/// </summary>
public sealed unsafe class OpusDecoderHandle : SafeHandleZeroOrMinusOneIsInvalid
{
    private NativeOpusDecoder* Decoder
    {
        get => (NativeOpusDecoder*)this.handle;
        set => this.handle = (nint)value;
    }

    /// <summary>
    /// Creates a new native decoder.
    /// </summary>
    public OpusDecoderHandle()
        : base(true)
        => this.Decoder = OpusInterop.CreateDecoder();

    /// <summary>
    /// Decodes a packet into PCM data. 
    /// </summary>
    /// <param name="packet">The opus-encoded packet.</param>
    /// <param name="pcm">A buffer for PCM data. This must be 11520 elements long.</param>
    /// <returns>The amount of samples decoded into the buffer.</returns>
    public int DecodePacket(ReadOnlySpan<byte> packet, Span<short> pcm)
        => OpusInterop.DecodePacket(this.Decoder, packet, pcm);

    /// <inheritdoc cref="OpusInterop.GetLastPacketSamples(NativeOpusDecoder*)"/>
    public int GetLastPacketSamples()
        => OpusInterop.GetLastPacketSamples(this.Decoder);

    /// <inheritdoc/>
    protected override bool ReleaseHandle()
    {
        OpusInterop.DestroyDecoder(this.Decoder);
        this.handle = 0;
        return true;
    }
}

