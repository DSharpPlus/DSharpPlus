using System;
using System.Buffers;
using System.IO.Pipelines;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;

using DSharpPlus.Voice.Codec;
using DSharpPlus.Voice.MemoryServices;

namespace DSharpPlus.Voice.AudioWriters;

internal sealed class S16LE48KHzMonoWriter : AbstractPcmAudioWriter
{
    private OverflowBuffer1Byte overflow;

    internal S16LE48KHzMonoWriter(IAudioEncoder encoder, VoiceConnection connection)
        : base(encoder, connection)
    {

    }

    /// <inheritdoc/>
    protected internal override int SampleSize => 2;

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
