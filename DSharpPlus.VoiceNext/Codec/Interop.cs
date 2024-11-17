using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;

namespace DSharpPlus.VoiceNext.Codec;

/// <summary>
/// This is an interop class. It contains wrapper methods for Opus and Sodium.
/// </summary>
internal static unsafe partial class Interop
{
    #region Sodium wrapper
    private const string SodiumLibraryName = "libsodium";

    /// <summary>
    /// Gets the Sodium key size for xsalsa20_poly1305 algorithm.
    /// </summary>
    public static int SodiumKeySize { get; } = (int)crypto_aead_aes256gcm_keybytes();

    /// <summary>
    /// Gets the Sodium nonce size for xsalsa20_poly1305 algorithm.
    /// </summary>
    public static int SodiumNonceSize { get; } = (int)crypto_aead_aes256gcm_npubbytes();

    public static int SodiumMacSize { get; } = (int)crypto_aead_aes256gcm_abytes();

    /// <summary>
    /// Indicates whether the current hardware is AEAD AES-256 GCM compatible.
    /// </summary>
    /// <returns></returns>
    public static bool IsAeadAes256GcmCompatible()
        => crypto_aead_aes256gcm_is_available() == 0;

    [LibraryImport(SodiumLibraryName)]
    private static partial int crypto_aead_aes256gcm_is_available();

    [LibraryImport(SodiumLibraryName)]
    private static partial nuint crypto_aead_aes256gcm_npubbytes();

    [LibraryImport(SodiumLibraryName)]
    private static partial nuint crypto_aead_aes256gcm_abytes();

    [LibraryImport(SodiumLibraryName)]
    private static partial nuint crypto_aead_aes256gcm_keybytes();

    [LibraryImport(SodiumLibraryName)]
    private static partial int crypto_aead_aes256gcm_encrypt
    (
        byte* encrypted,                                        // unsigned char *c
        ulong *encryptedLength,                                 // unsigned long long *clen_p
        byte* message,                                          // const unsigned char *m
        ulong messageLength,                                    // unsigned long long mlen
        // non-confidential data appended to the message
        byte* ad,                                               // const unsigned char *ad
        ulong adLength,                                         // unsigned long long adlen
        // unused, should be null
        byte* nonceSecret,                                      // const unsigned char *nsec
        byte* noncePublic,                                      // const unsigned char *npub
        byte* key                                               // const unsigned char *k
    );

    [LibraryImport(SodiumLibraryName)]
    private static partial int crypto_aead_aes256gcm_decrypt
    (
        byte* message,                                          // unsigned char *m
        ulong* messageLength,                                   // unsigned long long *mlen_p
        // unused, should be null
        byte* nonceSecret,                                      // unsigned char *nsec
        byte* encrypted,                                        // const unsigned char *c
        ulong encryptedLength,                                  // unsigned long long clen
        // non-confidential data appended to the message
        byte* ad,                                               // const unsigned char *ad
        ulong adLength,                                         // unsigned long long adlen
        byte* noncePublic,                                      // const unsigned char *npub
        byte* key                                               // const unsigned char *p
    );

    /// <summary>
    /// Encrypts supplied buffer using xsalsa20_poly1305 algorithm, using supplied key and nonce to perform encryption.
    /// </summary>
    /// <param name="source">Contents to encrypt.</param>
    /// <param name="target">Buffer to encrypt to.</param>
    /// <param name="key">Key to use for encryption.</param>
    /// <param name="nonce">Nonce to use for encryption.</param>
    /// <returns>Encryption status.</returns>
    public static unsafe void Encrypt(ReadOnlySpan<byte> source, Span<byte> target, ReadOnlySpan<byte> key, ReadOnlySpan<byte> nonce)
    {
        ulong targetLength = (ulong)target.Length;

        fixed (byte* pSource = source)
        fixed (byte* pTarget = target)
        fixed (byte* pKey = key)
        fixed (byte* pNonce = nonce)
        {
            crypto_aead_aes256gcm_encrypt(pTarget, &targetLength, pSource, (ulong)source.Length, null, 0, null, pNonce, pKey); 
        }
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
        ulong targetLength = (ulong)target.Length;

        fixed (byte* pSource = source)
        fixed (byte* pTarget = target)
        fixed (byte* pKey = key)
        fixed (byte* pNonce = nonce)
        {
            return crypto_aead_aes256gcm_decrypt(pTarget, &targetLength, null, pSource, (ulong)source.Length, null, 0, pNonce, pKey);
        }
    }
    #endregion

    #region Opus wrapper
    private const string OpusLibraryName = "libopus";

    [LibraryImport(OpusLibraryName, EntryPoint = "opus_encoder_create")]
    private static partial IntPtr OpusCreateEncoder(int sampleRate, int channels, int application, out OpusError error);

