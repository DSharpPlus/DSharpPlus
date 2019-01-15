using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.CompilerServices;

namespace DSharpPlus.VoiceNext.Codec
{
    internal sealed class Opus : IDisposable
    {
        public AudioFormat AudioFormat { get; }

        private IntPtr Encoder { get; }

#if !NETSTANDARD1_1
        private List<OpusDecoder> ManagedDecoders { get; }
#endif

        public Opus(AudioFormat audioFormat)
        {
            if (!audioFormat.IsValid())
                throw new ArgumentException("Invalid audio format specified.", nameof(audioFormat));

            this.AudioFormat = audioFormat;
            this.Encoder = Interop.OpusCreateEncoder(this.AudioFormat);

            // Set appropriate encoder options
            var sig = OpusSignal.Auto;
            switch (this.AudioFormat.VoiceApplication)
            {
                case VoiceApplication.Music:
                    sig = OpusSignal.Music;
                    break;

                case VoiceApplication.Voice:
                    sig = OpusSignal.Voice;
                    break;
            }
            Interop.OpusSetEncoderOption(this.Encoder, OpusControl.SetSignal, (int)sig);
            Interop.OpusSetEncoderOption(this.Encoder, OpusControl.SetPacketLossPercent, 15);
            Interop.OpusSetEncoderOption(this.Encoder, OpusControl.SetInBandFec, 1);
            Interop.OpusSetEncoderOption(this.Encoder, OpusControl.SetBitrate, 131072);

#if !NETSTANDARD1_1
            this.ManagedDecoders = new List<OpusDecoder>();
#endif
        }

        public void Encode(ReadOnlySpan<byte> pcm, ref Span<byte> target)
        {
            if (pcm.Length != target.Length)
                throw new ArgumentException("PCM and Opus buffer lengths need to be equal.", nameof(target));

            var duration = this.AudioFormat.CalculateSampleDuration(pcm.Length);
            var frameSize = this.AudioFormat.CalculateFrameSize(duration);
            var sampleSize = this.AudioFormat.CalculateSampleSize(duration);

            if (pcm.Length != sampleSize)
                throw new ArgumentException("Invalid PCM sample size.", nameof(target));

            Interop.OpusEncode(this.Encoder, pcm, frameSize, ref target);
        }

#if !NETSTANDARD1_1
        public void Decode(OpusDecoder decoder, ReadOnlySpan<byte> opus, ref Span<byte> target)
        {
            var frameSize = this.AudioFormat.CalculateMaximumFrameSize();
            if (target.Length != frameSize)
                throw new ArgumentException("PCM target buffer size needs to be equal to maximum buffer size for specified audio format.", nameof(target));

            var sampleCount = Interop.OpusDecode(decoder.Decoder, opus, frameSize, target);
            var sampleSize = this.AudioFormat.SampleCountToSampleSize(sampleCount);
            target = target.Slice(0, sampleSize);
        }

        public OpusDecoder CreateDecoder()
        {
            lock (this.ManagedDecoders)
            {
                var decoder = Interop.OpusCreateDecoder(this.AudioFormat);
                var managedDecoder = new OpusDecoder(decoder, this);
                this.ManagedDecoders.Add(managedDecoder);
                return managedDecoder;
            }
        }

        public void DestroyDecoder(OpusDecoder decoder)
        {
            lock (this.ManagedDecoders)
            {
                if (!this.ManagedDecoders.Contains(decoder))
                    return;

                this.ManagedDecoders.Remove(decoder);
                decoder.Dispose();
            }
        }
#endif

        public void Dispose()
        {
            Interop.OpusDestroyEncoder(this.Encoder);

#if !NETSTANDARD1_1
            lock (this.ManagedDecoders)
            {
                foreach (var decoder in this.ManagedDecoders)
                    decoder.Dispose();
            }
#endif
        }
    }

    /// <summary>
    /// Defines the format of PCM data consumed or produced by Opus.
    /// </summary>
    public struct AudioFormat
    {
        /// <summary>
        /// Gets the collection of sampling rates (in Hz) the Opus encoder can use.
        /// </summary>
        public static IReadOnlyCollection<int> AllowedSampleRates { get; } = new ReadOnlyCollection<int>(new[] { 8000, 12000, 16000, 24000, 48000 });

        /// <summary>
        /// Gets the collection of channel counts the Opus encoder can use.
        /// </summary>
        public static IReadOnlyCollection<int> AllowedChannelCounts { get; } = new ReadOnlyCollection<int>(new[] { 1, 2 });

        /// <summary>
        /// Gets the collection of sample durations (in ms) the Opus encoder can use.
        /// </summary>
        public static IReadOnlyCollection<int> AllowedSampleDurations { get; } = new ReadOnlyCollection<int>(new[] { 5, 10, 20, 40, 60 });

        /// <summary>
        /// Gets the default audio format. This is a formt configured for 48kHz sampling rate, 2 channels, with music quality preset.
        /// </summary>
        public static AudioFormat Default { get; } = new AudioFormat(48000, 2, VoiceApplication.Music);

