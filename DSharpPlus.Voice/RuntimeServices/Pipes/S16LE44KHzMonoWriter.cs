using System;
using System.Buffers;
using System.Diagnostics;
using System.IO.Pipelines;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;

using DSharpPlus.Voice.Interop.Speex;
using DSharpPlus.Voice.RuntimeServices.Memory;

namespace DSharpPlus.Voice.RuntimeServices.Pipes;

internal sealed class S16LE44KHzMonoWriter : AbstractPcmAudioWriter
{
    private readonly SpeexResamplerHandle resampler;
    private readonly ManualResetAccumulatingBuffer buffer;

    internal S16LE44KHzMonoWriter(PcmAudioPipe pipe)
        : base(pipe)
    {
        this.resampler = new(1, 44100);

        // this is precisely the amount of bytes needed for 0.02s (or the shortest opus frame) worth of s16le 44.1khz mono.
        this.buffer = new ManualResetAccumulatingBuffer(1764);
    }

    /// <inheritdoc/>
    protected internal override int SampleSize => 2;

    /// <inheritdoc/>
    public override void Advance(int bytes)
    {
        byte[] buffer48KHzMono = ArrayPool<byte>.Shared.Rent(2 * MemoryHelpers.CalculateNeededSamplesFor48KHz(44100, bytes / 2));
        SpanBufferWriter writer = new(buffer48KHzMono);
        ReadOnlySpan<byte> buffer44KHz = this.rentedBuffer;

        while (this.buffer.Write(buffer44KHz, out int consumed))
        {
            buffer44KHz = buffer44KHz[consumed..];
            Span<byte> target = writer.GetSpan();

            ReadOnlySpan<short> inputView = MemoryMarshal.Cast<byte, short>(this.buffer.WrittenSpan);
            Span<short> outputView = MemoryMarshal.Cast<byte, short>(target);

            this.resampler.Resample(inputView, outputView, out uint resamplerConsumed, out uint resamplerWritten);

            Debug.Assert(resamplerConsumed == 882);
            writer.Advance((int)resamplerWritten * 2);

            this.buffer.Reset();
        }

        int samplesWritten = writer.Written / 2;
        byte[] buffer48KHzStereo = ArrayPool<byte>.Shared.Rent(4 * samplesWritten);

        ReadOnlySpan<short> monoView = MemoryMarshal.Cast<byte, short>(buffer48KHzMono.AsSpan());
        Span<Int16x2> stereoView = MemoryMarshal.Cast<byte, Int16x2>(buffer48KHzStereo.AsSpan());

        MemoryHelpers.WidenToStereo(monoView[..samplesWritten], stereoView);

        MemoryHelpers.CopyTo(buffer48KHzStereo, this.writeHead);
        this.writeHead = this.writeHead.EndOfChain;

        ArrayPool<byte>.Shared.Return(buffer48KHzMono);
        ArrayPool<byte>.Shared.Return(buffer48KHzStereo);
    }

    /// <inheritdoc/>
    public override ValueTask<FlushResult> FlushAsync(CancellationToken cancellationToken = default)
    {
        byte[] converted = ArrayPool<byte>.Shared.Rent(2 * MemoryHelpers.CalculateNeededSamplesFor48KHz(44100, this.buffer.WrittenSpan.Length / 2));

        ReadOnlySpan<short> inputView = MemoryMarshal.Cast<byte, short>(this.buffer.WrittenSpan);
        Span<short> outputView = MemoryMarshal.Cast<byte, short>(converted);

        this.resampler.Resample(inputView, outputView, out uint consumed, out uint written);

        Debug.Assert(consumed * 2 == this.buffer.WrittenSpan.Length);

        int samplesWritten = (int)written / 2;
        byte[] stereo = ArrayPool<byte>.Shared.Rent(4 * samplesWritten);

        ReadOnlySpan<short> monoView = MemoryMarshal.Cast<byte, short>(converted.AsSpan());
        Span<Int16x2> stereoView = MemoryMarshal.Cast<byte, Int16x2>(stereo.AsSpan());

        MemoryHelpers.WidenToStereo(monoView[..samplesWritten], stereoView);

        MemoryHelpers.CopyTo(stereo, this.writeHead);
        this.writeHead = this.writeHead.EndOfChain;

        this.buffer.Reset();

        ArrayPool<byte>.Shared.Return(converted);
        ArrayPool<byte>.Shared.Return(stereo);

        return ValueTask.FromResult(new FlushResult());
    }

    public override void SignalCompletion()
    {
        this.resampler.Dispose();
        base.SignalCompletion();
    }
}
