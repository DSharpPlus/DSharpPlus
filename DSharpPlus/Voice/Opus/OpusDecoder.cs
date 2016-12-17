using System;
using System.Runtime.InteropServices;

namespace DSharpPlus
{
    internal class OpusDecoder : IDisposable
    {
        IntPtr opusDecoder;

        [DllImport("libopus.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr opus_decoder_create(int fs, int channels, out OpusError error);

        [DllImport("libopus.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern int opus_decode(IntPtr st, byte[] data, int len, IntPtr pcm, int frame_size, int decode_fec);

        [DllImport("libopus.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern void opus_decoder_destroy(IntPtr st);

        public OpusError Errors;
        public int sample_rate { get; internal set; } = 0;
        public int channels { get; internal set; } = 0;

        internal OpusDecoder(int sample_rate, int channels)
        {
            this.sample_rate = sample_rate;
            this.channels = channels;

            opusDecoder = opus_decoder_create(sample_rate, channels, out Errors);

            if (Errors != OpusError.OPUS_OK)
                throw new Exception(Errors.ToString());
        }

        public static OpusDecoder Create(int sample_rate, int channels)
        {
            if (sample_rate != 8000 &&
                sample_rate != 12000 &&
                sample_rate != 16000 &&
                sample_rate != 24000 &&
                sample_rate != 48000)
                throw new ArgumentOutOfRangeException("sample_rate");
            if (channels != 1 && channels != 2)
                throw new ArgumentOutOfRangeException("channels");

            return new OpusDecoder(sample_rate, channels);
        }

        public unsafe byte[] Decode(byte[] inputOpusData, int dataLength, out int decodedLength)
        {
            if (disposed)
                throw new ObjectDisposedException("OpusDecoder");

            IntPtr decodedPtr;
            byte[] decoded = new byte[4000];
            int frameCount = FrameCount(4000);
            int length = 0;
            fixed (byte* bdec = decoded)
            {
                decodedPtr = new IntPtr((void*)bdec);

                if (inputOpusData != null)
                    length = opus_decode(opusDecoder, inputOpusData, dataLength, decodedPtr, frameCount, 0);
            }
            decodedLength = length * 2;
            if (length < 0)
                throw new Exception("Decoding failed - " + ((OpusError)length).ToString());

            return decoded;
        }

        public int FrameCount(int bufferSize)
        {
            //  seems like bitrate should be required
            int bitrate = 16;
            int bytesPerSample = (bitrate / 8) * channels;
            return bufferSize / bytesPerSample;
        }

        ~OpusDecoder()
        {
            Dispose();
        }

        private bool disposed;
        public void Dispose()
        {
            if (disposed)
                return;

            GC.SuppressFinalize(this);

            if (opusDecoder != IntPtr.Zero)
            {
                opus_decoder_destroy(opusDecoder);
                opusDecoder = IntPtr.Zero;
            }

            disposed = true;
        }
    }
}
