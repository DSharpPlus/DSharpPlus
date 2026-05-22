using System;

using DSharpPlus.Voice.Exceptions;

namespace DSharpPlus.Voice.Interop.Opus;

internal static unsafe partial class OpusInterop
{
    /// <summary>
    /// Creates a new opus encoder with the specified target bitrate and DSharpPlus' default settings.
    /// </summary>
    public static NativeOpusEncoder* CreateEncoder(OpusEncodingMode mode, int bitrate)
    {
        const int OpusMaxComplexity = 10;
        const int OpusEnable = 1;
        const int OpusDisable = 0;

        // opus supports bitrates between 500 and 512,000; discord supports up to 384,000
        ArgumentOutOfRangeException.ThrowIfGreaterThan(bitrate, 512_000, nameof(bitrate));
        ArgumentOutOfRangeException.ThrowIfLessThan(bitrate, 500, nameof(bitrate));

        OpusError error;
        NativeOpusEncoder* encoder = opus_encoder_create(SamplingRate, Channels, mode, &error);

        if (error != OpusError.OpusOK)
        {
            throw new OpusException(error, "initializing");
        }

        // enable forward error correction
        _ = dsharpplus_opus_encoder_ctl_set_in_band_fec(encoder, OpusEnable);

        // set the target bitrate to our decided value, pulled from either the channel settings or bot-defined constraint
        _ = dsharpplus_opus_encoder_ctl_set_bitrate(encoder, bitrate);

        // disable cvbr
        _ = dsharpplus_opus_encoder_ctl_set_vbr_constraint(encoder, OpusDisable);

        // enable opus to use the best encoding possible
        _ = dsharpplus_opus_encoder_ctl_set_complexity(encoder, OpusMaxComplexity);

        return encoder;
    }

    public static void SetBandwidth(NativeOpusEncoder* encoder, OpusBandwidth bandwidth)
        => _ = dsharpplus_opus_encoder_ctl_set_bandwidth(encoder, bandwidth);

    /// <summary>
    /// Encodes a 20ms frame from the provided s16le pcm data into the target buffer.
    /// </summary>
    /// <returns>The amount of bytes written to the target buffer.</returns>
    public static int EncodeFrame(NativeOpusEncoder* encoder, ReadOnlySpan<short> pcm, Span<byte> target)
    {
        int samples = pcm.Length / Channels;
        int result;

        fixed (short* pPcm = pcm)
        fixed (byte* pTarget = target)
        {
            result = opus_encode(encoder, pPcm, samples, pTarget, target.Length);
        }

        if (result < 0)
        {
            OpusError error = (OpusError)result;
            throw new OpusException(error, "encoding");
        }

        return result;
    }

    /// <summary>
    /// Encodes a 20ms frame from the provided s24le pcm data into the target buffer.
    /// </summary>
    /// <returns>The amount of bytes written to the target buffer.</returns>
    public static int EncodeFrame(NativeOpusEncoder* encoder, ReadOnlySpan<int> pcm, Span<byte> target)
    {
        int samples = pcm.Length / Channels;
        int result;

        fixed (int* pPcm = pcm)
        fixed (byte* pTarget = target)
        {
            result = opus_encode24(encoder, pPcm, samples, pTarget, target.Length);
        }

        if (result < 0)
        {
            OpusError error = (OpusError)result;
            throw new OpusException(error, "encoding");
        }

        return result;
    }

    /// <summary>
    /// Encodes a 20ms frame from the provided float32 pcm data into the target buffer.
    /// </summary>
    /// <returns>The amount of bytes written to the target buffer.</returns>
    public static int EncodeFrame(NativeOpusEncoder* encoder, ReadOnlySpan<float> pcm, Span<byte> target)
    {
        int samples = pcm.Length / Channels;
        int result;

        fixed (float* pPcm = pcm)
        fixed (byte* pTarget = target)
        {
            result = opus_encode_float(encoder, pPcm, samples, pTarget, target.Length);
        }

        if (result < 0)
        {
            OpusError error = (OpusError)result;
            throw new OpusException(error, "encoding");
        }

        return result;
    }

    /// <summary>
    /// Sets the signal type for this encoder.
    /// </summary>
    public static void SetSignal(NativeOpusEncoder* encoder, OpusSignal signal)
        => dsharpplus_opus_encoder_ctl_set_signal(encoder, signal);

    /// <summary>
    /// Deletes the provided encoder. It is no longer valid to use anywhere after this operation.
    /// </summary>
    public static void DestroyEncoder(NativeOpusEncoder* encoder)
        => opus_encoder_destroy(encoder);

    /// <summary>
    /// Creates a new opus decoder. Since opus is a streaming codec, this must be done for each speaking user in a call.
    /// </summary>
    public static NativeOpusDecoder* CreateDecoder()
    {
        OpusError error;
        NativeOpusDecoder* decoder = opus_decoder_create(SamplingRate, Channels, &error);

        if (error != OpusError.OpusOK)
        {
            throw new OpusException(error, "initializing");
        }

        return decoder;
    }

    /// <summary>
    /// Decodes the provided packet into the provided pcm buffer.
    /// </summary>
    /// <remarks>The amount of samples decoded, half the amount of values assigned.</remarks>
    public static int DecodePacket(NativeOpusDecoder* decoder, ReadOnlySpan<byte> buffer, Span<short> pcm)
    {
        // 5760 samples, or 23040 bytes, is the maximum size of a packet permitted by the spec
        ArgumentOutOfRangeException.ThrowIfLessThan(pcm.Length, 11520, "buffer.Length");

        int samples = pcm.Length / (Channels * sizeof(short));
        int result;

        fixed (short* pPcm = pcm)
        fixed (byte* pBuffer = buffer)
        {
            result = opus_decode(decoder, pBuffer, buffer.Length, pPcm, samples, 1);
        }

        if (result < 0)
        {
            OpusError error = (OpusError)result;
            throw new OpusException(error, "decoding");
        }

        return result;
    }

    /// <summary>
    /// Decodes the provided packet into the provided float PCM buffer.
    /// </summary>
    /// <remarks>The amount of samples decoded, half the amount of values assigned.</remarks>
    public static int DecodePacket(NativeOpusDecoder* decoder, ReadOnlySpan<byte> buffer, Span<float> pcm)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(pcm.Length, 11520, "buffer.Length");

        int samples = pcm.Length / (Channels * sizeof(float));
        int result;

        fixed (float* pPcm = pcm)
        fixed (byte* pBuffer = buffer)
        {
            result = opus_decode_float(decoder, pBuffer, buffer.Length, pPcm, samples, 1);
        }

        if (result < 0)
        {
            OpusError error = (OpusError)result;
            throw new OpusException(error, "decoding");
        }

        return result;
    }

    /// <summary>
    /// Gets the amount of samples contained in the last decoded packet. Identical to the output of the last <c>DecodePacket</c> call.
    /// </summary>
    public static int GetLastPacketSamples(NativeOpusDecoder* decoder)
    {
        int samples;
        _ = dsharpplus_opus_decoder_get_last_packet_duration(decoder, &samples);

        return samples;
    }

    /// <summary>
    /// Deletes the provided decoder. It is no longer valid to use anywhere after this operation.
    /// </summary>
    /// <param name="decoder"></param>
    public static void DestroyDecoder(NativeOpusDecoder* decoder)
        => opus_decoder_destroy(decoder);
}
