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

namespace DSharpPlus.VoiceNext.Codec;

/// <summary>
/// This is an interop class. It contains wrapper methods for Opus and Sodium.
/// </summary>
internal static class Interop
{
    #region Sodium wrapper
    private const string SodiumLibraryName = "libsodium";

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

    [DllImport(SodiumLibraryName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "crypto_secretbox_xsalsa20poly1305_keybytes")]
    [return: MarshalAs(UnmanagedType.SysUInt)]
    private static extern UIntPtr _SodiumSecretBoxKeySize();

    [DllImport(SodiumLibraryName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "crypto_secretbox_xsalsa20poly1305_noncebytes")]
    [return: MarshalAs(UnmanagedType.SysUInt)]
    private static extern UIntPtr _SodiumSecretBoxNonceSize();

    [DllImport(SodiumLibraryName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "crypto_secretbox_xsalsa20poly1305_macbytes")]
    [return: MarshalAs(UnmanagedType.SysUInt)]
    private static extern UIntPtr _SodiumSecretBoxMacSize();

    [DllImport(SodiumLibraryName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "crypto_secretbox_easy")]
    private static extern unsafe int _SodiumSecretBoxCreate(byte* buffer, byte* message, ulong messageLength, byte* nonce, byte* key);

    [DllImport(SodiumLibraryName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "crypto_secretbox_open_easy")]
    private static extern unsafe int _SodiumSecretBoxOpen(byte* buffer, byte* encryptedMessage, ulong encryptedLength, byte* nonce, byte* key);

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
        int status = 0;
        fixed (byte* sourcePtr = &source.GetPinnableReference())
        fixed (byte* targetPtr = &target.GetPinnableReference())
        fixed (byte* keyPtr = &key.GetPinnableReference())
        fixed (byte* noncePtr = &nonce.GetPinnableReference())
        {
            status = _SodiumSecretBoxCreate(targetPtr, sourcePtr, (ulong)source.Length, noncePtr, keyPtr);
        }

        return status;
    }

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
        int status = 0;
        fixed (byte* sourcePtr = &source.GetPinnableReference())
        fixed (byte* targetPtr = &target.GetPinnableReference())
        fixed (byte* keyPtr = &key.GetPinnableReference())
        fixed (byte* noncePtr = &nonce.GetPinnableReference())
        {
            status = _SodiumSecretBoxOpen(targetPtr, sourcePtr, (ulong)source.Length, noncePtr, keyPtr);
        }

        return status;
    }
    #endregion

    #region Opus wrapper
    private const string OpusLibraryName = "libopus";

    [DllImport(OpusLibraryName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "opus_encoder_create")]
    private static extern IntPtr _OpusCreateEncoder(int sampleRate, int channels, int application, out OpusError error);

    [DllImport(OpusLibraryName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "opus_encoder_destroy")]
    public static extern void OpusDestroyEncoder(IntPtr encoder);

    [DllImport(OpusLibraryName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "opus_encode")]
    private static extern unsafe int _OpusEncode(IntPtr encoder, byte* pcmData, int frameSize, byte* data, int maxDataBytes);

    [DllImport(OpusLibraryName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "opus_encoder_ctl")]
    private static extern OpusError _OpusEncoderControl(IntPtr encoder, OpusControl request, int value);

    [DllImport(OpusLibraryName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "opus_decoder_create")]
    private static extern IntPtr _OpusCreateDecoder(int sampleRate, int channels, out OpusError error);

    [DllImport(OpusLibraryName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "opus_decoder_destroy")]
    public static extern void OpusDestroyDecoder(IntPtr decoder);

    [DllImport(OpusLibraryName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "opus_decode")]
    private static extern unsafe int _OpusDecode(IntPtr decoder, byte* opusData, int opusDataLength, byte* data, int frameSize, int decodeFec);

    [DllImport(OpusLibraryName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "opus_packet_get_nb_channels")]
    private static extern unsafe int _OpusGetPacketChanelCount(byte* opusData);

    [DllImport(OpusLibraryName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "opus_packet_get_nb_frames")]
    private static extern unsafe int _OpusGetPacketFrameCount(byte* opusData, int length);

    [DllImport(OpusLibraryName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "opus_packet_get_samples_per_frame")]
    private static extern unsafe int _OpusGetPacketSamplePerFrameCount(byte* opusData, int samplingRate);

    [DllImport(OpusLibraryName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "opus_decoder_ctl")]
    private static extern int _OpusDecoderControl(IntPtr decoder, OpusControl request, out int value);

    public static IntPtr OpusCreateEncoder(AudioFormat audioFormat)
    {
        nint encoder = _OpusCreateEncoder(audioFormat.SampleRate, audioFormat.ChannelCount, (int)audioFormat.VoiceApplication, out OpusError error);
        return error != OpusError.Ok ? throw new Exception($"Could not instantiate Opus encoder: {error} ({(int)error}).") : encoder;
    }

    public static void OpusSetEncoderOption(IntPtr encoder, OpusControl option, int value)
    {
        OpusError error = OpusError.Ok;
        if ((error = _OpusEncoderControl(encoder, option, value)) != OpusError.Ok)
        {
            throw new Exception($"Could not set Opus encoder option: {error} ({(int)error}).");
        }
    }

    public static unsafe void OpusEncode(IntPtr encoder, ReadOnlySpan<byte> pcm, int frameSize, ref Span<byte> opus)
    {
        int len = 0;

        fixed (byte* pcmPtr = &pcm.GetPinnableReference())
        fixed (byte* opusPtr = &opus.GetPinnableReference())
        {
            len = _OpusEncode(encoder, pcmPtr, frameSize, opusPtr, opus.Length);
        }

        if (len < 0)
        {
            OpusError error = (OpusError)len;
            throw new Exception($"Could not encode PCM data to Opus: {error} ({(int)error}).");
        }

        opus = opus.Slice(0, len);
    }

    public static IntPtr OpusCreateDecoder(AudioFormat audioFormat)
    {
        nint decoder = _OpusCreateDecoder(audioFormat.SampleRate, audioFormat.ChannelCount, out OpusError error);
        return error != OpusError.Ok ? throw new Exception($"Could not instantiate Opus decoder: {error} ({(int)error}).") : decoder;
    }

    public static unsafe int OpusDecode(IntPtr decoder, ReadOnlySpan<byte> opus, int frameSize, Span<byte> pcm, bool useFec)
    {
        int len = 0;

        fixed (byte* opusPtr = &opus.GetPinnableReference())
        fixed (byte* pcmPtr = &pcm.GetPinnableReference())
        {
            len = _OpusDecode(decoder, opusPtr, opus.Length, pcmPtr, frameSize, useFec ? 1 : 0);
        }

        if (len < 0)
        {
            OpusError error = (OpusError)len;
            throw new Exception($"Could not decode PCM data from Opus: {error} ({(int)error}).");
        }

        return len;
    }

    public static unsafe int OpusDecode(IntPtr decoder, int frameSize, Span<byte> pcm)
    {
        int len = 0;

        fixed (byte* pcmPtr = &pcm.GetPinnableReference())
        {
            len = _OpusDecode(decoder, null, 0, pcmPtr, frameSize, 1);
        }

        if (len < 0)
        {
            OpusError error = (OpusError)len;
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

    public static void OpusGetLastPacketDuration(IntPtr decoder, out int sampleCount) => _OpusDecoderControl(decoder, OpusControl.GetLastPacketDuration, out sampleCount);
    #endregion
}
