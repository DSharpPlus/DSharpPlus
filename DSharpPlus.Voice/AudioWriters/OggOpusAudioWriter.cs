using System;
using System.Buffers;
using System.IO.Pipelines;
using System.Threading;
using System.Threading.Tasks;

using DSharpPlus.Voice.AudioWriters.Ogg;
using DSharpPlus.Voice.MemoryServices.Channels;

using Microsoft.Extensions.Logging;

namespace DSharpPlus.Voice.AudioWriters;

internal sealed class OggOpusAudioWriter : AudioWriter
{
    private readonly OggOpusReader oggOpusReader;
    private byte[]? rentedBuffer;

    public OggOpusAudioWriter(AudioChannelWriter writer, ILogger<OggOpusReader> logger)
        : base(writer) 
        => this.oggOpusReader = new(writer, logger);

    /// <inheritdoc/>
    public override void Advance(int bytes)
    {
        if (this.rentedBuffer is null)
        {
            throw new InvalidOperationException("Advance(int) may only be called once per retrieved buffer.");
        }

        this.oggOpusReader.Ingest(this.rentedBuffer.AsSpan()[..bytes]);
        ArrayPool<byte>.Shared.Return(this.rentedBuffer);
        this.rentedBuffer = null;
    }

    /// <inheritdoc/>
    public override void CancelPendingFlush()
    {
        
    }

    /// <inheritdoc/>
    public override ValueTask<FlushResult> FlushAsync(CancellationToken cancellationToken = default)
        => ValueTask.FromResult<FlushResult>(new());

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

        private void ReturnAndRentBuffer(int size)
    {
        if (this.rentedBuffer is not null)
        {
            ArrayPool<byte>.Shared.Return(this.rentedBuffer);
        }

        this.rentedBuffer = ArrayPool<byte>.Shared.Rent(size == 0 ? 16384 : size);
    }
}
