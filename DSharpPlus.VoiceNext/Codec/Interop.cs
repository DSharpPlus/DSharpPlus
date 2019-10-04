﻿using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace DSharpPlus.VoiceNext.Codec
{
    /// <summary>
    /// This is an interop class. It contains wrapper methods for Opus and Sodium.
    /// </summary>
    internal static class Interop
    {
        #region Sodium wrapper
        /// <summary>
        /// Gets the Sodium key size for xsalsa20_poly1305 algorithm.
        /// </summary>
        public static int SodiumKeySize { get; } = (int)_SodiumSecretBoxKeySize();

        /// <summary>
        /// Gets the Sodium nonce size for xsalsa20_poly1305 algorithm.
        /// </summary>
        public static int SodiumNonceSize { get; } = (int)_SodiumSecretBoxNonceSize();

        /// <summary>
        /// Gets the Sodium MAC size for xsalsa20_poly1305 algorithm.
        /// </summary>
        public static int SodiumMacSize { get; } = (int)_SodiumSecretBoxMacSize();

        [DllImport("sodium", CallingConvention = CallingConvention.Cdecl, EntryPoint = "crypto_secretbox_xsalsa20poly1305_keybytes")]
        [return: MarshalAs(UnmanagedType.SysUInt)]
        private static extern UIntPtr _SodiumSecretBoxKeySize();

        [DllImport("sodium", CallingConvention = CallingConvention.Cdecl, EntryPoint = "crypto_secretbox_xsalsa20poly1305_noncebytes")]
        [return: MarshalAs(UnmanagedType.SysUInt)]
        private static extern UIntPtr _SodiumSecretBoxNonceSize();

        [DllImport("sodium", CallingConvention = CallingConvention.Cdecl, EntryPoint = "crypto_secretbox_xsalsa20poly1305_macbytes")]
        [return: MarshalAs(UnmanagedType.SysUInt)]
        private static extern UIntPtr _SodiumSecretBoxMacSize();

        [DllImport("sodium", CallingConvention = CallingConvention.Cdecl, EntryPoint = "crypto_secretbox_easy")]
        private static unsafe extern int _SodiumSecretBoxCreate(byte* buffer, byte* message, ulong messageLength, byte* nonce, byte* key);

#if !NETSTANDARD1_1
        [DllImport("sodium", CallingConvention = CallingConvention.Cdecl, EntryPoint = "crypto_secretbox_open_easy")]
        private static unsafe extern int _SodiumSecretBoxOpen(byte* buffer, byte* encryptedMessage, ulong encryptedLength, byte* nonce, byte* key);
#endif

        /// <summary>
        /// Encrypts supplied buffer using xsalsa20_poly1305 algorithm, using supplied key and nonce to perform encryption.
        /// </summary>
        /// <param name="source">Contents to encrypt.</param>
        /// <param name="target">Buffer to encrypt to.</param>
        /// <param name="key">Key to use for encryption.</param>
        /// <param name="nonce">Nonce to use for encryption.</param>
        /// <returns>Encryption status.</returns>
        public static unsafe int Encrypt(ReadOnlySpan<byte> source, Span<byte> target, ReadOnlySpan<byte> key, ReadOnlySpan<byte> nonce)
        {
            var status = 0;
            fixed (byte* sourcePtr = &source.GetPinnableReference())
            fixed (byte* targetPtr = &target.GetPinnableReference())
            fixed (byte* keyPtr = &key.GetPinnableReference())
            fixed (byte* noncePtr = &nonce.GetPinnableReference())
                status = _SodiumSecretBoxCreate(targetPtr, sourcePtr, (ulong)source.Length, noncePtr, keyPtr);

            return status;
        }

#if !NETSTANDARD1_1
        /// <summary>
        /// Decrypts supplied buffer using xsalsa20_poly1305 algorithm, using supplied key and nonce to perform decryption.
        /// </summary>
        /// <param name="source">Buffer to decrypt from.</param>
        /// <param name="target">Decrypted message buffer.</param>
        /// <param name="key">Key to use for decryption.</param>
        /// <param name="nonce">Nonce to use for decryption.</param>
        /// <returns>Decryption status.</returns>
        public static unsafe int Decrypt(ReadOnlySpan<byte> source, Span<byte> target, ReadOnlySpan<byte> key, ReadOnlySpan<byte> nonce)
        {
            var status = 0;
            fixed (byte* sourcePtr = &source.GetPinnableReference())
            fixed (byte* targetPtr = &target.GetPinnableReference())
            fixed (byte* keyPtr = &key.GetPinnableReference())
            fixed (byte* noncePtr = &nonce.GetPinnableReference())
                status = _SodiumSecretBoxOpen(targetPtr, sourcePtr, (ulong)source.Length, noncePtr, keyPtr);

            return status;
        }
#endif
        #endregion

        #region Opus wrapper
        [DllImport("opus", CallingConvention = CallingConvention.Cdecl, EntryPoint = "opus_encoder_create")]
        private static extern IntPtr _OpusCreateEncoder(int sampleRate, int channels, int application, out OpusError error);

        [DllImport("opus", CallingConvention = CallingConvention.Cdecl, EntryPoint = "opus_encoder_destroy")]
        public static extern void OpusDestroyEncoder(IntPtr encoder);

        [DllImport("opus", CallingConvention = CallingConvention.Cdecl, EntryPoint = "opus_encode")]
        private static unsafe extern int _OpusEncode(IntPtr encoder, byte* pcmData, int frameSize, byte* data, int maxDataBytes);

        [DllImport("opus", CallingConvention = CallingConvention.Cdecl, EntryPoint = "opus_encoder_ctl")]
        private static extern OpusError _OpusEncoderControl(IntPtr encoder, OpusControl request, int value);

#if !NETSTANDARD1_1
        [DllImport("opus", CallingConvention = CallingConvention.Cdecl, EntryPoint = "opus_decoder_create")]
        private static extern IntPtr _OpusCreateDecoder(int sampleRate, int channels, out OpusError error);

        [DllImport("opus", CallingConvention = CallingConvention.Cdecl, EntryPoint = "opus_decoder_destroy")]
        public static extern void OpusDestroyDecoder(IntPtr decoder);

        [DllImport("opus", CallingConvention = CallingConvention.Cdecl, EntryPoint = "opus_decode")]
        private static unsafe extern int _OpusDecode(IntPtr decoder, byte* opusData, int opusDataLength, byte* data, int frameSize, int decodeFec);

        [DllImport("opus", CallingConvention = CallingConvention.Cdecl, EntryPoint = "opus_packet_get_nb_channels")]
        private static unsafe extern int _OpusGetPacketChanelCount(byte* opusData);

        [DllImport("opus", CallingConvention = CallingConvention.Cdecl, EntryPoint = "opus_packet_get_nb_frames")]
        private static unsafe extern int _OpusGetPacketFrameCount(byte* opusData, int length);

        [DllImport("opus", CallingConvention = CallingConvention.Cdecl, EntryPoint = "opus_packet_get_samples_per_frame")]
        private static unsafe extern int _OpusGetPacketSamplePerFrameCount(byte* opusData, int samplingRate);

        [DllImport("opus", CallingConvention = CallingConvention.Cdecl, EntryPoint = "opus_decoder_ctl")]
        private static extern int _OpusDecoderControl(IntPtr decoder, OpusControl request, out int value);
#endif

        public static IntPtr OpusCreateEncoder(AudioFormat audioFormat)
        {
            var encoder = _OpusCreateEncoder(audioFormat.SampleRate, audioFormat.ChannelCount, (int)audioFormat.VoiceApplication, out var error);
            if (error != OpusError.Ok)
                throw new Exception($"Could not instantiate Opus encoder: {error} ({(int)error}).");

            return encoder;
        }

        public static void OpusSetEncoderOption(IntPtr encoder, OpusControl option, int value)
        {
            var error = OpusError.Ok;
            if ((error = _OpusEncoderControl(encoder, option, value)) != OpusError.Ok)
                throw new Exception($"Could not set Opus encoder option: {error} ({(int)error}).");
        }

        public static unsafe void OpusEncode(IntPtr encoder, ReadOnlySpan<byte> pcm, int frameSize, ref Span<byte> opus)
        {
            var len = 0;

            fixed (byte* pcmPtr = &pcm.GetPinnableReference())
            fixed (byte* opusPtr = &opus.GetPinnableReference())
                len = _OpusEncode(encoder, pcmPtr, frameSize, opusPtr, opus.Length);

            if (len < 0)
            {
                var error = (OpusError)len;
                throw new Exception($"Could not encode PCM data to Opus: {error} ({(int)error}).");
            }

            opus = opus.Slice(0, len);
        }

#if !NETSTANDARD1_1
        public static IntPtr OpusCreateDecoder(AudioFormat audioFormat)
        {
            var decoder = _OpusCreateDecoder(audioFormat.SampleRate, audioFormat.ChannelCount, out var error);
            if (error != OpusError.Ok)
                throw new Exception($"Could not instantiate Opus decoder: {error} ({(int)error}).");

            return decoder;
        }

        public static unsafe int OpusDecode(IntPtr decoder, ReadOnlySpan<byte> opus, int frameSize, Span<byte> pcm, bool useFec)
        {
            var len = 0;

            fixed (byte* opusPtr = &opus.GetPinnableReference())
            fixed (byte* pcmPtr = &pcm.GetPinnableReference())
                len = _OpusDecode(decoder, opusPtr, opus.Length, pcmPtr, frameSize, useFec ? 1 : 0);

            if (len < 0)
            {
                var error = (OpusError)len;
                throw new Exception($"Could not decode PCM data from Opus: {error} ({(int)error}).");
            }

            return len;
        }

        public static unsafe int OpusDecode(IntPtr decoder, int frameSize, Span<byte> pcm)
        {
            var len = 0;
            
            fixed (byte* pcmPtr = &pcm.GetPinnableReference())
                len = _OpusDecode(decoder, null, 0, pcmPtr, frameSize, 1);

            if (len < 0)
            {
                var error = (OpusError)len;
                throw new Exception($"Could not decode PCM data from Opus: {error} ({(int)error}).");
            }

            return len;
        }

        public static unsafe void OpusGetPacketMetrics(ReadOnlySpan<byte> opus, int samplingRate, out int channels, out int frames, out int samplesPerFrame, out int frameSize)
        {
            fixed (byte* opusPtr = &opus.GetPinnableReference())
            {
                frames = _OpusGetPacketFrameCount(opusPtr, opus.Length);
                samplesPerFrame = _OpusGetPacketSamplePerFrameCount(opusPtr, samplingRate);
                channels = _OpusGetPacketChanelCount(opusPtr);
            }

            frameSize = frames * samplesPerFrame;
        }

        public static void OpusGetLastPacketDuration(IntPtr decoder, out int sampleCount)
        {
            _OpusDecoderControl(decoder, OpusControl.GetLastPacketDuration, out sampleCount);
        }
#endif
#endregion
    }
}
