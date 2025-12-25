using System;

using CommunityToolkit.HighPerformance.Buffers;

namespace DSharpPlus.Voice.Cryptors;

/// <summary>
/// Represents a mechanism for encrypting and decrypting audio frames.
/// </summary>
public interface ICryptor
{
    /// <summary>
    /// Encrypts the given frame into the buffer writer.
    /// </summary>
    public void Encrypt(ReadOnlySpan<byte> frame, ArrayPoolBufferWriter<byte> encrypted);

    /// <summary>
    /// Decrypts the given frame into the buffer writer.
    /// </summary>
    public void Decrypt(ReadOnlySpan<byte> encryptedFrame, ArrayPoolBufferWriter<byte> decrypted, out int extensionHeaderLength);

    /// <summary>
    /// Gets the encryption mode supported by this cryptor.
    /// </summary>
    public string EncryptionMode { get; }
}
