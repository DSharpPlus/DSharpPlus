using System.Runtime.InteropServices;

namespace DSharpPlus.Voice.Interop.Opus;

internal static unsafe partial class OpusInterop
{
    public const int SamplingRate = 48000;
    public const int FrameDuration = 20;
    public const int Channels = 2;
    public const int SampleCount = 960;

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
    private static partial OpusEncoder* opus_encoder_create(int samplingRate, int channels, OpusEncodingMode mode, OpusError* error);

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
    private static partial int opus_encode(OpusEncoder* encoder, short* pcm, int frameSize, byte* data, int maxDataBytes);

    /// <summary>
    /// <code>
    /// <![CDATA[void opus_encoder_destroy(OpusEncoder *st);]]>
    /// </code>
    /// </summary>
    [LibraryImport("opus")]
    private static partial void opus_encoder_destroy(OpusEncoder* encoder);

    /// <summary>
    /// <code>
    /// <![CDATA[int dsharpplus_opus_encoder_ctl_set_bitrate(OpusEncoder* encoder, int bitrate);]]>
    /// </code>
    /// </summary>
    [LibraryImport("opus")]
    private static partial int dsharpplus_opus_encoder_ctl_set_bitrate(OpusEncoder* encoder, int bitrate);

    /// <summary>
    /// <code>
    /// <![CDATA[int dsharpplus_opus_encoder_ctl_set_max_bandwidth(OpusEncoder* encoder, int bandwidth);]]>
    /// </code>
    /// </summary>
    [LibraryImport("opus")]
    private static partial int dsharpplus_opus_encoder_ctl_set_max_bandwidth(OpusEncoder* encoder, int bandwidth);

    /// <summary>
    /// <code>
    /// <![CDATA[int dsharpplus_opus_encoder_ctl_set_in_band_fec(OpusEncoder* encoder, int fec);]]>
    /// </code>
    /// </summary>
    [LibraryImport("opus")]
    private static partial int dsharpplus_opus_encoder_ctl_set_in_band_fec(OpusEncoder* encoder, int fec);

    /// <summary>
    /// <code>
    /// <![CDATA[int dsharpplus_opus_encoder_ctl_set_packet_loss(OpusEncoder* encoder, int packet_loss_percent);]]>
    /// </code>
    /// </summary>
    [LibraryImport("opus")]
    private static partial int dsharpplus_opus_encoder_ctl_set_packet_loss(OpusEncoder* encoder, int packetLossPercent);

    /// <summary>
    /// <code>
    /// <![CDATA[int dsharpplus_opus_encoder_ctl_set_signal(OpusEncoder* encoder, int signal);]]>
    /// </code>
    /// </summary>
    [LibraryImport("opus")]
    private static partial int dsharpplus_opus_encoder_ctl_set_signal(OpusEncoder* encoder, OpusSignal signal);

    /// <summary>
    /// <code>
    /// <![CDATA[int dsharpplus_opus_encoder_ctl_set_vbr_constraint(OpusEncoder* encoder, int value);]]>
    /// </code>
    /// </summary>
    [LibraryImport("opus")]
    private static partial int dsharpplus_opus_encoder_ctl_set_vbr_constraint(OpusEncoder* encoder, int mode);

    /// <summary>
    /// <code>
    /// <![CDATA[int dsharpplus_opus_encoder_ctl_set_complexity(OpusEncoder* encoder, int complexity);]]>
    /// </code>
    /// </summary>
    [LibraryImport("opus")]
    private static partial int dsharpplus_opus_encoder_ctl_set_complexity(OpusEncoder* encoder, int complexity);

    /// <summary>
    /// <code>
    /// <![CDATA[int dsharpplus_opus_encoder_ctl_reset_state(OpusEncoder* encoder);]]>
    /// </code>
    /// </summary>
    [LibraryImport("opus")]
    private static partial int dsharpplus_opus_encoder_ctl_reset_state(OpusEncoder* encoder);

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
    private static partial OpusDecoder* opus_decoder_create(int sampleRate, int channels, OpusError* error);

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
    private static partial int opus_decode(OpusDecoder* decoder, byte* data, int length, short* pcm, int frameSize, int decodeErrorCorrectionData);

    /// <summary>
    /// <code>
    /// <![CDATA[void opus_decoder_destroy(OpusDecoder *st);]]>
    /// </code>
    /// </summary>
    [LibraryImport("opus")]
    private static partial void opus_decoder_destroy(OpusDecoder* decoder);

    /// <summary>
    /// <code>
    /// <![CDATA[int dsharpplus_opus_decoder_ctl_reset_state(OpusDecoder* decoder);]]>
    /// </code>
    /// </summary>
    [LibraryImport("opus")]

    private static partial int dsharpplus_opus_decoder_ctl_reset_state(OpusDecoder* decoder);

    /// <summary>
    /// <code>
    /// <![CDATA[int dsharpplus_opus_decoder_get_last_packet_duration(OpusDecoder* decoder, int* target);]]>
    /// </code>
    /// </summary>
    [LibraryImport("opus")]
    private static partial int dsharpplus_opus_decoder_get_last_packet_duration(OpusDecoder* decoder, int* sampleCount);

    /// <summary>
    /// <code>
    /// <![CDATA[OpusRepacketizer* opus_repacketizer_create(void);]]>
    /// </code>
    /// </summary>
    [LibraryImport("opus")]
    private static partial OpusRepacketizer* opus_repacketizer_create();

    /// <summary>
    /// <code>
    /// <![CDATA[OpusRepacketizer* opus_repacketizer_init(OpusRepacketizer *rp);]]>
    /// </code>
    /// </summary>
    [LibraryImport("opus")]
    private static partial OpusRepacketizer* opus_repacketizer_init(OpusRepacketizer* repacketizer);

    /// <summary>
    /// <code>
    /// <![CDATA[int opus_repacketizer_cat(OpusRepacketizer *rp, const unsigned char *data, opus_int32 len);]]>
    /// </code>
    /// </summary>
    [LibraryImport("opus")]
    private static partial int opus_repacketizer_cat(OpusRepacketizer* repacketizer, byte* data, int length);

    /// <summary>
    /// <code>
    /// <![CDATA[opus_int32 opus_repacketizer_out(OpusRepacketizer *rp, unsigned char *data, opus_int32 maxlen);]]>
    /// </code>
    /// </summary>
    [LibraryImport("opus")]
    private static partial int opus_repacketizer_out(OpusRepacketizer* repacketizer, byte* data, int length);

    /// <summary>
    /// <code>
    /// <![CDATA[int opus_repacketizer_get_nb_frames(OpusRepacketizer *rp);]]>
    /// </code>
    /// </summary>
    [LibraryImport("opus")]
    private static partial int opus_repacketizer_get_nb_frames(OpusRepacketizer* repacketizer);

    /// <summary>
    /// <code>
    /// <![CDATA[void opus_repacketizer_destroy(OpusRepacketizer *rp);]]>
    /// </code>
    /// </summary>
    [LibraryImport("opus")]
    private static partial void opus_repacketizer_destroy(OpusRepacketizer* repacketizer);
}
