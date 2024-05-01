namespace DSharpPlus.VoiceNext.Codec;

using System;
using System.Collections.Generic;

internal sealed class Opus : IDisposable
{
    public AudioFormat AudioFormat { get; }

    private IntPtr Encoder { get; }

    private List<OpusDecoder> ManagedDecoders { get; }

    public Opus(AudioFormat audioFormat)
    {
        if (!audioFormat.IsValid())
        {
            throw new ArgumentException("Invalid audio format specified.", nameof(audioFormat));
        }

        AudioFormat = audioFormat;
        Encoder = Interop.OpusCreateEncoder(AudioFormat);

        // Set appropriate encoder options
        OpusSignal sig = OpusSignal.Auto;
        switch (AudioFormat.VoiceApplication)
        {
            case VoiceApplication.Music:
                sig = OpusSignal.Music;
                break;

            case VoiceApplication.Voice:
                sig = OpusSignal.Voice;
                break;
        }
        Interop.OpusSetEncoderOption(Encoder, OpusControl.SetSignal, (int)sig);
        Interop.OpusSetEncoderOption(Encoder, OpusControl.SetPacketLossPercent, 15);
        Interop.OpusSetEncoderOption(Encoder, OpusControl.SetInBandFec, 1);
        Interop.OpusSetEncoderOption(Encoder, OpusControl.SetBitrate, 131072);

        ManagedDecoders = [];
    }

    public void Encode(ReadOnlySpan<byte> pcm, ref Span<byte> target)
    {
        if (pcm.Length != target.Length)
        {
            throw new ArgumentException("PCM and Opus buffer lengths need to be equal.", nameof(target));
        }

        int duration = AudioFormat.CalculateSampleDuration(pcm.Length);
        int frameSize = AudioFormat.CalculateFrameSize(duration);
        int sampleSize = AudioFormat.CalculateSampleSize(duration);

        if (pcm.Length != sampleSize)
        {
            throw new ArgumentException("Invalid PCM sample size.", nameof(target));
        }

        Interop.OpusEncode(Encoder, pcm, frameSize, ref target);
    }

    public void Decode(OpusDecoder decoder, ReadOnlySpan<byte> opus, ref Span<byte> target, bool useFec, out AudioFormat outputFormat)
    {
        //if (target.Length != this.AudioFormat.CalculateMaximumFrameSize())
        //    throw new ArgumentException("PCM target buffer size needs to be equal to maximum buffer size for specified audio format.", nameof(target));

        Interop.OpusGetPacketMetrics(opus, AudioFormat.SampleRate, out int channels, out int frames, out int samplesPerFrame, out int frameSize);
        outputFormat = AudioFormat.ChannelCount != channels ? new AudioFormat(AudioFormat.SampleRate, channels, AudioFormat.VoiceApplication) : AudioFormat;

        if (decoder.AudioFormat.ChannelCount != channels)
        {
            decoder.Initialize(outputFormat);
        }

        int sampleCount = Interop.OpusDecode(decoder.Decoder, opus, frameSize, target, useFec);

        int sampleSize = outputFormat.SampleCountToSampleSize(sampleCount);
        target = target[..sampleSize];
    }

    public void ProcessPacketLoss(OpusDecoder decoder, int frameSize, ref Span<byte> target) => Interop.OpusDecode(decoder.Decoder, frameSize, target);

    public int GetLastPacketSampleCount(OpusDecoder decoder)
    {
        Interop.OpusGetLastPacketDuration(decoder.Decoder, out int sampleCount);
        return sampleCount;
    }

    public OpusDecoder CreateDecoder()
    {
        lock (ManagedDecoders)
        {
            OpusDecoder managedDecoder = new(this);
            ManagedDecoders.Add(managedDecoder);
            return managedDecoder;
        }
    }

    public void DestroyDecoder(OpusDecoder decoder)
    {
        lock (ManagedDecoders)
        {
            if (!ManagedDecoders.Contains(decoder))
            {
                return;
            }

            ManagedDecoders.Remove(decoder);
            decoder.Dispose();
        }
    }

    public void Dispose()
    {
        Interop.OpusDestroyEncoder(Encoder);

        lock (ManagedDecoders)
        {
            foreach (OpusDecoder decoder in ManagedDecoders)
            {
                decoder.Dispose();
            }
        }
    }
}

/// <summary>
/// Represents an Opus decoder.
/// </summary>
public class OpusDecoder : IDisposable
{
    /// <summary>
    /// Gets the audio format produced by this decoder.
    /// </summary>
    public AudioFormat AudioFormat { get; private set; }

    internal Opus Opus { get; }
    internal IntPtr Decoder { get; private set; }

    private volatile bool _isDisposed = false;

    internal OpusDecoder(Opus managedOpus) => Opus = managedOpus;

    /// <summary>
    /// Used to lazily initialize the decoder to make sure we're
    /// using the correct output format, this way we don't end up
    /// creating more decoders than we need.
    /// </summary>
    /// <param name="outputFormat"></param>
    internal void Initialize(AudioFormat outputFormat)
    {
        if (Decoder != IntPtr.Zero)
        {
            Interop.OpusDestroyDecoder(Decoder);
        }

        AudioFormat = outputFormat;

        Decoder = Interop.OpusCreateDecoder(outputFormat);
    }

    /// <summary>
    /// Disposes of this Opus decoder.
    /// </summary>
    public void Dispose()
    {
        if (_isDisposed)
        {
            return;
        }

        _isDisposed = true;
        if (Decoder != IntPtr.Zero)
        {
            Interop.OpusDestroyDecoder(Decoder);
        }
    }
}

[Flags]
internal enum OpusError
{
    Ok = 0,
    BadArgument = -1,
    BufferTooSmall = -2,
    InternalError = -3,
    InvalidPacket = -4,
    Unimplemented = -5,
    InvalidState = -6,
    AllocationFailure = -7
}

internal enum OpusControl : int
{
    SetBitrate = 4002,
    SetBandwidth = 4008,
    SetInBandFec = 4012,
    SetPacketLossPercent = 4014,
    SetSignal = 4024,
    ResetState = 4028,
    GetLastPacketDuration = 4039
}

internal enum OpusSignal : int
{
    Auto = -1000,
    Voice = 3001,
    Music = 3002,
}
