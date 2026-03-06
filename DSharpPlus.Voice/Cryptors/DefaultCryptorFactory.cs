using System;
using System.Collections.Generic;
using System.Linq;

using DSharpPlus.Voice.Interop.Sodium;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace DSharpPlus.Voice.Cryptors;

/// <summary>
/// The default cryptor factory for DSharpPlus.
/// </summary>
public sealed class DefaultCryptorFactory : ICryptorFactory
{
    private readonly bool enableAeadAes256Gcm;

    public DefaultCryptorFactory(IOptions<VoiceOptions> options, ILogger<ICryptorFactory> logger)
    {
        this.enableAeadAes256Gcm = SodiumInterop.IsAeadAes256GcmAvailable() && options.Value.EnableAeadAes256GcmEncryption;
        logger.LogInformation("AEAD AES-256 GCM encryption support {status}.", this.enableAeadAes256Gcm ? "enabled" : "disabled");
    }

    /// <inheritdoc/>
    public IReadOnlyList<string> SupportedEncryptionModes => ["aead_aes256_gcm_rtpsize", "aead_xchacha20_poly1305_rtpsize"];

    public string SelectPreferredEncryptionMode(params IEnumerable<string> supportedEncryptionModes)
    {
        if (supportedEncryptionModes.Contains("aead_aes256_gcm_rtpsize") && this.enableAeadAes256Gcm)
        {
            return "aead_aes256_gcm_rtpsize";
        }

        if (supportedEncryptionModes.Contains("aead_xchacha20_poly1305_rtpsize"))
        {
            return "aead_xchacha20_poly1305_rtpsize";
        }

        throw new InvalidOperationException($"None of the encryption modes [{string.Join(", ", supportedEncryptionModes)}] are supported.");
    }

    /// <inheritdoc/>
    public ICryptor CreateCryptor(string selectedEncryptionMode, byte[] key)
    {
        return selectedEncryptionMode switch
        {
            "aead_aes256_gcm_rtpsize" when this.enableAeadAes256Gcm => new AeadAes256GcmCryptor(key),
            "aead_xchacha20_poly1305_rtpsize" => new AeadXChaCha20Poly1305Cryptor(key),
            _ => throw new InvalidOperationException($"Unsupported encyrption mode {selectedEncryptionMode}.")
        };
    }
}
