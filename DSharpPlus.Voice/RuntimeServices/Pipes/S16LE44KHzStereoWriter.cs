using System;
using System.Buffers;
using System.Diagnostics;
using System.IO.Pipelines;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;

using DSharpPlus.Voice.Codec;
using DSharpPlus.Voice.RuntimeServices.Memory;

namespace DSharpPlus.Voice.RuntimeServices.Pipes;

/// <summary>
/// Represents a writer for signed 16-bit little endian 44100Hz two-channel PCM audio.
/// </summary>
internal sealed class S16LE44KHzStereoWriter : AbstractPcmAudioWriter
{
    private readonly ResampleHelper44KHz resampler;
    private OverflowBuffer3Bytes overflow;

    internal S16LE44KHzStereoWriter(IAudioEncoder encoder, VoiceConnection connection)
        : base(encoder, connection) 
        => this.resampler = new(2);

    /// <inheritdoc/>
    protected internal override int SampleSize => 4;

    protected override void ProcessSubmittedBytes(ReadOnlySpan<byte> bytes)
    {
        int length = bytes.Length + this.overflow.Available;
        byte[] buffer = ArrayPool<byte>.Shared.Rent(length);

        this.overflow.CopyTo(buffer, out int written);
        bytes.CopyTo(buffer.AsSpan()[written..]);

        short[] resampledBuffer = ArrayPool<short>.Shared.Rent(1920);

        ReadOnlySpan<short> submitted = MemoryMarshal.Cast<byte, short>(buffer.AsSpan()[..(length & 0b11)]);
        Span<short> resampled = resampledBuffer.AsSpan()[..1920];

        while (this.resampler.ResampleFrame(submitted, resampled, out int consumed, out written))
        {
            Debug.Assert(written == 1920);

            ReadOnlySpan<Int16x2> pcm = MemoryMarshal.Cast<short, Int16x2>(resampled);
            base.Encode(pcm);

            submitted = submitted[consumed..];
        }

        this.overflow.SetOverflow(buffer.AsSpan()[(length & ~0b11)..length]);
        ArrayPool<byte>.Shared.Return(buffer);
        ArrayPool<short>.Shared.Return(resampledBuffer);
    }

    /// <inheritdoc/>
    public override ValueTask<FlushResult> FlushAsync(CancellationToken cancellationToken = default)
    {
        short[] resampledBuffer = ArrayPool<short>.Shared.Rent(1920);

        this.resampler.Flush(resampledBuffer, out int written);

        ReadOnlySpan<Int16x2> pcm = MemoryMarshal.Cast<short, Int16x2>(resampledBuffer.AsSpan()[..written]);
        base.Encode(pcm);

        this.overflow.Clear();

        ArrayPool<short>.Shared.Return(resampledBuffer);

        return ValueTask.FromResult(new FlushResult());
    }

    public override void SignalCompletion()
    {
        this.resampler.Dispose();
        base.SignalCompletion();
    }
}
