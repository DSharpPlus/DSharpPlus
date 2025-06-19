using System;
using System.Diagnostics;

namespace DSharpPlus.Voice.Interop.Sodium;

#pragma warning disable IDE0046

/// <summary>
/// The interop wrapper for sodium.
/// </summary>
internal partial class SodiumInterop
{
    public const int AeadAes256GcmAdditionalBytes = 16;
    public const int AeadAes256GcmKeyLength = 32;
    public const int AeadAes256GcmNonceLength = 12;

    public const int AeadXChaCha20Poly1305AdditionalBytes = 16;
    public const int AeadXChaCha20Poly1305KeyLength = 32;
    public const int AeadXChaCha20Poly1305NonceLength = 24;

    public static void Initialize()
        => sodium_init();

    public static bool IsAeadAes256GcmAvailable()
        => crypto_aead_aes256gcm_is_available() == 1;

    /// <summary>
    /// Encrypts the given frame with AEAD AES-256 GCM using the provided key and nonce. 
    /// </summary>
    public static unsafe int EncryptAeadAes256Gcm(ReadOnlySpan<byte> source, Span<byte> target, ReadOnlySpan<byte> additionalData, ReadOnlySpan<byte> key, ReadOnlySpan<byte> nonce)
    {
        ulong written;
        Debug.Assert(target.Length >= source.Length + AeadAes256GcmAdditionalBytes);

        fixed (byte* pSource = source)
        fixed (byte* pTarget = target)
        fixed (byte* pAdditional = additionalData)
        fixed (byte* pKey = key)
        fixed (byte* pNonce = nonce)
        {
            int result = crypto_aead_aes256gcm_encrypt(pTarget, &written, pSource, (ulong)source.Length, pAdditional, (ulong)additionalData.Length, null, pNonce, pKey);
            Debug.Assert(result == 0);
        }

        return (int)written;
    }

    /// <summary>
    /// Decrypts the given frame with AEAD AES-256 GCM using the provided key and nonce.
    /// </summary>
    public static unsafe int DecryptAeadAes256GCM(ReadOnlySpan<byte> source, Span<byte> target, ReadOnlySpan<byte> additionalData, ReadOnlySpan<byte> key, ReadOnlySpan<byte> nonce)
    {
        ulong written;
        Debug.Assert(target.Length >= source.Length - AeadAes256GcmAdditionalBytes);

        fixed (byte* pSource = source)
        fixed (byte* pTarget = target)
        fixed (byte* pAdditional = additionalData)
        fixed (byte* pKey = key)
        fixed (byte* pNonce = nonce)
        {
            int result = crypto_aead_aes256gcm_decrypt(pTarget, &written, null, pSource, (ulong)source.Length, pAdditional, (ulong)additionalData.Length, pNonce, pKey);
            Debug.Assert(result == 0);
        }

        return (int)written;
    }

    /// <summary>
    /// Encrypts the given frame with AEAD XChaCha20-Poly1305 using the provided key and nonce.
    /// </summary>
    public static unsafe int EncryptAeadXChaCha20Poly1305(ReadOnlySpan<byte> source, Span<byte> target, ReadOnlySpan<byte> additionalData, ReadOnlySpan<byte> key, ReadOnlySpan<byte> nonce)
    {
        ulong written;
        Debug.Assert(target.Length >= source.Length + AeadXChaCha20Poly1305AdditionalBytes);

        fixed (byte* pSource = source)
        fixed (byte* pTarget = target)
        fixed (byte* pAdditional = additionalData)
        fixed (byte* pKey = key)
        fixed (byte* pNonce = nonce)
        {
            int result = crypto_aead_xchacha20poly1305_ietf_encrypt(pTarget, &written, pSource, (ulong)source.Length, pAdditional, (ulong)additionalData.Length, null, pNonce, pKey);
            Debug.Assert(result == 0);
        }

        return (int)written;
    }

    /// <summary>
    /// Decrypts the given frame with AEAD XChaCha20-Poly1305 using the provided key and nonce.
    /// </summary>
    public static unsafe int DecryptAeadXChaCha20Poly1305(ReadOnlySpan<byte> source, Span<byte> target, ReadOnlySpan<byte> additionalData, ReadOnlySpan<byte> key, ReadOnlySpan<byte> nonce)
    {
        ulong written;
        Debug.Assert(target.Length >= source.Length - AeadXChaCha20Poly1305AdditionalBytes);

        fixed (byte* pSource = source)
        fixed (byte* pTarget = target)
        fixed (byte* pAdditional = additionalData)
        fixed (byte* pKey = key)
        fixed (byte* pNonce = nonce)
        {
            int result = crypto_aead_xchacha20poly1305_ietf_decrypt(pTarget, &written, null, pSource, (ulong)source.Length, pAdditional, (ulong)additionalData.Length, pNonce, pKey);
            Debug.Assert(result == 0);
        }

        return (int)written;
    }
}
