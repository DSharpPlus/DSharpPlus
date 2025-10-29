using System;
using System.Buffers;
using System.IO;
using System.IO.Pipelines;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;

using DSharpPlus.Voice.Codec;
using DSharpPlus.Voice.RuntimeServices.Memory;

namespace DSharpPlus.Voice;

/// <summary>
/// Provides a base implementation of an audio writer for PCM and PCM-shaped data.
/// </summary>
public abstract class AbstractPcmAudioWriter : AbstractAudioWriter
{
    private byte[]? rentedBuffer;
    private readonly short[] overflowBuffer;
    private readonly IAudioEncoder encoder;

    private int overflowSamples;

    protected AbstractPcmAudioWriter(IAudioEncoder encoder, VoiceConnection connection)
        : base(connection)
    {
        this.rentedBuffer = null;
        this.overflowBuffer = new short[11520];
        this.encoder = encoder;
    }

    /// <summary>
    /// The amount of bytes consumed by a sample in the current audio format.
    /// </summary>
    protected internal abstract int SampleSize { get; }

    /// <summary>
    /// This is called by the framework when data is submitted. The implementation cannot take ownership of the span
    /// </summary>
    /// <param name="bytes"></param>
    protected abstract void ProcessSubmittedBytes(ReadOnlySpan<byte> bytes);

    /// <inheritdoc/>
    public override Memory<byte> GetMemory(int sizeHint = 0)
    {
        ReturnAndRentBuffer(sizeHint);
        return this.rentedBuffer;
    }

    /// <inheritdoc/>
    public override Span<byte> GetSpan(int sizeHint = 0)
    {
        ReturnAndRentBuffer(sizeHint);
        return this.rentedBuffer;
    }

    /// <inheritdoc/>
    public override void Advance(int bytes)
    {
        if (this.rentedBuffer is null)
        {
            throw new InvalidOperationException("Advance(int) may only be called once per retrieved buffer.");
        }

        ProcessSubmittedBytes(this.rentedBuffer.AsSpan()[..bytes]);
        ArrayPool<byte>.Shared.Return(this.rentedBuffer);
    }

    public override ValueTask<FlushResult> FlushAsync(CancellationToken cancellationToken = default)
    {
        Flush();
        return ValueTask.FromResult(new FlushResult());
    }

    private void Flush()
    {
        AudioBufferLease packet = this.encoder.Encode(this.overflowBuffer.AsSpan()[..(this.overflowSamples * 2)], out _);
        this.PacketWriter.TryWrite(packet);
        this.overflowSamples = 0;
    }

    // flushes are synchronous, nothing to be canceled
    /// <inheritdoc/>
    public override void CancelPendingFlush()
    {
        
    }

    /// <inheritdoc/>
    public override void SignalSilence()
    {
        Flush();

        for (int i = 0; i < 5; i++)
        {
            this.PacketWriter.TryWrite(this.encoder.WriteSilenceFrame());
        }
    }

    /// <inheritdoc/>
    public sealed override Stream AsStream(bool leaveOpen = false)
        => new AudioWriteStream(this, this.SampleSize);

    /// <summary>
    /// Packetizes and encodes the provided s16le 48khz PCM data.
    /// </summary>
    protected void Encode(ReadOnlySpan<Int16x2> pcm)
    {
        // if this doesn't result in a full packet being written, don't bother, just add it to the overflow
        if (this.overflowSamples + pcm.Length < this.encoder.SamplesPerPacket)
        {
            Span<Int16x2> target = MemoryMarshal.Cast<short, Int16x2>(this.overflowBuffer.AsSpan()[(this.overflowSamples * 2)..]);
            pcm.CopyTo(target);

            this.overflowSamples += pcm.Length;
            return;
        }

        // start by concatenating the overflow from the last operation with the start of our span
        short[] overflowWrapper = ArrayPool<short>.Shared.Rent(this.encoder.SamplesPerPacket * 2);
        this.overflowBuffer.AsSpan().CopyTo(overflowWrapper);

        int samplesNeededToFillOverflow = this.encoder.SamplesPerPacket - this.overflowSamples;
        MemoryMarshal.Cast<Int16x2, short>(pcm[..samplesNeededToFillOverflow]).CopyTo(overflowWrapper.AsSpan()[(this.overflowSamples * 4)..]);

        // we pass fake sequences and timestamps, they'll be filled out later.
        AudioBufferLease firstPacket = this.encoder.Encode(overflowWrapper.AsSpan()[..(this.encoder.SamplesPerPacket * 4)], out _);
        this.PacketWriter.TryWrite(firstPacket);

        ArrayPool<short>.Shared.Return(overflowWrapper);

        pcm = pcm[samplesNeededToFillOverflow..];

        // encode the bulk of our data
        while (pcm.Length >= this.encoder.SamplesPerPacket)
        {
            AudioBufferLease packet = this.encoder.Encode(MemoryMarshal.Cast<Int16x2, short>(pcm[..this.encoder.SamplesPerPacket]), out _);
            this.PacketWriter.TryWrite(packet);
            pcm = pcm[this.encoder.SamplesPerPacket..];
        }

        // write the rest to the overflow buffer for next time
        MemoryMarshal.Cast<Int16x2, short>(pcm).CopyTo(this.overflowBuffer);
        this.overflowSamples = pcm.Length;
    }

    private void ReturnAndRentBuffer(int size)
    {
        if (this.rentedBuffer is not null)
        {
            ArrayPool<byte>.Shared.Return(this.rentedBuffer);
        }

        this.rentedBuffer = ArrayPool<byte>.Shared.Rent(size);
    }
}
