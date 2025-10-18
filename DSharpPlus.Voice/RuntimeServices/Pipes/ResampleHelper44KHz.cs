using System;
using System.Diagnostics;
using System.Runtime.InteropServices;

using DSharpPlus.Voice.Interop.Speex;

namespace DSharpPlus.Voice.RuntimeServices.Pipes;

internal class ResampleHelper44KHz : IDisposable
{
    private readonly SpeexResamplerHandle resampler;
    private readonly ManualResetAccumulatingBuffer buffer;

    public ResampleHelper44KHz(int channels)
    {
        this.resampler = new((uint)channels, 44100);
        this.buffer = new(channels * 1764);
    }

    public bool ResampleFrame(ReadOnlySpan<short> buffer44Khz, Span<short> buffer48Khz, out int consumed, out int written)
    {
        ReadOnlySpan<byte> byteWiseView = MemoryMarshal.Cast<short, byte>(buffer44Khz);

        // all we did was aggregate data without reaching a full frame, return
        if (!this.buffer.Write(byteWiseView, out consumed))
        {
            written = 0;
            return false;
        }

        ReadOnlySpan<short> accumulated = MemoryMarshal.Cast<byte, short>(this.buffer.WrittenSpan);

        this.resampler.Resample(accumulated, buffer48Khz, out uint resamplerConsumed, out uint resamplerWritten);

        Debug.Assert(resamplerConsumed == 882);

        written = (int)resamplerWritten;

        this.buffer.Reset();
        return true;
    }

    public void Flush(Span<short> buffer48Khz, out int written)
    {
        ReadOnlySpan<short> accumulated = MemoryMarshal.Cast<byte, short>(this.buffer.WrittenSpan);

        this.resampler.Resample(accumulated, buffer48Khz, out _, out uint resamplerWritten);
        written = (int)resamplerWritten;

        this.buffer.Reset();
    }

    public void Dispose() => this.resampler.Dispose();
}
