using System;
using System.Buffers;
using System.IO.Pipelines;
using System.Threading;
using System.Threading.Tasks;

namespace DSharpPlus.Voice.RuntimeServices.Pipes;

/// <summary>
/// Represents a writer for signed 16-bit little endian 48000Hz two-channel PCM audio.
/// </summary>
public sealed class S16LE48KHzStereoWriter : AbstractPcmAudioWriter
{
    internal S16LE48KHzStereoWriter(PcmAudioPipe pipe)
        : base(pipe)
    {

    }

    /// <inheritdoc/>
    protected internal override int SampleSize => 4;

    /// <inheritdoc/>
    public override void Advance(int bytes)
    {
        MemoryHelpers.CopyTo(this.rentedBuffer.AsSpan()[..bytes], this.writeHead);
        this.writeHead = this.writeHead.EndOfChain;

        ArrayPool<byte>.Shared.Return(this.rentedBuffer);
    }

    // this reader never stores anything that would need to be flushed
    /// <inheritdoc/>
    public override ValueTask<FlushResult> FlushAsync(CancellationToken cancellationToken = default)
        => ValueTask.FromResult(new FlushResult());
}
