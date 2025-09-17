using System;
using System.Buffers;
using System.Diagnostics;
using System.IO.Pipelines;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;

using DSharpPlus.Voice.Interop.Speex;

namespace DSharpPlus.Voice.RuntimeServices.Pipes;

/// <summary>
/// Represents a writer for signed 16-bit little endian 44100Hz two-channel PCM audio.
/// </summary>
internal sealed class S16LE44KHzStereoWriter : AbstractPcmAudioWriter
{
    private readonly SpeexResamplerHandle resampler;
    private readonly ManualResetAccumulatingBuffer buffer;

    internal S16LE44KHzStereoWriter(PcmAudioPipe pipe)
        : base(pipe)
    {
        this.resampler = new(2, 44100);

        // this is precisely the amount of bytes needed for 0.02s (or the shortest opus frame) worth of s16le 44.1khz stereo.
        this.buffer = new ManualResetAccumulatingBuffer(3528);
    }

    /// <inheritdoc/>
    protected internal override int SampleSize => 4;

    /// <inheritdoc/>
    public override void Advance(int bytes)
    {
        byte[] buffer48KHz = ArrayPool<byte>.Shared.Rent(4 * MemoryHelpers.CalculateNeededSamplesFor48KHz(44100, bytes / 4));
        SpanBufferWriter writer = new(buffer48KHz);
        ReadOnlySpan<byte> buffer44KHz = this.rentedBuffer;

        while (this.buffer.Write(buffer44KHz, out int consumed))
        {
            buffer44KHz = buffer44KHz[consumed..];
            Span<byte> target = writer.GetSpan();

            ReadOnlySpan<short> inputView = MemoryMarshal.Cast<byte, short>(this.buffer.WrittenSpan);
            Span<short> outputView = MemoryMarshal.Cast<byte, short>(target);

            this.resampler.Resample(inputView, outputView, out uint resamplerConsumed, out uint resamplerWritten);

            Debug.Assert(resamplerConsumed == 882);
            writer.Advance((int)resamplerWritten * 4);
        }

        MemoryHelpers.CopyTo(buffer48KHz.AsSpan()[..writer.Written], this.writeHead);
        this.writeHead = this.writeHead.EndOfChain;

        ArrayPool<byte>.Shared.Return(buffer48KHz);
    }

    /// <inheritdoc/>
    public override ValueTask<FlushResult> FlushAsync(CancellationToken cancellationToken = default)
    {
        byte[] converted = ArrayPool<byte>.Shared.Rent(4 * MemoryHelpers.CalculateNeededSamplesFor48KHz(44100, this.buffer.WrittenSpan.Length / 4));

        ReadOnlySpan<short> inputView = MemoryMarshal.Cast<byte, short>(this.buffer.WrittenSpan);
        Span<short> outputView = MemoryMarshal.Cast<byte, short>(converted);

        this.resampler.Resample(inputView, outputView, out uint consumed, out uint written);

        Debug.Assert(consumed * 4 == this.buffer.WrittenSpan.Length);

        MemoryHelpers.CopyTo(converted.AsSpan()[..(int)written], this.writeHead);
        this.writeHead = this.writeHead.EndOfChain;

        this.buffer.Reset();

        return ValueTask.FromResult(new FlushResult());
    }

    public override void SignalCompletion()
    {
        this.resampler.Dispose();
        base.SignalCompletion();
    }
}
