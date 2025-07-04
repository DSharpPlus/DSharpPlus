using System.Runtime.InteropServices;

using OpusErrorOrLength = int;

namespace DSharpPlus.Voice.Interop.Opus;

internal static unsafe partial class OpusInterop
{
    /// <summary>
    /// <code>
    /// <![CDATA[OpusEncoder *opus_encoder_create(
    ///     opus_int32 Fs,
    ///     int channels,
    ///     int application,
    ///     int* error
    /// );]]>
    /// </code>
    /// </summary>
    [LibraryImport("opus")]
    public static partial OpusEncoder* opus_encoder_create(int samplingRate, int channels, OpusEncodingMode mode, OpusError* error);

    /// <summary>
    /// <code>
    /// <![CDATA[opus_int32 opus_encode(
    ///     OpusEncoder *st,
    ///     const opus_int16 *pcm,
    ///     int frame_size,
    ///     unsigned char *data,
    ///     opus_int32 max_data_bytes
    /// );]]>
    /// </code>
    /// </summary>
    [LibraryImport("opus")]
    public static partial OpusErrorOrLength opus_encode(OpusEncoder* encoder, short* pcm, int frameSize, byte* data, int maxDataBytes);

    /// <summary>
    /// <code>
    /// <![CDATA[void opus_encoder_destroy(OpusEncoder *st);]]>
    /// </code>
    /// </summary>
    [LibraryImport("opus")]
    public static partial void opus_encoder_destroy(OpusEncoder* encoder);

    /// <summary>
    /// <code>
    /// <![CDATA[int dsharpplus_opus_encoder_ctl_set_bitrate(OpusEncoder* encoder, int bitrate);]]>
    /// </code>
    /// </summary>
    [LibraryImport("opus")]
    public static partial int dsharpplus_opus_encoder_ctl_set_bitrate(OpusEncoder* encoder, int bitrate);

    /// <summary>
    /// <code>
    /// <![CDATA[int dsharpplus_opus_encoder_ctl_set_max_bandwidth(OpusEncoder* encoder, int bandwidth);]]>
    /// </code>
    /// </summary>
    [LibraryImport("opus")]
    public static partial int dsharpplus_opus_encoder_ctl_set_max_bandwidth(OpusEncoder* encoder, int bandwidth);

    /// <summary>
    /// <code>
    /// <![CDATA[int dsharpplus_opus_encoder_ctl_set_in_band_fec(OpusEncoder* encoder, int fec);]]>
    /// </code>
    /// </summary>
    [LibraryImport("opus")]
    public static partial int dsharpplus_opus_encoder_ctl_set_in_band_fec(OpusEncoder* encoder, int fec);

    /// <summary>
    /// <code>
    /// <![CDATA[int dsharpplus_opus_encoder_ctl_set_packet_loss(OpusEncoder* encoder, int packet_loss_percent);]]>
    /// </code>
    /// </summary>
    [LibraryImport("opus")]
    public static partial int dsharpplus_opus_encoder_ctl_set_packet_loss(OpusEncoder* encoder, int packetLossPercent);

    /// <summary>
    /// <code>
    /// <![CDATA[int dsharpplus_opus_encoder_ctl_set_signal(OpusEncoder* encoder, int signal);]]>
    /// </code>
    /// </summary>
    [LibraryImport("opus")]
    public static partial int dsharpplus_opus_encoder_ctl_set_signal(OpusEncoder* encoder, int signal);

    /// <summary>
    /// <code>
    /// <![CDATA[int dsharpplus_opus_encoder_ctl_reset_state(OpusEncoder* encoder);]]>
    /// </code>
    /// </summary>
    [LibraryImport("opus")]
    public static partial int dsharpplus_opus_encoder_ctl_reset_state(OpusEncoder* encoder);

    /// <summary>
    /// <code>
    /// <![CDATA[OpusDecoder *opus_decoder_create(
    ///     opus_int32 Fs,
    ///     int channels,
    ///     int *error
    /// );]]>
    /// </code>
    /// </summary>
    [LibraryImport("opus")]
    public static partial OpusDecoder* opus_decoder_create(int sampleRate, int channels, OpusError* error);

    /// <summary>
    /// <code>
    /// <![CDATA[int opus_decode(
    ///     OpusDecoder *st,
    ///     const unsigned char *data,
    ///     opus_int32 len,
    ///     opus_int16 *pcm,
    ///     int frame_size,
    ///     int decode_fec
    /// );]]>
    /// </code>
    /// </summary>
    [LibraryImport("opus")]
    public static partial OpusErrorOrLength opus_decode(OpusDecoder* decoder, byte* data, int length, short* pcm, int frameSize, int decodeErrorCorrectionData);

    /// <summary>
    /// <code>
    /// <![CDATA[void opus_decoder_destroy(OpusDecoder *st);]]>
    /// </code>
    /// </summary>
    [LibraryImport("opus")]
    public static partial void opus_decoder_destroy(OpusDecoder* decoder);

    /// <summary>
    /// <code>
    /// <![CDATA[int dsharpplus_opus_decoder_ctl_reset_state(OpusDecoder* decoder);]]>
    /// </code>
    /// </summary>
    [LibraryImport("opus")]

    public static partial int dsharpplus_opus_decoder_ctl_reset_state(OpusDecoder* decoder);

    /// <summary>
    /// <code>
    /// <![CDATA[int dsharpplus_opus_decoder_get_last_packet_duration(OpusDecoder* decoder, int* target);]]>
    /// </code>
    /// </summary>
    [LibraryImport("opus")]
    public static partial int dsharpplus_opus_decoder_get_last_packet_duration(OpusDecoder* decoder, int* sampleCount);
}
