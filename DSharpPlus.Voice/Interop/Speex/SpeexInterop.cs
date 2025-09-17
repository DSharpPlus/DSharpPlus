using System;

namespace DSharpPlus.Voice.Interop.Speex;

/// <summary>
/// Provides the native bindings for libspeexdsp.
/// </summary>
internal static unsafe partial class SpeexInterop
{
    /// <summary>
    /// Creates a new native resampler.
    /// </summary>
    /// <param name="channels">The amount of channels being resampled.</param>
    /// <param name="inputSampleRate">The sample rate of the user-provided audio.</param>
    public static NativeSpeexResampler* CreateResampler(uint channels, uint inputSampleRate)
    {
        SpeexError error;
        NativeSpeexResampler* resampler = Bindings.speex_resampler_init(channels, inputSampleRate, 48000, Bindings.MaxQuality, &error);

        if (error == SpeexError.AllocationFailed)
        {
            throw new InvalidOperationException("Failed to allocate an audio resampler.");
        }

        return resampler;
    }

    /// <summary>
    /// Resamples the provided audio from its original sample rate to 48000Hz.
    /// </summary>
    /// <param name="resampler">The resampler to use. The sample rate of the input must be made known to the resampler in creating it.</param>
    /// <param name="input">The input audio provided by the user.</param>
    /// <param name="output">A buffer for output audio at 48000Hz. This must be long enough to contain the relevant amount of samples.</param>
    /// <param name="consumed">The amount of data consumed from the input.</param>
    /// <param name="written">The amount of data written to the output.</param>
    /// <returns>A value indicating whether the span was fully processed.</returns>
    public static bool Resample(NativeSpeexResampler* resampler, ReadOnlySpan<short> input, Span<short> output, out uint consumed, out uint written)
    {
        SpeexError error;

        fixed (short* pInput = input)
        fixed (short* pOutput = output)
        fixed (uint* pConsumed = &consumed)
        fixed (uint* pWritten = &written)
        {
            error = Bindings.speex_resampler_process_interleaved_int(resampler, pInput, pConsumed, pOutput, pWritten);
        }

        if (error == SpeexError.PointerOverlap)
        {
            throw new InvalidOperationException("The input and output spans overlapped.");
        }

        if (error == SpeexError.Overflow)
        {
            throw new OverflowException();
        }

        return (int)consumed == input.Length;
    }

    /// <summary>
    /// Destroys a given resampler.
    /// </summary>
    public static void DestroyResampler(NativeSpeexResampler* resampler)
        => Bindings.speex_resampler_destroy(resampler);
}
