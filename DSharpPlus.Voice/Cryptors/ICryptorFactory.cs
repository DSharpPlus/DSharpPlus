using System.Collections.Generic;

namespace DSharpPlus.Voice.Cryptors;

/// <summary>
/// Provides a method to get a cryptor for a given transport connection.
/// </summary>
public interface ICryptorFactory
{
    /// <summary>
    /// Selects the preferred cryptor for this connection and returns a ready-to-use instance.
    /// </summary>
    /// <param name="acceptedEncryptionModes">The encryption modes supported by the voice gateway node.</param>
    /// <param name="key">The key for the cryptor, valid for the lifetime of the transport connection.</param>
    public ICryptor CreateCryptor(IEnumerable<string> acceptedEncryptionModes, byte[] key);

    /// <summary>
    /// Gets the list of encryption modes supported by DSharpPlus.Voice.
    /// </summary>
    public IReadOnlyList<string> SupportedEncryptionModes { get; }
}
