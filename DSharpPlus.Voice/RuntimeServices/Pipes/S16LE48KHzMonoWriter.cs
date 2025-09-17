using System;
using System.Buffers;
using System.IO.Pipelines;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;

using DSharpPlus.Voice.RuntimeServices.Memory;

namespace DSharpPlus.Voice.RuntimeServices.Pipes;

internal sealed class S16LE48KHzMonoWriter : AbstractPcmAudioWriter
{
    internal S16LE48KHzMonoWriter(PcmAudioPipe pipe)
        : base(pipe)
    {

    }

    protected internal override int SampleSize => 2;

    public override void Advance(int bytes)
    {
        byte[] stereo = ArrayPool<byte>.Shared.Rent(4 * bytes);

        ReadOnlySpan<short> monoView = MemoryMarshal.Cast<byte, short>(this.rentedBuffer.AsSpan());
        Span<Int16x2> stereoView = MemoryMarshal.Cast<byte, Int16x2>(stereo.AsSpan());

        MemoryHelpers.WidenToStereo(monoView[..(bytes / 2)], stereoView);

        MemoryHelpers.CopyTo(stereo, this.writeHead);
        this.writeHead = this.writeHead.EndOfChain;

        ArrayPool<byte>.Shared.Return(stereo);
    }

    // we don't buffer anything, so no point to flushing
    public override ValueTask<FlushResult> FlushAsync(CancellationToken cancellationToken = default)
        => ValueTask.FromResult(new FlushResult());
}
