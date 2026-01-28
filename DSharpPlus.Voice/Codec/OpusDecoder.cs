using System;
using System.Buffers;

using DSharpPlus.Voice.Interop.Opus;

namespace DSharpPlus.Voice.Codec;

/// <summary>
/// Represents a decoder for the opus codec.
/// </summary>
public sealed class OpusDecoder : IAudioDecoder
{
    private readonly OpusDecoderHandle decoder;

    public OpusDecoder() 
        => this.decoder = new OpusDecoderHandle();

    /// <inheritdoc/>
    public void Decode(ReadOnlySpan<byte> packet, IBufferWriter<short> writer)
    {
        Span<short> target = writer.GetSpan(11520);
        int samples = this.decoder.DecodePacket(packet, target);

        // DecodePacket returns the amount of samples decoded; we deal with two-channel audio.
        writer.Advance(samples * 2);
    }

    /// <inheritdoc/>
    public void Dispose()
        => this.decoder.Dispose();
}
