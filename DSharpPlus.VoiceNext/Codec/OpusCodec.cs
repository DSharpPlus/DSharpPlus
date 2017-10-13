using System;
using System.Linq;
using System.Runtime.InteropServices;

namespace DSharpPlus.VoiceNext.Codec
{
    internal sealed class OpusCodec : IDisposable
    {
        [DllImport("libopus", CallingConvention = CallingConvention.Cdecl, EntryPoint = "opus_encoder_create")]
        private static extern IntPtr CreateEncoder(int samplerate, int channels, int application, out OpusError error);

        [DllImport("libopus", CallingConvention = CallingConvention.Cdecl, EntryPoint = "opus_encoder_destroy")]
        private static extern void DestroyEncoder(IntPtr encoder);

        [DllImport("libopus", CallingConvention = CallingConvention.Cdecl, EntryPoint = "opus_encode")]
        private static extern int Encode(IntPtr encoder, byte[] pcm, int frameSize, IntPtr data, int maxDataBytes);

        [DllImport("libopus", CallingConvention = CallingConvention.Cdecl, EntryPoint = "opus_encoder_ctl")]
        private static extern OpusError EncoderControl(IntPtr encoder, OpusControl request, int value);

#if !NETSTANDARD1_1
        [DllImport("libopus", CallingConvention = CallingConvention.Cdecl, EntryPoint = "opus_decoder_create")]
        private static extern IntPtr CreateDecoder(int samplerate, int channels, out OpusError error);

        [DllImport("libopus", CallingConvention = CallingConvention.Cdecl, EntryPoint = "opus_decoder_destroy")]
        private static extern void DestroyDecoder(IntPtr decoder);

        [DllImport("libopus", CallingConvention = CallingConvention.Cdecl, EntryPoint = "opus_decode")]
        private static extern int Decode(IntPtr decoder, byte[] opus, int frameSize, IntPtr data, int maxDataBytes, int decodeFec);

        public const int PcmSampleSize = 3840;
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

        public OpusCodec(int samplerate, int channels, VoiceApplication application)
        {
            if (!AllowedSampleRates.Contains(samplerate))
            {
                throw new ArgumentOutOfRangeException(nameof(samplerate), string.Concat("Sample rate must be one of ", string.Join(", ", AllowedSampleRates)));
            }

            if (!AllowedChannelCounts.Contains(channels))
            {
                throw new ArgumentOutOfRangeException(nameof(channels), string.Concat("Channel count must be one of ", string.Join(", ", AllowedChannelCounts)));
            }

            SampleRate = samplerate;
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

        public unsafe byte[] Encode(byte[] pcmInput, int offset, int count, int bitrate = 16)
        {
            if (IsDisposed)
            {
                throw new ObjectDisposedException(nameof(Encoder), "Encoder is disposed");
            }

            var frame = new byte[count];
            Array.Copy(pcmInput, offset, frame, 0, frame.Length);

            var frameSize = FrameCount(frame.Length, bitrate);
            var enc = new byte[frame.Length];
            int len;

            fixed (byte* encptr = enc)
            {
                var encdata = new IntPtr(encptr);
                len = Encode(Encoder, frame, frameSize, encdata, enc.Length);
            }

            if (len < 0)
            {
                throw new Exception(string.Concat("OPUS encoding failed (", (OpusError)len, ")"));
            }

            Array.Resize(ref enc, len);
            return enc;
        }

#if !NETSTANDARD1_1
        public unsafe byte[] Decode(byte[] opusInput, int offset, int count, int bitrate = 16)
        {
            if (IsDisposed)
            {
                throw new ObjectDisposedException(nameof(Decoder), "Decoder is disposed");
            }

            var frame = new byte[PcmSampleSize];
            
            var frameSize = FrameCount(frame.Length, bitrate);
            int len;
            var opus = new byte[count];
            Array.Copy(opusInput, offset, opus, 0, count);

            fixed (byte* decptr = frame)
            {
                var decdata = new IntPtr(decptr);
                len = Decode(Decoder, opus, frameSize, decdata, frame.Length, 0);
            }

            if (len < 0)
            {
                throw new Exception(string.Concat("OPUS decoding failed (", (OpusError)len, ")"));
            }

            //Array.Resize(ref frame, len * this.Channels * 2);
            return frame;
        }
#endif

        public void Dispose()
        {
            if (IsDisposed)
            {
                return;
            }

            if (Encoder != IntPtr.Zero)
            {
                DestroyEncoder(Encoder);
            }
            Encoder = IntPtr.Zero;

#if !NETSTANDARD1_1
            if (Decoder != IntPtr.Zero)
            {
                DestroyDecoder(Decoder);
            }
            Decoder = IntPtr.Zero;
#endif

            IsDisposed = true;
        }

        private int FrameCount(int length, int bitrate)
        {
            int bps = (bitrate >> 3) << 1; // (bitrate / 8) * 2;
            return length / bps;
        }

        private void CheckForError(OpusError err)
        {
            if (err < 0)
            {
                throw new Exception(string.Concat("Opus returned an error: ", err.ToString()));
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

    internal enum OpusControl
    {
        SetBitrate = 4002,
        SetBandwidth = 4008,
        SetInBandFec = 4012,
        SetPacketLossPercent = 4014,
        SetSignal = 4024,
        ResetState = 4028
    }

    internal enum OpusSignal
    {
        Auto = -1000,
        Voice = 3001,
        Music = 3002,
    }

    public enum VoiceApplication
    {
        Voice = 2048,
        Music = 2049,
        LowLatency = 2051
    }
}
