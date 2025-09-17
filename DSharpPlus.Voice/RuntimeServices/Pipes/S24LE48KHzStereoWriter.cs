using System;
using System.Buffers;
using System.IO.Pipelines;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;

namespace DSharpPlus.Voice.RuntimeServices.Pipes;

internal sealed class S24LE48KHzStereoWriter : AbstractPcmAudioWriter
{
    internal S24LE48KHzStereoWriter(PcmAudioPipe pipe)
        : base(pipe)
    {

    }

    protected internal override int SampleSize => 8;

    public override void Advance(int bytes)
    {
        byte[] narrow = ArrayPool<byte>.Shared.Rent(bytes / 2);

        ReadOnlySpan<int> s24View = MemoryMarshal.Cast<byte, int>(this.rentedBuffer.AsSpan());
        Span<short> s16View = MemoryMarshal.Cast<byte, short>(narrow.AsSpan());

        MemoryHelpers.NarrowToS16LE(s24View[..(bytes / 4)], s16View);

        MemoryHelpers.CopyTo(narrow, this.writeHead);
        this.writeHead = this.writeHead.EndOfChain;

        ArrayPool<byte>.Shared.Return(narrow);
    }

    public override ValueTask<FlushResult> FlushAsync(CancellationToken cancellationToken = default)
        => ValueTask.FromResult(new FlushResult());
}
