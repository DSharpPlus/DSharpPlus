using System;

using CommunityToolkit.HighPerformance.Buffers;

namespace DSharpPlus.Voice.E2EE;

/// <summary>
/// Provides the E2EE implementation for DSharpPlus.Voice.
/// </summary>
public interface IE2EESession : IDisposable
{
    /// <summary>
    /// Decrypts the provided frame.
    /// </summary>
    /// <param name="userId">The snowflake identifier of the user who sent this payload.</param>
    /// <param name="encryptedFrame">The E2EE-encrypted frame data.</param>
    /// <param name="decryptedFrame">A buffer for the decrypted frame data, equal in length to the encrypted data.</param>
    /// <returns>The amount of bytes written to <paramref name="decryptedFrame"/>.</returns>
    public int DecryptFrame(ulong userId, ReadOnlySpan<byte> encryptedFrame, Span<byte> decryptedFrame);

    /// <summary>
    /// Encrypts the provided frame.
    /// </summary>
    /// <param name="unencryptedFrame">The unencrypted frame data.</param>
    /// <param name="encryptedFrame">A buffer for the E2EE-encrypted frame data.</param>
    /// <returns>The amount of bytes written to <paramref name="encryptedFrame"/>.</returns>
    public int EncryptFrame(ReadOnlySpan<byte> unencryptedFrame, Span<byte> encryptedFrame);

    /// <summary>
    /// Processes an otherwise unspecified commit.
    /// </summary>
    public void ProcessCommit(byte[] payload);

    /// <summary>
    /// Processes proposals and retuns a message with the E2EE client's response in turn.
    /// </summary>
    public byte[] ProcessProposals(byte[] payload);

    /// <summary>
    /// Welcomes a new user to the E2EE group.
    /// </summary>
    public void ProcessWelcome(byte[] payload);

    /// <summary>
    /// Reinitializes the E2EE session with a different DAVE protocol version.
    /// </summary>
    public void ReinitializeE2EESession(ushort protocolVersion);

    /// <summary>
    /// Sets the voice gateway as an external sender capable of adding members to the E2EE group.
    /// </summary>
    public void SetExternalSender(byte[] payload);

    /// <summary>
    /// Writes the client's key package to the writer.
    /// </summary>
    public void WriteKeyPackage(ArrayPoolBufferWriter<byte> writer);
}
