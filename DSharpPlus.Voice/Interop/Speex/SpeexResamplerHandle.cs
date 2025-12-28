using System;

using Microsoft.Win32.SafeHandles;

namespace DSharpPlus.Voice.Interop.Speex;

/// <summary>
/// Represents a convenience wrapper around a <see cref="NativeSpeexResampler"/>. This type is not thread-safe and may only be used synchronized.
/// </summary>
public sealed unsafe class SpeexResamplerHandle : SafeHandleZeroOrMinusOneIsInvalid
{
    private NativeSpeexResampler* Resampler
    {
        get => (NativeSpeexResampler*)this.handle;
        set => this.handle = (nint)value;
    }

    /// <summary>
    /// Creates a new resampler handle.
    /// </summary>
    /// <param name="channels">The amount of channels in the input stream. This will be identical in the output stream.</param>
    /// <param name="inputSampleRate">The sample rate of the input stream.</param>
    public SpeexResamplerHandle(uint channels, uint inputSampleRate)
        : base(true) 
        => this.Resampler = SpeexInterop.CreateResampler(channels, inputSampleRate);

    /// <summary>
    /// Resamples the provided audio from its original sample rate to 48000Hz.
    /// </summary>
    /// <param name="input">The input audio provided by the user. The sample rate must match what was passed in the constructor.</param>
    /// <param name="output">A buffer for output audio at 48000Hz. This must be long enough to contain the relevant amount of samples.</param>
    /// <param name="consumed">The amount of data consumed from the input.</param>
    /// <param name="written">The amount of data written to the output.</param>
    /// <returns>A value indicating whether the span was fully processed.</returns>
    public bool Resample(ReadOnlySpan<short> input, Span<short> output, out uint consumed, out uint written)
        => SpeexInterop.Resample(this.Resampler, input, output, out consumed, out written);

    /// <inheritdoc/>
    protected override bool ReleaseHandle() => throw new System.NotImplementedException();
}
