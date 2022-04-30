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
using System.Runtime.InteropServices;

namespace DSharpPlus.VoiceNext.Interop
{
    internal static unsafe class Bindings
    {
        [DllImport("libopus", CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr opus_encoder_create(int samplingRate, int channels, int application, out OpusError error);

        [DllImport("libopus", CallingConvention = CallingConvention.Cdecl)]
        private static extern void opus_encoder_destroy(IntPtr encoder);

        [DllImport("libopus", CallingConvention = CallingConvention.Cdecl)]
        private static extern int opus_encode(IntPtr encoder, byte* pcm, int frameSize, byte* data, int maxDataBytes);

        [DllImport("libopus", CallingConvention = CallingConvention.Cdecl)]
        private static extern OpusError opus_encoder_ctl(IntPtr encoder, OpusControl ctl, int value);

        [DllImport("libopus", CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr opus_decoder_create(int sampleRate, int channels, out OpusError error);

        [DllImport("libopus", CallingConvention = CallingConvention.Cdecl)]
        private static extern void opus_decoder_destroy(IntPtr decoder);

        [DllImport("libopus", CallingConvention = CallingConvention.Cdecl)]
        private static extern int opus_decode(IntPtr decoder, byte* opusData, int opusDataLength, byte* data, int frameSize, int decodeFec);

        [DllImport("libopus", CallingConvention = CallingConvention.Cdecl)]
        private static extern int opus_packet_get_nb_channels(byte* data);

        [DllImport("libopus", CallingConvention = CallingConvention.Cdecl)]
        private static extern int opus_packet_get_nb_frames(byte* data, int length);

        [DllImport("libopus", CallingConvention = CallingConvention.Cdecl)]
        private static extern int opus_packet_get_samples_per_frame(byte* data, int samplingRate);

        [DllImport("libopus", CallingConvention = CallingConvention.Cdecl)]
        private static extern int opus_decoder_ctl(IntPtr decoder, OpusControl ctl, out int value);

        public static IntPtr CreateEncoder(int sampleRate, int channelCount, int application)
        {
            var encoder = opus_encoder_create(sampleRate, channelCount, application, out var error);
            return error == OpusError.Ok ? encoder : throw new Exception($"Failed to instantiate Opus encoder: {error} ({(int)error})");
        }

        public static void SetEncoderOption(IntPtr encoder, OpusControl option, int value)
        {
            var error = OpusError.Ok;
            if ((error = opus_encoder_ctl(encoder, option, value)) != OpusError.Ok)
                throw new Exception($"Failed to set Opus encoder option: ${error} ({(int)error})");
        }

        public static void Encode(IntPtr encoder, ReadOnlySpan<byte> pcm, int frameSize, ref Span<byte> data)
        {
            var length = 0;

            fixed (byte* pcmPointer = pcm)
            fixed (byte* dataPointer = data)
                length = opus_encode(encoder, pcmPointer, frameSize, dataPointer, data.Length);

            if (length < 0)
            {
                var error = (OpusError)length;
                throw new Exception($"Failed to encode PCM data: {error} ({length})");
            }

            data = data.Slice(0, length);
        }

        public static IntPtr CreateDecoder(int sampleRate, int channelCount)
        {
            var decoder = opus_decoder_create(sampleRate, channelCount, out var error);
            return error == OpusError.Ok ? decoder : throw new Exception($"Failed to instantiate Opus decoder: {error} ({(int)error})");
        }

        public static int Decode(IntPtr decoder, ReadOnlySpan<byte> data, int frameSize, Span<byte> pcm, bool useFec)
        {
            var length = 0;

            fixed (byte* dataPointer = data)
            fixed (byte* pcmPointer = pcm)
                length = opus_decode(decoder, dataPointer, data.Length, pcmPointer, frameSize, useFec ? 1 : 0);

            if (length < 0)
            {
                var error = (OpusError)length;
                throw new Exception($"Failed to decode PCM data: {error} ({length})");
            }

            return length;
        }

        public static int Decode(IntPtr decoder, int frameSize, Span<byte> pcm)
        {
            var length = 0;

            fixed (byte* pcmPointer = pcm)
                length = opus_decode(decoder, null, 0, pcmPointer, frameSize, 1);

            if (length < 0)
            {
                var error = (OpusError)length;
                throw new Exception($"Failed to decode PCM data: {error} ({length})");
            }

            return length;
        }

        public static OpusPacketMetrics GetPacketMetrics(ReadOnlySpan<byte> data, int samplingRate)
        {
            int channels, frames, samplesPerFrame;

            fixed(byte* dataPointer = data)
            {
                frames = opus_packet_get_nb_frames(dataPointer, data.Length);
                samplesPerFrame = opus_packet_get_samples_per_frame(dataPointer, samplingRate);
                channels = opus_packet_get_nb_channels(dataPointer);
            }

            return new()
            {
                ChannelCount = channels,
                FrameCount = frames,
                SamplesPerFrame = samplesPerFrame,
                FrameSize = frames * samplesPerFrame
            };
        }

        public static void GetLastPacketDuration(IntPtr decoder, out int sampleCount)
            => opus_decoder_ctl(decoder, OpusControl.GetLastPacketDuration, out sampleCount);
    }
}
