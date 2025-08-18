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

    /// <summary>
    /// Encodes a 20ms frame from the provided pcm data into the target buffer.
    /// </summary>
    /// <returns>The amount of bytes written to the target buffer.</returns>
    public static int EncodeFrame(NativeOpusEncoder* encoder, ReadOnlySpan<short> pcm, Span<byte> target)
    {
        int samples = pcm.Length / (Channels * sizeof(short));
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
    /// Gets the amount of samples contained in the last decoded packet. Identical to the output of the last <see cref="DecodePacket"/> call.
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

    /// <summary>
    /// Creates a new opus repacketizer.
    /// </summary>
    public static NativeOpusRepacketizer* CreateRepacketizer()
        => opus_repacketizer_create();

    /// <summary>
    /// Reinitializes the provided repacketizer. This must be called every time a packet is taken out of a repacketizer.
    /// </summary>
    public static NativeOpusRepacketizer* ReinitializeRepacketizer(NativeOpusRepacketizer* repacketizer)
        => opus_repacketizer_init(repacketizer);

    /// <summary>
    /// Submits the provided frame to the repacketizer, returning whether the operation succeeded.
    /// </summary>
    public static OpusError SubmitFrameToRepacketizer(NativeOpusRepacketizer* repacketizer, ReadOnlySpan<byte> frame)
    {
        fixed (byte* pFrame = frame)
        {
            return (OpusError)opus_repacketizer_cat(repacketizer, pFrame, frame.Length);
        }
    }

    /// <summary>
    /// Gets the amount of frames submitted to this repacketizer. DSharpPlus encodes in 20ms frames, so once this hits 6 we must extract the packet.
    /// </summary>
    public static int GetFramesInRepacketizer(NativeOpusRepacketizer* repacketizer)
        => opus_repacketizer_get_nb_frames(repacketizer);

    /// <summary>
    /// Extracts a finished packet from the repacketizer.
    /// </summary>
    /// <returns>The amount of bytes written to the packet.</returns>
    public static int ExtractPacket(NativeOpusRepacketizer* repacketizer, Span<byte> buffer)
    {
        // 5760 is the maximum amount of bytes in a packet we can send to discord, pinning 384kbps
        // if discord ever raises their maximum bitrate we'll have to update this limit and our memory management code.
        ArgumentOutOfRangeException.ThrowIfLessThan(buffer.Length, 5760, "buffer.Length");

        int result;

        fixed (byte* pBuffer = buffer)
        {
            result = opus_repacketizer_out(repacketizer, pBuffer, buffer.Length);
        }

        if (result < 0)
        {
            throw new OpusException((OpusError)result, "repacketizing");
        }

        return result;
    }

    /// <summary>
    /// Deletes the provided repacketizer. It is no longer valid to use anywhere after this operation.
    /// </summary>
    public static void DestroyRepacketizer(NativeOpusRepacketizer* repacketizer)
        => opus_repacketizer_destroy(repacketizer);
}
