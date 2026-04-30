#pragma warning disable IDE0040

using System.Runtime.InteropServices;

namespace DSharpPlus.Voice.Interop.Speex;

partial class SpeexInterop
{
    // this is a nested type so that it only gets loaded if we actually use the speex-based features
    // we do provide speex for the supported eight platforms, but we might as well do this as a courtesy since speex is not entirely necessary.
    private static unsafe partial class Bindings
    {
        public const int MaxQuality = 10;

        /// <summary>
        /// <code>
        /// <![CDATA[SpeexResamplerState *speex_resampler_init
        /// (
        ///     spx_uint32_t nb_channels,
        ///     spx_uint32_t in_rate,
        ///     spx_uint32_t out_rate,
        ///     int quality,
        ///     int *err
        /// );]]>
        /// </code>
        /// </summary>
        [LibraryImport("speexdsp")]
        public static partial NativeSpeexResampler* speex_resampler_init
        (
            uint channels,
            uint inputSampleRate,
            uint outputSampleRate,
            int quality,
            SpeexError* error
        );

        /// <summary>
        /// <code>
        /// <![CDATA[void speex_resampler_destroy(SpeexResamplerState *st);]]>
        /// </code>
        /// </summary>
        [LibraryImport("speexdsp")]
        public static partial void speex_resampler_destroy(NativeSpeexResampler* resampler);

        /// <summary>
        /// <code>
        /// <![CDATA[int speex_resampler_process_interleaved_int
        /// (
        ///     SpeexResamplerState *st,
        ///     const spx_int16_t *in
        ///     spx_uint32_t *in_len,
        ///     spx_int16_t *out,
        ///     spx_uint32_t *out_len
        /// );]]>
        /// </code>
        /// </summary>
        [LibraryImport("speexdsp")]
        public static partial SpeexError speex_resampler_process_interleaved_int
        (
            NativeSpeexResampler* resampler,
            short* input,
            uint* inputProcessed,
            short* output,
            uint* outputWritten
        );
    }
}
