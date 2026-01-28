using System;
using System.Buffers;
using System.IO.Pipelines;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;

using DSharpPlus.Voice.Codec;
using DSharpPlus.Voice.MemoryServices;

namespace DSharpPlus.Voice.AudioWriters;

internal sealed class S24LE48KHzStereoWriter : AbstractPcmAudioWriter
{
    private OverflowBuffer7Bytes frameOverflow;
    private readonly int[] overflowBuffer;
    private int overflowSamples;

    internal S24LE48KHzStereoWriter(IAudioEncoder encoder, VoiceConnection connection)
        : base(encoder, connection) 
        => this.overflowBuffer = new int[11520];

    /// <inheritdoc/>
    protected internal override int SampleSize => 8;

    // this reader never stores anything that would need to be flushed, but we do clear the overflow in case there's a random half-sample
    // left over that would otherwise corrupt newly submitted audio
    /// <inheritdoc/>
    public override ValueTask<FlushResult> FlushAsync(CancellationToken cancellationToken = default)
    {
        this.frameOverflow.Clear();
        return ValueTask.FromResult(new FlushResult());
    }

    /// <inheritdoc/>
    protected override void ProcessSubmittedBytes(ReadOnlySpan<byte> bytes)
    {
        int length = bytes.Length + this.frameOverflow.Available;
        byte[] buffer = ArrayPool<byte>.Shared.Rent(length);

        this.frameOverflow.CopyTo(buffer, out int written);
        bytes.CopyTo(buffer.AsSpan()[written..]);

        ReadOnlySpan<byte> final = buffer.AsSpan()[..(length & ~0b111)];
        ReadOnlySpan<Int32x2> wide = MemoryMarshal.Cast<byte, Int32x2>(final);

        Encode(wide);

        this.frameOverflow.SetOverflow(buffer.AsSpan()[(length & ~0b111)..length]);
        ArrayPool<byte>.Shared.Return(buffer);
    }

    private void Encode(ReadOnlySpan<Int32x2> pcm)
    {
        // if this doesn't result in a full packet being written, don't bother, just add it to the overflow
        if (this.overflowSamples + pcm.Length < this.encoder.SamplesPerPacket)
        {
            Span<Int32x2> target = MemoryMarshal.Cast<int, Int32x2>(this.overflowBuffer.AsSpan()[(this.overflowSamples * 2)..]);
            pcm.CopyTo(target);

            this.overflowSamples += pcm.Length;
            return;
        }

        // start by concatenating the overflow from the last operation with the start of our span
        int[] overflowWrapper = ArrayPool<int>.Shared.Rent(this.encoder.SamplesPerPacket * 2);
        this.overflowBuffer.AsSpan().CopyTo(overflowWrapper);

        int samplesNeededToFillOverflow = this.encoder.SamplesPerPacket - this.overflowSamples;
        MemoryMarshal.Cast<Int32x2, int>(pcm[..samplesNeededToFillOverflow]).CopyTo(overflowWrapper.AsSpan()[(this.overflowSamples * 4)..]);

        // we pass fake sequences and timestamps, they'll be filled out later.
        AudioBufferLease firstPacket = this.encoder.Encode(overflowWrapper.AsSpan()[..(this.encoder.SamplesPerPacket * 4)], out _);
        this.PacketWriter.TryWrite(firstPacket);

        ArrayPool<int>.Shared.Return(overflowWrapper);

        pcm = pcm[samplesNeededToFillOverflow..];

        // encode the bulk of our data
        while (pcm.Length >= this.encoder.SamplesPerPacket)
        {
            AudioBufferLease packet = this.encoder.Encode(MemoryMarshal.Cast<Int32x2, int>(pcm[..this.encoder.SamplesPerPacket]), out _);
            this.PacketWriter.TryWrite(packet);
            pcm = pcm[this.encoder.SamplesPerPacket..];
        }

        // write the rest to the overflow buffer for next time
        MemoryMarshal.Cast<Int32x2, int>(pcm).CopyTo(this.overflowBuffer);
        this.overflowSamples = pcm.Length;
    }

    /// <summary>
    /// Processes and encodes the provided PCM data.
    /// </summary>
    public void WriteAudio(ReadOnlySpan<Int16x2> pcm)
    {
        // we interpret this as the user saying the audio they submitted previously is no longer what they're writing, so we should clear this
        this.frameOverflow.Clear();
        base.Encode(pcm);
    }
}