    [LibraryImport(OpusLibraryName, EntryPoint = "opus_encoder_destroy")]
    public static partial void OpusDestroyEncoder(IntPtr encoder);

    [LibraryImport(OpusLibraryName, EntryPoint = "opus_encode")]
    private static unsafe partial int OpusEncode(IntPtr encoder, byte* pcmData, int frameSize, byte* data, int maxDataBytes);

    [LibraryImport(OpusLibraryName, EntryPoint = "opus_encoder_ctl")]
    private static partial OpusError OpusEncoderControl(IntPtr encoder, OpusControl request, int value);

    [LibraryImport(OpusLibraryName, EntryPoint = "opus_decoder_create")]
    private static partial IntPtr OpusCreateDecoder(int sampleRate, int channels, out OpusError error);

    [LibraryImport(OpusLibraryName, EntryPoint = "opus_decoder_destroy")]
    public static partial void OpusDestroyDecoder(IntPtr decoder);

    [LibraryImport(OpusLibraryName, EntryPoint = "opus_decode")]
    private static unsafe partial int OpusDecode(IntPtr decoder, byte* opusData, int opusDataLength, byte* data, int frameSize, int decodeFec);

    [LibraryImport(OpusLibraryName, EntryPoint = "opus_packet_get_nb_channels")]
    private static unsafe partial int OpusGetPacketChanelCount(byte* opusData);

    [LibraryImport(OpusLibraryName, EntryPoint = "opus_packet_get_nb_frames")]
    private static unsafe partial int OpusGetPacketFrameCount(byte* opusData, int length);

    [LibraryImport(OpusLibraryName, EntryPoint = "opus_packet_get_samples_per_frame")]
    private static unsafe partial int OpusGetPacketSamplePerFrameCount(byte* opusData, int samplingRate);

    [LibraryImport(OpusLibraryName, EntryPoint = "opus_decoder_ctl")]
    private static partial int OpusDecoderControl(IntPtr decoder, OpusControl request, out int value);

    public static IntPtr OpusCreateEncoder(AudioFormat audioFormat)
    {
        nint encoder = OpusCreateEncoder(audioFormat.SampleRate, audioFormat.ChannelCount, (int)audioFormat.VoiceApplication, out OpusError error);
        return error != OpusError.Ok ? throw new Exception($"Could not instantiate Opus encoder: {error} ({(int)error}).") : encoder;
    }

    public static void OpusSetEncoderOption(IntPtr encoder, OpusControl option, int value)
    {
        OpusError error;
        if ((error = OpusEncoderControl(encoder, option, value)) != OpusError.Ok)
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
            len = OpusEncode(encoder, pcmPtr, frameSize, opusPtr, opus.Length);
        }

        if (len < 0)
        {
            OpusError error = (OpusError)len;
            throw new Exception($"Could not encode PCM data to Opus: {error} ({(int)error}).");
        }

        opus = opus[..len];
    }

    public static IntPtr OpusCreateDecoder(AudioFormat audioFormat)
    {
        nint decoder = OpusCreateDecoder(audioFormat.SampleRate, audioFormat.ChannelCount, out OpusError error);
        return error != OpusError.Ok ? throw new Exception($"Could not instantiate Opus decoder: {error} ({(int)error}).") : decoder;
    }

    public static unsafe int OpusDecode(IntPtr decoder, ReadOnlySpan<byte> opus, int frameSize, Span<byte> pcm, bool useFec)
    {
        int len = 0;

        fixed (byte* opusPtr = &opus.GetPinnableReference())
        fixed (byte* pcmPtr = &pcm.GetPinnableReference())
        {
            len = OpusDecode(decoder, opusPtr, opus.Length, pcmPtr, frameSize, useFec ? 1 : 0);
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
            len = OpusDecode(decoder, null, 0, pcmPtr, frameSize, 1);
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
            frames = OpusGetPacketFrameCount(opusPtr, opus.Length);
            samplesPerFrame = OpusGetPacketSamplePerFrameCount(opusPtr, samplingRate);
            channels = OpusGetPacketChanelCount(opusPtr);
        }

        frameSize = frames * samplesPerFrame;
    }

    [SuppressMessage("Quality Assurance", "CA1806:OpusGetLastPacketDuration calls OpusDecoderControl but does not use the HRESULT or error code that the method returns. This could lead to unexpected behavior in error conditions or low-resource situations. Use the result in a conditional statement, assign the result to a variable, or pass it as an argument to another method.",
        Justification = "It's VoiceNext and I don't care - Lunar")]
    public static void OpusGetLastPacketDuration(IntPtr decoder, out int sampleCount) => OpusDecoderControl(decoder, OpusControl.GetLastPacketDuration, out sampleCount);
    #endregion
}
