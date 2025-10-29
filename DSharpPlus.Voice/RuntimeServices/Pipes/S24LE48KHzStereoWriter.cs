using System;
using System.Buffers;
using System.IO.Pipelines;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;

using DSharpPlus.Voice.Codec;
using DSharpPlus.Voice.RuntimeServices.Memory;

namespace DSharpPlus.Voice.RuntimeServices.Pipes;

internal sealed class S24LE48KHzStereoWriter : AbstractPcmAudioWriter
{
    private OverflowBuffer7Bytes overflow;

    internal S24LE48KHzStereoWriter(IAudioEncoder encoder, VoiceConnection connection)
        : base(encoder, connection)
    {

    }

    /// <inheritdoc/>
    protected internal override int SampleSize => 8;

    // this reader never stores anything that would need to be flushed, but we do clear the overflow in case there's a random half-sample
    // left over that would otherwise corrupt newly submitted audio
    /// <inheritdoc/>
    public override ValueTask<FlushResult> FlushAsync(CancellationToken cancellationToken = default)
    {
        this.overflow.Clear();
        return ValueTask.FromResult(new FlushResult());
    }

    /// <inheritdoc/>
    protected override void ProcessSubmittedBytes(ReadOnlySpan<byte> bytes)
    {
        int length = bytes.Length + this.overflow.Available;
        byte[] buffer = ArrayPool<byte>.Shared.Rent(length);

        this.overflow.CopyTo(buffer, out int written);
        bytes.CopyTo(buffer.AsSpan()[written..]);

        ReadOnlySpan<byte> final = buffer.AsSpan()[..(length & ~0b111)];
        ReadOnlySpan<Int32x2> wide = MemoryMarshal.Cast<byte, Int32x2>(final);

        Int16x2[] pcmBuffer = ArrayPool<Int16x2>.Shared.Rent(wide.Length);
        Span<Int16x2> narrow = pcmBuffer.AsSpan()[..wide.Length];

        // NarrowToS16LE doesn't take an opinion on channel count, so we have to create views that don't either
        ReadOnlySpan<int> wideView = MemoryMarshal.Cast<Int32x2, int>(wide);
        Span<short> narrowView = MemoryMarshal.Cast<Int16x2, short>(narrow);

        MemoryHelpers.NarrowToS16LE(wideView, narrowView);

        base.Encode(narrow);

        this.overflow.SetOverflow(buffer.AsSpan()[(length & ~0b111)..length]);
        ArrayPool<byte>.Shared.Return(buffer);
        ArrayPool<Int16x2>.Shared.Return(pcmBuffer);
    }

    /// <summary>
    /// Processes and encodes the provided PCM data.
    /// </summary>
    public void WriteAudio(ReadOnlySpan<Int16x2> pcm)
    {
        // we interpret this as the user saying the audio they submitted previously is no longer what they're writing, so we should clear this
        this.overflow.Clear();
        base.Encode(pcm);
    }
}
