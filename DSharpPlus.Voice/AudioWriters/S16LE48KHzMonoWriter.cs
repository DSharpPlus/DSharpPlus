using System;
using System.Buffers;
using System.IO.Pipelines;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;

using DSharpPlus.Voice.Codec;
using DSharpPlus.Voice.MemoryServices;
using DSharpPlus.Voice.MemoryServices.Channels;
using DSharpPlus.Voice.MemoryServices.Collections;

namespace DSharpPlus.Voice.AudioWriters;

internal sealed class S16LE48KHzMonoWriter : AbstractPcmAudioWriter
{
    private OverflowBuffer1Byte overflow;

    internal S16LE48KHzMonoWriter(IAudioEncoder encoder, AudioChannelWriter writer)
        : base(encoder, writer)
    {

    }

    /// <inheritdoc/>
    protected internal override int SampleSize => 2;

    // this reader never stores anything that would need to be flushed, but we do clear the overflow in case there's a random half-sample
    // left over that would otherwise corrupt newly submitted audio
    /// <inheritdoc/>
    public override async ValueTask<FlushResult> FlushAsync(CancellationToken cancellationToken = default)
    {
        this.overflow.Clear();
        return await base.FlushAsync(cancellationToken);
    }

    /// <inheritdoc/>
    protected override void ProcessSubmittedBytes(ReadOnlySpan<byte> bytes)
    {
        int length = bytes.Length + this.overflow.Available;
        byte[] buffer = ArrayPool<byte>.Shared.Rent(length);

        this.overflow.CopyTo(buffer, out int written);
        bytes.CopyTo(buffer.AsSpan()[written..]);

        ReadOnlySpan<byte> final = buffer.AsSpan()[..(length & ~0b1)];
        ReadOnlySpan<short> mono = MemoryMarshal.Cast<byte, short>(final);

        Int16x2[] pcmBuffer = ArrayPool<Int16x2>.Shared.Rent(mono.Length);
        Span<Int16x2> stereo = pcmBuffer.AsSpan()[..mono.Length];

        MemoryHelpers.WidenToStereo(mono, stereo);

        base.Encode(stereo);

        this.overflow.SetOverflow(buffer.AsSpan()[(length & ~0b1)..length]);
        ArrayPool<byte>.Shared.Return(buffer);
        ArrayPool<Int16x2>.Shared.Return(pcmBuffer);
    }
}
