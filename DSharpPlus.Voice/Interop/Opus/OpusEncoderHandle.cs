using System;

using DSharpPlus.Voice.Codec;

using Microsoft.Win32.SafeHandles;

namespace DSharpPlus.Voice.Interop.Opus;

/// <summary>
/// Represents a convenience wrapper around a <see cref="NativeOpusEncoder"/>. This wrapper is not thread-safe
/// and may only be used synchronized.
/// </summary>
public sealed unsafe class OpusEncoderHandle : SafeHandleZeroOrMinusOneIsInvalid
{
    private NativeOpusEncoder* Encoder
    {
        get => (NativeOpusEncoder*)this.handle;
        set => this.handle = (nint)value;
    }

    /// <summary>
    /// Creates a new native encoder for the specified bitrate.
    /// </summary>
    public OpusEncoderHandle(AudioType audioType, int bitrate)
        : base(true)
    {
        this.Encoder = OpusInterop.CreateEncoder(OpusEncodingMode.Voip, bitrate);
        OpusInterop.SetSignal(this.Encoder, AudioTypeToOpusSignal(audioType));
    }

    /// <summary>
    /// Encodes a frame, returning the amount of bytes written to the target buffer.
    /// </summary>
    /// <param name="pcm">The PCM source data. This must be provided in 48KHz, 16-bit little endian, two channel format.</param>
    /// <param name="target">The output buffer to write to. If this buffer is not small enough, opus will lower the quality accordingly to fit.</param>
    /// <returns></returns>
    public int EncodeFrame(ReadOnlySpan<short> pcm, Span<byte> target)
        => OpusInterop.EncodeFrame(this.Encoder, pcm, target);

    /// <summary>
    /// Encodes a frame, returning the amount of bytes written to the target buffer.
    /// </summary>
    /// <param name="pcm">The PCM source data. This must be provided in 48KHz, 24-bit little endian, two channel format.</param>
    /// <param name="target">The output buffer to write to. If this buffer is not small enough, opus will lower the quality accordingly to fit.</param>
    /// <returns></returns>
    public int EncodeFrame(ReadOnlySpan<int> pcm, Span<byte> target)
        => OpusInterop.EncodeFrame(this.Encoder, pcm, target);

    /// <summary>
    /// Encodes a frame, returning the amount of bytes written to the target buffer.
    /// </summary>
    /// <param name="pcm">The PCM source data. This must be provided in 48KHz, single-precision float little endian, two channel format.</param>
    /// <param name="target">The output buffer to write to. If this buffer is not small enough, opus will lower the quality accordingly to fit.</param>
    /// <returns></returns>
    public int EncodeFrame(ReadOnlySpan<float> pcm, Span<byte> target)
        => OpusInterop.EncodeFrame(this.Encoder, pcm, target);

    /// <summary>
    /// Sets the audio type used by this encoder.
    /// </summary>
    public void SetAudioType(AudioType audioType)
        => OpusInterop.SetSignal(this.Encoder, AudioTypeToOpusSignal(audioType));

    /// <inheritdoc/>
    protected override bool ReleaseHandle()
    {
        OpusInterop.DestroyEncoder(this.Encoder);
        this.handle = 0;
        return true;
    }

    private static OpusSignal AudioTypeToOpusSignal(AudioType audioType)
    {
        return audioType switch
        {
            AudioType.Voice => OpusSignal.Voice,
            AudioType.Music => OpusSignal.Music,
            AudioType.Auto => OpusSignal.Auto,
            _ => OpusSignal.Auto
        };
    }
}
