using System;
using System.Runtime.InteropServices;

namespace DSharpPlus
{
    internal class OpusEncoder : IDisposable
    {
        IntPtr opusEncoder;

        [DllImport("libopus.dll", CallingConvention = CallingConvention.Cdecl)]
        internal static extern IntPtr opus_encoder_create(int fs, int channels, int application, out OpusError error);

        [DllImport("libopus.dll", CallingConvention = CallingConvention.Cdecl)]
        internal static extern int opus_encode(IntPtr st, byte[] pcm, int frame_size, IntPtr data, int max_data_bytes);

        [DllImport("libopus.dll", CallingConvention = CallingConvention.Cdecl)]
        internal static extern void opus_encoder_destroy(IntPtr encoder);

        public OpusError Errors;
        public int sample_rate { get; internal set; } = 0;
        public int channels { get; internal set; } = 0;

        internal OpusEncoder(int sample_rate, int channels, VoiceApplication application)
        {
            this.sample_rate = sample_rate;
            this.channels = channels;

            opusEncoder = opus_encoder_create(sample_rate, channels, (int)application, out Errors);

            if (Errors != OpusError.OPUS_OK)
                throw new Exception(Errors.ToString());
        }

        public static OpusEncoder Create(int sample_rate, int channels, VoiceApplication application)
        {
            if (sample_rate != 8000 &&
                sample_rate != 12000 &&
                sample_rate != 16000 &&
                sample_rate != 24000 &&
                sample_rate != 48000)
                throw new ArgumentOutOfRangeException("sample_rate");
            if (channels != 1 && channels != 2)
                throw new ArgumentOutOfRangeException("channels");

            return new OpusEncoder(sample_rate, channels, application);
        }

        public unsafe byte[] Encode(byte[] inputPcmSamples, int sampleLength, out int encodedLength)
        {
            if (disposed)
                throw new ObjectDisposedException("OpusEncoder");

            int frames = FrameCount(inputPcmSamples);
            IntPtr encodedPtr;
            byte[] encoded = new byte[4000];
            int length = 0;
            fixed (byte* benc = encoded)
            {
                encodedPtr = new IntPtr((void*)benc);
                length = opus_encode(opusEncoder, inputPcmSamples, frames, encodedPtr, sampleLength);
            }
            encodedLength = length;
            if (length < 0)
                throw new Exception("Encoding failed - " + ((OpusError)length).ToString());

            return encoded;
        }

        public int FrameCount(byte[] pcmSamples)
        {
            int bitrate = 16;
            int bytesPerSample = (bitrate / 8) * channels;
            return pcmSamples.Length / bytesPerSample;
        }

        ~OpusEncoder()
        {
            Dispose();
        }

        private bool disposed;
        public void Dispose()
        {
            if (disposed)
                return;

            GC.SuppressFinalize(this);

            if (opusEncoder != IntPtr.Zero)
            {
                opus_encoder_destroy(opusEncoder);
                opusEncoder = IntPtr.Zero;
            }

            disposed = true;
        }
    }
}
