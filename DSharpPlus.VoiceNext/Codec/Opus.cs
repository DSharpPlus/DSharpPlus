using System;
using System.Collections.Generic;

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
}
