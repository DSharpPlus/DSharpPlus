using System.Runtime.InteropServices;

namespace DSharpPlus.Voice.Interop.Sodium;

internal unsafe partial class SodiumInterop
{
    // sodium_init checks hardware support for AEAD AES256-GCM, the preferred transport encryption mode

    /// <summary>
    /// <c>void sodium_init(void);</c>
    /// </summary>
    [LibraryImport("libsodium")]
    private static partial void sodium_init();

    // AEAD AES256-GCM imports
    // Discord prefers we use this functionality whenever possible, so we do an availability check on startup. if it is available, we try to use this.

    /// <summary>
    /// <c>int crypto_aead_aes256gcm_is_available(void);</c>
    /// </summary>
    [LibraryImport("libsodium")]
    private static partial int crypto_aead_aes256gcm_is_available();

    /// <summary>
    /// <c>size_t crypto_aead_aes256gcm_keybytes(void);</c>
    /// </summary>
    [LibraryImport("libsodium")]
    private static partial nuint crypto_aead_aes256gcm_keybytes();

    /// <summary>
    /// <c>size_t crypto_aead_aes256gcm_npubbytes(void);</c>
    /// </summary>
    [LibraryImport("libsodium")]
    private static partial nuint crypto_aead_aes256gcm_npubbytes();

    /// <summary>
    /// <c>size_t crypto_aead_aes256gcm_abytes(void);</c>
    /// </summary>
    [LibraryImport("libsodium")]
    private static partial nuint crypto_aead_aes256gcm_abytes();

    /// <summary>
    /// <code><![CDATA[int crypto_aead_aes256gcm_encrypt(
    ///     unsigned char *c, unsigned long long *clen_p,
    ///     const unsigned char *m, unsigned long long mlen,
    ///     const unsigned char *ad, unsigned long long adlen,
    ///     const unsigned char *nsec, const unsigned char *npub,
    ///     const unsigned char *k)]]></code>
    /// </summary>
    [LibraryImport("libsodium")]
    private static partial int crypto_aead_aes256gcm_encrypt
    (
        byte* encrypted,
        ulong* encryptedLength,
        byte* message,
        ulong messageLength,
        byte* additionalData,
        ulong additionalLength,
        // unused, should be null
        byte* nonceSecret,
        byte* noncePublic,
        byte* key
    );

    /// <summary>
    /// <code><![CDATA[int crypto_aead_aes256gcm_decrypt(
    ///     unsigned char *m, unsigned long long *mlen_p,
    ///     unsigned char *nsec,
    ///     const unsigned char *c, unsigned long long clen,
    ///     const unsigned char *ad, unsigned long long adlen,
    ///     const unsigned char *npub,
    ///     const unsigned char *k)]]></code>
    /// </summary>
    [LibraryImport("libsodium")]
    private static partial int crypto_aead_aes256gcm_decrypt
    (
        byte* message,
        ulong* messageLength,
        // unused, should be null
        byte* nonceSecret,
        byte* encrypted,
        ulong encryptedLength,
        byte* additionalData,
        ulong additionalLength,
        byte* noncePublic,
        byte* key
    );

    // AEAD XChaCha20-Poly1305 imports
    // this serves as the fallback if AEAD AES256-GCM is not available, and thereby as the de-facto baseline.

    /// <summary>
    /// <c>size_t crypto_aead_xchacha20poly1305_ietf_keybytes(void);</c>
    /// </summary>
    [LibraryImport("libsodium")]
    private static partial nuint crypto_aead_xchacha20poly1305_ietf_keybytes();

    /// <summary>
    /// <c>size_t crypto_aead_xchacha20poly1305_ietf_npubbytes(void);</c>
    /// </summary>
    [LibraryImport("libsodium")]
    private static partial nuint crypto_aead_xchacha20poly1305_ietf_npubbytes();

    /// <summary>
    /// <c>size_t crypto_aead_xchacha20poly1305_ietf_abytes(void);</c>
    /// </summary>
    [LibraryImport("libsodium")]
    private static partial nuint crypto_aead_xchacha20poly1305_ietf_abytes();

    /// <summary>
    /// <code><![CDATA[int crypto_aead_xchacha20poly1305_ietf_encrypt(
    ///     unsigned char *c, unsigned long long *clen_p,
    ///     const unsigned char *m, unsigned long long mlen,
    ///     const unsigned char *ad, unsigned long long adlen,
    ///     const unsigned char *nsec, const unsigned char *npub,
    ///     const unsigned char *k)]]></code>
    /// </summary>
    [LibraryImport("libsodium")]
    private static partial int crypto_aead_xchacha20poly1305_ietf_encrypt
    (
        byte* encrypted,
        ulong* encryptedLength,
        byte* message,
        ulong messageLength,
        byte* additionalData,
        ulong additionalLength,
        // unused, should be null
        byte* nonceSecret,
        byte* noncePublic,
        byte* key
    );

    /// <summary>
    /// <code><![CDATA[int crypto_aead_xchacha20poly1305_ietf_decrypt(
    ///     unsigned char *m, unsigned long long *mlen_p,
    ///     unsigned char *nsec,
    ///     const unsigned char *c, unsigned long long clen,
    ///     const unsigned char *ad, unsigned long long adlen,
    ///     const unsigned char *npub,
    ///     const unsigned char *k)]]></code>
    /// </summary>
    [LibraryImport("libsodium")]
    private static partial int crypto_aead_xchacha20poly1305_ietf_decrypt
    (
        byte* message,
        ulong* messageLength,
        // unused, should be null
        byte* nonceSecret,
        byte* encrypted,
        ulong encryptedLength,
        byte* additionalData,
        ulong additionalLength,
        byte* noncePublic,
        byte* key
    );
}
