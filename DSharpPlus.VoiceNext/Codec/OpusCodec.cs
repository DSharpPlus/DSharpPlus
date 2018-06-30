using System;
using System.Linq;
using System.Runtime.InteropServices;

namespace DSharpPlus.VoiceNext.Codec
{
    internal sealed class OpusCodec : IDisposable
    {
        [DllImport("libopus", CallingConvention = CallingConvention.Cdecl, EntryPoint = "opus_encoder_create")]
        private static extern IntPtr CreateEncoder(int sampleRate, int channels, int application, out OpusError error);

        [DllImport("libopus", CallingConvention = CallingConvention.Cdecl, EntryPoint = "opus_encoder_destroy")]
        private static extern void DestroyEncoder(IntPtr encoder);

        [DllImport("libopus", CallingConvention = CallingConvention.Cdecl, EntryPoint = "opus_encode")]
        private static extern int Encode(IntPtr encoder, byte[] pcmData, int frameSize, IntPtr data, int maxDataBytes);

        [DllImport("libopus", CallingConvention = CallingConvention.Cdecl, EntryPoint = "opus_encoder_ctl")]
        private static extern OpusError EncoderControl(IntPtr encoder, OpusControl request, int value);

#if !NETSTANDARD1_1
        [DllImport("libopus", CallingConvention = CallingConvention.Cdecl, EntryPoint = "opus_decoder_create")]
        private static extern IntPtr CreateDecoder(int sampleRate, int channels, out OpusError error);

        [DllImport("libopus", CallingConvention = CallingConvention.Cdecl, EntryPoint = "opus_decoder_destroy")]
        private static extern void DestroyDecoder(IntPtr decoder);

        [DllImport("libopus", CallingConvention = CallingConvention.Cdecl, EntryPoint = "opus_decode")]
        private static extern int Decode(IntPtr decoder, byte[] opusData, int frameSize, IntPtr data, int maxDataBytes, int decodeFec);

        public const int PCM_SAMPLE_SIZE = 3840;
#endif

        private IntPtr Encoder { get; set; }
#if !NETSTANDARD1_1
        private IntPtr Decoder { get; set; }
#endif
        private bool IsDisposed { get; set; }

        private int SampleRate { get; set; }
        private int Channels { get; set; }
        private VoiceApplication Application { get; set; }

        private static int[] AllowedSampleRates { get; set; }
        private static int[] AllowedChannelCounts { get; set; }

        public OpusCodec(int sampleRate, int channels, VoiceApplication application)
        {
            if (!AllowedSampleRates.Contains(sampleRate))
                throw new ArgumentOutOfRangeException(nameof(sampleRate), string.Concat("Sample rate must be one of ", string.Join(", ", AllowedSampleRates)));

            if (!AllowedChannelCounts.Contains(channels))
                throw new ArgumentOutOfRangeException(nameof(channels), string.Concat("Channel count must be one of ", string.Join(", ", AllowedChannelCounts)));

            SampleRate = sampleRate;
            Channels = channels;
            Application = application;

            Encoder = CreateEncoder(SampleRate, Channels, (int)Application, out var err);
            CheckForError(err);

            var sig = OpusSignal.Auto;
            switch (application)
            {
                case VoiceApplication.Music:
                    sig = OpusSignal.Music;
                    break;

                case VoiceApplication.Voice:
                    sig = OpusSignal.Voice;
                    break;
            }

            err = EncoderControl(Encoder, OpusControl.SetSignal, (int)sig);
            CheckForError(err);

            err = EncoderControl(Encoder, OpusControl.SetPacketLossPercent, 15);
            CheckForError(err);

            err = EncoderControl(Encoder, OpusControl.SetInBandFec, 1);
            CheckForError(err);

            err = EncoderControl(Encoder, OpusControl.SetBitrate, 131072);
            CheckForError(err);

#if !NETSTANDARD1_1
            Decoder = CreateDecoder(SampleRate, Channels, out err);
            CheckForError(err);
#endif
        }

        ~OpusCodec()
        {
            Dispose();
        }

        static OpusCodec()
        {
            AllowedSampleRates = new[] { 8000, 12000, 16000, 24000, 48000 };
            AllowedChannelCounts = new[] { 1, 2 };
        }

        public unsafe byte[] Encode(byte[] pcmData, int offset, int count, int bitRate = 16)
        {
            if (IsDisposed)
                throw new ObjectDisposedException(nameof(Encoder), "Encoder is disposed");

            var frame = new byte[count];
            Array.Copy(pcmData, offset, frame, 0, frame.Length);

            var frame_size = FrameCount(frame.Length, bitRate);
            var encdata = IntPtr.Zero;
            var enc = new byte[frame.Length];
            int len = 0;

            fixed (byte* encptr = enc)
            {
                encdata = new IntPtr(encptr);
                len = Encode(Encoder, frame, frame_size, encdata, enc.Length);
            }

            if (len < 0)
                throw new Exception(string.Concat("OPUS encoding failed (", (OpusError)len, ")"));

            Array.Resize(ref enc, len);
            return enc;
        }

#if !NETSTANDARD1_1
        public unsafe byte[] Decode(byte[] opusData, int offset, int count, int bitRate = 16)
        {
            if (IsDisposed)
                throw new ObjectDisposedException(nameof(Decoder), "Decoder is disposed");

            var frame = new byte[PCM_SAMPLE_SIZE];
            
            var frame_size = FrameCount(frame.Length, bitRate);
            var decdata = IntPtr.Zero;
            var len = 0;
            var opus = new byte[count];
            Array.Copy(opusData, offset, opus, 0, count);

            fixed (byte* decptr = frame)
            {
                decdata = new IntPtr(decptr);
                len = Decode(Decoder, opus, frame_size, decdata, frame.Length, 0);
            }

            if (len < 0)
                throw new Exception(string.Concat("OPUS decoding failed (", (OpusError)len, ")"));

            //Array.Resize(ref frame, len * this.Channels * 2);
            return frame;
        }
#endif

        public void Dispose()
        {
            if (IsDisposed)
                return;
            
            if (Encoder != IntPtr.Zero)
                DestroyEncoder(Encoder);
            Encoder = IntPtr.Zero;

#if !NETSTANDARD1_1
            if (Decoder != IntPtr.Zero)
                DestroyDecoder(Decoder);
            Decoder = IntPtr.Zero;
#endif

            IsDisposed = true;
        }

        private int FrameCount(int length, int bitRate)
        {
            var bps = (bitRate >> 2) & ~1; // (bitrate / 8) * 2;
            return length / bps;
        }

        private void CheckForError(OpusError err)
        {
            if (err < 0)
                throw new Exception(string.Concat("Opus returned an error: ", err.ToString()));
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
