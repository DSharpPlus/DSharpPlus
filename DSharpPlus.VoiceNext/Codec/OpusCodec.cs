using System;
using System.Linq;
using System.Runtime.InteropServices;

namespace DSharpPlus.VoiceNext.Codec
{
    public sealed class OpusCodec : IDisposable
    {
        [DllImport("libopus.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "opus_encoder_create")]
        private static extern IntPtr CreateEncoder(int samplerate, int channels, int application, out OpusError error);

        [DllImport("libopus.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "opus_encoder_destroy")]
        private static extern void DestroyEncoder(IntPtr encoder);

        [DllImport("libopus.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "opus_encode")]
        private static extern int Encode(IntPtr encoder, byte[] pcm, int frame_size, IntPtr data, int max_data_bytes);

        private IntPtr Encoder { get; set; }
        private OpusError Errors { get; set; }
        private bool IsDisposed { get; set; }

        private int SampleRate { get; set; }
        private int Channels { get; set; }
        private VoiceApplication Application { get; set; }

        private static int[] AllowedSampleRates { get; set; }
        private static int[] AllowedChannelCounts { get; set; }

        public OpusCodec(int samplerate, int channels, VoiceApplication application)
        {
            if (!AllowedSampleRates.Contains(samplerate))
                throw new ArgumentOutOfRangeException(nameof(samplerate), string.Concat("Sample rate must be one of ", string.Join(", ", AllowedSampleRates)));

            if (!AllowedChannelCounts.Contains(channels))
                throw new ArgumentOutOfRangeException(nameof(channels), string.Concat("Channel count must be one of ", string.Join(", ", AllowedChannelCounts)));

            this.SampleRate = samplerate;
            this.Channels = channels;
            this.Application = application;

            var err = OpusError.OPUS_OK;
            this.Encoder = CreateEncoder(this.SampleRate, this.Channels, (int)this.Application, out err);
            this.Errors = err;
            if (this.Errors != OpusError.OPUS_OK)
                throw new Exception(this.Errors.ToString());
        }

        ~OpusCodec()
        {
            this.Dispose();
        }

        static OpusCodec()
        {
            AllowedSampleRates = new[] { 8000, 12000, 16000, 24000, 48000 };
            AllowedChannelCounts = new[] { 1, 2 };
        }

        public unsafe byte[] Encode(byte[] pcm_input, int offset, int count, int bitrate = 16)
        {
            if (this.IsDisposed)
                throw new ObjectDisposedException(nameof(this.Encoder), "Encoder is disposed");

            var frame = new byte[count];
            Array.Copy(pcm_input, offset, frame, 0, frame.Length);

            var frame_size = this.FrameCount(frame.Length, bitrate);
            var encdata = IntPtr.Zero;
            var enc = new byte[frame.Length];
            int len = 0;

            fixed (byte* encptr = enc)
            {
                encdata = new IntPtr(encptr);
                len = Encode(this.Encoder, frame, frame_size, encdata, enc.Length);
            }

            if (len < 0)
                throw new Exception(string.Concat("OPUS encoding failed (", (OpusError)len, ")"));

            Array.Resize(ref enc, len);
            return enc;
        }

        public void Dispose()
        {
            if (this.IsDisposed)
                return;
            
            if (this.Encoder != IntPtr.Zero)
                DestroyEncoder(this.Encoder);

            this.Encoder = IntPtr.Zero;
            this.IsDisposed = true;
        }

        private int FrameCount(int length, int bitrate)
        {
            int bps = (bitrate >> 3) << 1; // (bitrate / 8) * 2;
            return length / bps;
        }
    }

    [Flags]
    internal enum OpusError
    {
        OPUS_OK = 0,
        OPUS_BAD_ARG,
        OPUS_BUFFER_TO_SMALL,
        OPUS_INTERNAL_ERROR,
        OPUS_INVALID_PACKET,
        OPUS_UNIMPLEMENTED,
        OPUS_INVALID_STATE,
        OPUS_ALLOC_FAIL
    }

    public enum VoiceApplication : int
    {
        Voice = 2048,
        Music = 2049,
        LowLatency = 2051
    }
}