        /// <summary>
        /// Gets the audio sampling rate in Hz.
        /// </summary>
        public int SampleRate { get; }

        /// <summary>
        /// Gets the audio channel count.
        /// </summary>
        public int ChannelCount { get; }

        /// <summary>
        /// Gets the voice application, which dictates the quality preset.
        /// </summary>
        public VoiceApplication VoiceApplication { get; }

        /// <summary>
        /// Creates a new audio format for use with Opus encoder.
        /// </summary>
        /// <param name="sampleRate">Audio sampling rate in Hz.</param>
        /// <param name="channelCount">Number of audio channels in the data.</param>
        /// <param name="voiceApplication">Encoder preset to use.</param>
        public AudioFormat(int sampleRate = 48000, int channelCount = 2, VoiceApplication voiceApplication = VoiceApplication.Music)
        {
            if (!AllowedSampleRates.Contains(sampleRate))
                throw new ArgumentOutOfRangeException(nameof(sampleRate), "Invalid sample rate specified.");

            if (!AllowedChannelCounts.Contains(channelCount))
                throw new ArgumentOutOfRangeException(nameof(channelCount), "Invalid channel count specified.");

            if (voiceApplication != VoiceApplication.Music && voiceApplication != VoiceApplication.Voice && voiceApplication != VoiceApplication.LowLatency)
                throw new ArgumentOutOfRangeException(nameof(voiceApplication), "Invalid voice application specified.");

            this.SampleRate = sampleRate;
            this.ChannelCount = channelCount;
            this.VoiceApplication = voiceApplication;
        }

        /// <summary>
        /// Calculates a sample size in bytes.
        /// </summary>
        /// <param name="sampleDuration">Millsecond duration of a sample.</param>
        /// <returns>Calculated sample size in bytes.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int CalculateSampleSize(int sampleDuration)
        {
            if (!AllowedSampleDurations.Contains(sampleDuration))
                throw new ArgumentOutOfRangeException(nameof(sampleDuration), "Invalid sample duration specified.");

            // Sample size in bytes is a product of the following:
            // - duration in milliseconds
            // - number of channels
            // - sample rate in kHz
            // - size of data (in this case, sizeof(int16_t))
            // which comes down to below:
            return sampleDuration * this.ChannelCount * (this.SampleRate / 1000) * 2;
        }

#if !NETSTANDARD1_1
        /// <summary>
        /// Gets the maximum buffer size for decoding. This method should be called when decoding Opus data to PCM, to ensure sufficient buffer size.
        /// </summary>
        /// <returns>Buffer size required to decode data.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int GetMaximumBufferSize()
            => this.CalculateMaximumFrameSize();
#endif

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal int CalculateSampleDuration(int sampleSize)
            => sampleSize / (this.SampleRate / 1000) / this.ChannelCount / 2 /* sizeof(int16_t) */;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal int CalculateFrameSize(int sampleDuration)
            => sampleDuration * (this.SampleRate / 1000);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal int CalculateMaximumFrameSize()
            => 120 * (this.SampleRate / 1000);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal int SampleCountToSampleSize(int sampleCount)
            => sampleCount * this.ChannelCount * 2 /* sizeof(int16_t) */;

        internal bool IsValid()
            => AllowedSampleRates.Contains(this.SampleRate) && AllowedChannelCounts.Contains(this.ChannelCount) &&
                (this.VoiceApplication == VoiceApplication.Music || this.VoiceApplication == VoiceApplication.Voice || this.VoiceApplication == VoiceApplication.LowLatency);
    }

#if !NETSTANDARD1_1
    /// <summary>
    /// Represents an Opus decoder.
    /// </summary>
    public class OpusDecoder : IDisposable
    {
        /// <summary>
        /// Gets the audio format produced by this decoder.
        /// </summary>
        public AudioFormat AudioFormat 
            => this.Opus.AudioFormat;

        internal IntPtr Decoder { get; }
        internal Opus Opus { get; }
        private volatile bool _isDisposed = false;

        internal OpusDecoder(IntPtr decoder, Opus managedOpus)
        {
            this.Decoder = decoder;
            this.Opus = managedOpus;
        }

        internal void Decode(ReadOnlySpan<byte> opus, ref Span<byte> pcm)
            => this.Opus.Decode(this, opus, ref pcm);

        /// <summary>
        /// Disposes of this Opus decoder.
        /// </summary>
        public void Dispose()
        {
            if (this._isDisposed)
                return;

            this._isDisposed = true;
            if (this.Decoder != IntPtr.Zero)
                Interop.OpusDestroyDecoder(this.Decoder);
        }
    }
#endif

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
        ResetState = 4028
    }

    internal enum OpusSignal : int
    {
        Auto = -1000,
        Voice = 3001,
        Music = 3002,
    }

    public enum VoiceApplication : int
    {
        Voice = 2048,
        Music = 2049,
        LowLatency = 2051
    }
}
