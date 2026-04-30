using System.Collections.Generic;

namespace DSharpPlus.Voice.Cryptors;

/// <summary>
/// Provides a method to get a cryptor for a given transport connection.
/// </summary>
public interface ICryptorFactory
{
    /// <summary>
    /// Creates a cryptor for this connection.
    /// </summary>
    /// <param name="selectedEncryptionMode">The encryption modes selected for this connection.</param>
    /// <param name="key">The key for the cryptor, valid for the lifetime of the transport connection.</param>
    public ICryptor CreateCryptor(string selectedEncryptionMode, byte[] key);

    /// <summary>
    /// Selects the preferred encryption mode based on what the voice server supports.
    /// </summary>
    public string SelectPreferredEncryptionMode(params IEnumerable<string> supportedEncryptionModes);

    /// <summary>
    /// Gets the list of encryption modes supported by DSharpPlus.Voice.
    /// </summary>
    public IReadOnlyList<string> SupportedEncryptionModes { get; }
}
