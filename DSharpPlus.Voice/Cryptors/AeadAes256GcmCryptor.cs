using System;
using System.Buffers.Binary;
using System.Diagnostics;
using System.Threading;

using CommunityToolkit.HighPerformance;
using CommunityToolkit.HighPerformance.Buffers;

using DSharpPlus.Voice.Interop.Sodium;
using DSharpPlus.Voice.Protocol.Rtp;

namespace DSharpPlus.Voice.Cryptors;

/// <summary>
/// The default cryptor for AEAD AES256-GCM.
/// </summary>
public sealed class AeadAes256GcmCryptor : ICryptor
{
    private readonly byte[] key;
    private uint nonce = 0;

    /// <summary>
    /// Creates a new AEAD AES256-GCM cryptor with the specified key.
    /// </summary>
    public AeadAes256GcmCryptor(byte[] key)
        => this.key = key;

    /// <inheritdoc/>
    public string EncryptionMode => "aead_aes256_gcm_rtpsize";

    /// <inheritdoc/>
    public void Decrypt(ReadOnlySpan<byte> encryptedFrame, ArrayPoolBufferWriter<byte> decrypted, out int extensionHeaderLength)
    {
        RtpFrameInfo frameInfo = FrameParser.ParseRtpsizeSuffixedNonce(encryptedFrame, 4);
        extensionHeaderLength = frameInfo.ExtensionHeaderLength;

        Span<byte> nonce = stackalloc byte[SodiumInterop.AeadAes256GcmNonceLength];
        encryptedFrame[frameInfo.Nonce].CopyTo(nonce);

        ReadOnlySpan<byte> encryptedData = encryptedFrame[frameInfo.VoiceData];
        ReadOnlySpan<byte> additionalData = encryptedFrame[frameInfo.Header];

        Span<byte> target = decrypted.GetSpan(encryptedData.Length);

        Debug.Assert(nonce.Length == SodiumInterop.AeadAes256GcmNonceLength);
        Debug.Assert(this.key.Length == SodiumInterop.AeadAes256GcmKeyLength);

        int written = SodiumInterop.DecryptAeadAes256GCM(encryptedData, target, additionalData, this.key, nonce);
        decrypted.Advance(written);
    }

    /// <inheritdoc/>
    public void Encrypt(ReadOnlySpan<byte> frame, ArrayPoolBufferWriter<byte> encrypted)
    {
        const int rtpHeaderSize = 12;

        uint nonceInt = Interlocked.Increment(ref this.nonce);

        Span<byte> nonce = stackalloc byte[SodiumInterop.AeadAes256GcmNonceLength];
        BinaryPrimitives.WriteUInt32BigEndian(nonce, nonceInt);

        // write the unencrypted header
        encrypted.Write(frame[..rtpHeaderSize]);

        ReadOnlySpan<byte> header = frame[..rtpHeaderSize];
        ReadOnlySpan<byte> unencrypted = frame[rtpHeaderSize..];
        Span<byte> target = encrypted.GetSpan(unencrypted.Length + SodiumInterop.AeadAes256GcmAdditionalBytes);

        Debug.Assert(this.key.Length == SodiumInterop.AeadAes256GcmKeyLength);

        int written = SodiumInterop.EncryptAeadAes256Gcm(unencrypted, target, header, this.key, nonce);
        encrypted.Advance(written);

        BinaryPrimitives.WriteUInt32BigEndian(encrypted.GetSpan(4), nonceInt);
        encrypted.Advance(4);
    }
}
