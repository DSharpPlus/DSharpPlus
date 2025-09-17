using System;
using System.Buffers;

namespace DSharpPlus.Voice.RuntimeServices.Pipes;

public abstract class AbstractPcmAudioWriter : AbstractAudioWriter
{
    protected readonly PcmAudioPipe pipe;
    protected PcmAudioSegment writeHead;
    protected byte[]? rentedBuffer;

    protected AbstractPcmAudioWriter(PcmAudioPipe pipe)
    {
        this.pipe = pipe;
        this.writeHead = pipe.Segment;
        this.rentedBuffer = null;
    }

    /// <inheritdoc/>
    public override long Position => this.writeHead.RunningHeadIndex;

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

    // flushes are synchronous, nothing to be canceled
    /// <inheritdoc/>
    public override void CancelPendingFlush()
    {

    }

    /// <inheritdoc/>
    public override void SignalSilence()
        => this.pipe.SignalSilence();

    /// <inheritdoc/>
    public override void SignalCompletion()
        => this.pipe.SignalCompletion();

    private void ReturnAndRentBuffer(int size)
    {
        if (this.rentedBuffer is not null)
        {
            ArrayPool<byte>.Shared.Return(this.rentedBuffer);
        }

        this.rentedBuffer = ArrayPool<byte>.Shared.Rent(size);
    }
}
