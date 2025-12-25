using System;
using System.Collections.Generic;
using System.Linq;

namespace DSharpPlus.Voice.Cryptors;

/// <summary>
/// The default cryptor factory for DSharpPlus.
/// </summary>
public sealed class DefaultCryptorFactory : ICryptorFactory
{
    /// <inheritdoc/>
    public IReadOnlyList<string> SupportedEncryptionModes => ["aead_aes256_gcm_rtpsize", "aead_xchacha20_poly1305_rtpsize"];

    /// <inheritdoc/>
    public ICryptor CreateCryptor(IEnumerable<string> acceptedEncryptionModes, byte[] key)
    {
        if (acceptedEncryptionModes.Contains("aead_aes256_gcm_rtpsize"))
        {
            return new AeadAes256GcmCryptor(key);
        }

        if (acceptedEncryptionModes.Contains("aead_xchacha20_poly1305_rtpsize"))
        {
            return new AeadXChaCha20Poly1305Cryptor(key);
        }

        throw new InvalidOperationException($"Cannot create a cryptor for encryption modes [{string.Join(", ", acceptedEncryptionModes)}]");
    }
}
