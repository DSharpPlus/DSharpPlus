using System;
using System.Buffers;
using System.Buffers.Binary;
using System.Diagnostics;
using System.Threading;

using CommunityToolkit.HighPerformance.Buffers;

using DSharpPlus.Voice.Interop.Sodium;
using DSharpPlus.Voice.Protocol.Rtp;

namespace DSharpPlus.Voice.Cryptors;

/// <summary>
/// The built-in cryptor for AEAD XChaCha20-Poly1305.
/// </summary>
public sealed class AeadXChaCha20Poly1305Cryptor : ICryptor
{
    private readonly byte[] key;
    private uint nonce = 0;

    /// <summary>
    /// Creates a new AEAD XChaCha20-Poly1305 cryptor with the specified key.
    /// </summary>
    public AeadXChaCha20Poly1305Cryptor(byte[] key)
        => this.key = key;

    /// <inheritdoc/>
    public string EncryptionMode => "aead_xchacha20_poly1305_rtpsize";

    /// <inheritdoc/>
    public void Decrypt(ReadOnlySpan<byte> encryptedFrame, ArrayPoolBufferWriter<byte> decrypted, out int extensionHeaderLength)
    {
        RtpFrameInfo frameInfo = FrameParser.ParseRtpsizeSuffixedNonce(encryptedFrame, 4);
        extensionHeaderLength = frameInfo.ExtensionHeaderLength;

        Span<byte> nonce = stackalloc byte[SodiumInterop.AeadXChaCha20Poly1305NonceLength];
        encryptedFrame[frameInfo.Nonce].CopyTo(nonce);

        ReadOnlySpan<byte> encryptedData = encryptedFrame[frameInfo.VoiceData];
        ReadOnlySpan<byte> additionalData = encryptedFrame[frameInfo.Header];

        Span<byte> target = decrypted.GetSpan(encryptedData.Length);

        Debug.Assert(this.key.Length == SodiumInterop.AeadXChaCha20Poly1305KeyLength);

        int written = SodiumInterop.DecryptAeadXChaCha20Poly1305(encryptedData, target, additionalData, this.key, nonce);
        decrypted.Advance(written);
    }

    /// <inheritdoc/>
    public void Encrypt(ReadOnlySpan<byte> frame, ArrayPoolBufferWriter<byte> encrypted)
    {
        const int rtpHeaderSize = 12;

        uint nonceInt = Interlocked.Increment(ref this.nonce);

        Span<byte> nonce = stackalloc byte[SodiumInterop.AeadXChaCha20Poly1305NonceLength];
        BinaryPrimitives.WriteUInt32BigEndian(nonce, nonceInt);

        // write the unencrypted header
        encrypted.Write(frame[..rtpHeaderSize]);

        ReadOnlySpan<byte> header = frame[..rtpHeaderSize];
        ReadOnlySpan<byte> unencrypted = frame[rtpHeaderSize..];
        Span<byte> target = encrypted.GetSpan(unencrypted.Length + SodiumInterop.AeadXChaCha20Poly1305AdditionalBytes);

        Debug.Assert(this.key.Length == SodiumInterop.AeadXChaCha20Poly1305KeyLength);

        int written = SodiumInterop.EncryptAeadXChaCha20Poly1305(unencrypted, target, header, this.key, nonce);
        encrypted.Advance(written);

        BinaryPrimitives.WriteUInt32BigEndian(encrypted.GetSpan(4), nonceInt);
        encrypted.Advance(4);
    }
}
