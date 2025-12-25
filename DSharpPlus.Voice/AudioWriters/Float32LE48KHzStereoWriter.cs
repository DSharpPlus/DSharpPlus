using System;
using System.Buffers;
using System.IO.Pipelines;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;

using DSharpPlus.Voice.Codec;
using DSharpPlus.Voice.MemoryServices;

namespace DSharpPlus.Voice.AudioWriters;

internal sealed class Float32LE48KHzStereoWriter : AbstractPcmAudioWriter
{
    private OverflowBuffer7Bytes frameOverflow;
    private readonly float[] overflowBuffer;
    private int overflowSamples;

    internal Float32LE48KHzStereoWriter(IAudioEncoder encoder, VoiceConnection connection)
        : base(encoder, connection) 
        => this.overflowBuffer = new float[11520];

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
        ReadOnlySpan<Singlex2> wide = MemoryMarshal.Cast<byte, Singlex2>(final);

        Encode(wide);

        this.frameOverflow.SetOverflow(buffer.AsSpan()[(length & ~0b111)..length]);
        ArrayPool<byte>.Shared.Return(buffer);
    }

    private void Encode(ReadOnlySpan<Singlex2> pcm)
    {
        // if this doesn't result in a full packet being written, don't bother, just add it to the overflow
        if (this.overflowSamples + pcm.Length < this.encoder.SamplesPerPacket)
        {
            Span<Singlex2> target = MemoryMarshal.Cast<float, Singlex2>(this.overflowBuffer.AsSpan()[(this.overflowSamples * 2)..]);
            pcm.CopyTo(target);

            this.overflowSamples += pcm.Length;
            return;
        }

        // start by concatenating the overflow from the last operation with the start of our span
        float[] overflowWrapper = ArrayPool<float>.Shared.Rent(this.encoder.SamplesPerPacket * 2);
        this.overflowBuffer.AsSpan().CopyTo(overflowWrapper);

        int samplesNeededToFillOverflow = this.encoder.SamplesPerPacket - this.overflowSamples;
        MemoryMarshal.Cast<Singlex2, float>(pcm[..samplesNeededToFillOverflow]).CopyTo(overflowWrapper.AsSpan()[(this.overflowSamples * 4)..]);

        // we pass fake sequences and timestamps, they'll be filled out later.
        AudioBufferLease firstPacket = this.encoder.Encode(overflowWrapper.AsSpan()[..(this.encoder.SamplesPerPacket * 4)], out _);
        this.PacketWriter.TryWrite(firstPacket);

        ArrayPool<float>.Shared.Return(overflowWrapper);

        pcm = pcm[samplesNeededToFillOverflow..];

        // encode the bulk of our data
        while (pcm.Length >= this.encoder.SamplesPerPacket)
        {
            AudioBufferLease packet = this.encoder.Encode(MemoryMarshal.Cast<Singlex2, float>(pcm[..this.encoder.SamplesPerPacket]), out _);
            this.PacketWriter.TryWrite(packet);
            pcm = pcm[this.encoder.SamplesPerPacket..];
        }

        // write the rest to the overflow buffer for next time
        MemoryMarshal.Cast<Singlex2, float>(pcm).CopyTo(this.overflowBuffer);
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

