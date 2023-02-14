// This file is part of the DSharpPlus project.
//
// Copyright (c) 2015 Mike Santiago
// Copyright (c) 2016-2022 DSharpPlus Contributors
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.

using System;
using System.Collections.Generic;

namespace DSharpPlus.VoiceNext.Codec
{
    internal sealed class Opus : IDisposable
    {
        public AudioFormat AudioFormat { get; }

        private IntPtr Encoder { get; }

        private List<OpusDecoder> ManagedDecoders { get; }

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

            this.ManagedDecoders = new List<OpusDecoder>();
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

        public void Decode(OpusDecoder decoder, ReadOnlySpan<byte> opus, ref Span<byte> target, bool useFec, out AudioFormat outputFormat)
        {
            //if (target.Length != this.AudioFormat.CalculateMaximumFrameSize())
            //    throw new ArgumentException("PCM target buffer size needs to be equal to maximum buffer size for specified audio format.", nameof(target));

            Interop.OpusGetPacketMetrics(opus, this.AudioFormat.SampleRate, out var channels, out var frames, out var samplesPerFrame, out var frameSize);
            outputFormat = this.AudioFormat.ChannelCount != channels ? new AudioFormat(this.AudioFormat.SampleRate, channels, this.AudioFormat.VoiceApplication) : this.AudioFormat;

            if (decoder.AudioFormat.ChannelCount != channels)
                decoder.Initialize(outputFormat);

            var sampleCount = Interop.OpusDecode(decoder.Decoder, opus, frameSize, target, useFec);

            var sampleSize = outputFormat.SampleCountToSampleSize(sampleCount);
            target = target.Slice(0, sampleSize);
        }

        public void ProcessPacketLoss(OpusDecoder decoder, int frameSize, ref Span<byte> target) => Interop.OpusDecode(decoder.Decoder, frameSize, target);

        public int GetLastPacketSampleCount(OpusDecoder decoder)
        {
            Interop.OpusGetLastPacketDuration(decoder.Decoder, out var sampleCount);
            return sampleCount;
        }

        public OpusDecoder CreateDecoder()
        {
            lock (this.ManagedDecoders)
            {
                var managedDecoder = new OpusDecoder(this);
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

        public void Dispose()
        {
            Interop.OpusDestroyEncoder(this.Encoder);

            lock (this.ManagedDecoders)
            {
                foreach (var decoder in this.ManagedDecoders)
                    decoder.Dispose();
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

        internal OpusDecoder(Opus managedOpus)
        {
            this.Opus = managedOpus;
        }

        /// <summary>
        /// Used to lazily initialize the decoder to make sure we're
        /// using the correct output format, this way we don't end up
        /// creating more decoders than we need.
        /// </summary>
        /// <param name="outputFormat"></param>
        internal void Initialize(AudioFormat outputFormat)
        {
            if (this.Decoder != IntPtr.Zero)
                Interop.OpusDestroyDecoder(this.Decoder);

            this.AudioFormat = outputFormat;

            this.Decoder = Interop.OpusCreateDecoder(outputFormat);
        }

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
}
