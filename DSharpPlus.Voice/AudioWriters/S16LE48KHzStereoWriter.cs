using System;
using System.Buffers;
using System.IO.Pipelines;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;

using DSharpPlus.Voice.Codec;
using DSharpPlus.Voice.MemoryServices;

namespace DSharpPlus.Voice.AudioWriters;

/// <summary>
/// Represents a writer for signed 16-bit little endian 48000Hz two-channel PCM audio.
/// </summary>
public sealed class S16LE48KHzStereoWriter : AbstractPcmAudioWriter
{
    private OverflowBuffer3Bytes overflow;

    internal S16LE48KHzStereoWriter(IAudioEncoder encoder, VoiceConnection connection)
        : base(encoder, connection)
    {

    }

    /// <inheritdoc/>
    protected internal override int SampleSize => 4;

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

        ReadOnlySpan<byte> final = buffer.AsSpan()[..(length & ~0b11)];
        ReadOnlySpan<Int16x2> pcm = MemoryMarshal.Cast<byte, Int16x2>(final);

        base.Encode(pcm);

        this.overflow.SetOverflow(buffer.AsSpan()[(length & ~0b11)..length]);
        ArrayPool<byte>.Shared.Return(buffer);
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
