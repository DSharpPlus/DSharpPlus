using System;
using System.Threading;

using CommunityToolkit.HighPerformance.Buffers;

using DSharpPlus.Voice.Interop.Koana;

namespace DSharpPlus.Voice.E2EE;

/// <summary>
/// Provides the E2EE implementation for DSharpPlus.Voice, based on DAVE 1.1.4.
/// </summary>
public sealed class MlsSession : IE2EESession, IDisposable
{
    private KoanaInterop koana;
    private Lock mlsLock;

    private uint ssrc;
    private ushort protocolVersion;

    /// <inheritdoc/>
    public void Initialize(ushort protocolVersion, ulong channelId, ulong userId, uint ssrc)
    {
        this.koana = new(protocolVersion, channelId, userId);
        this.mlsLock = new();

        this.protocolVersion = protocolVersion;
        this.ssrc = ssrc;
    }

    /// <summary>
    /// Reinitializes the E2EE session with a different DAVE protocol version.
    /// </summary>
    public void ReinitializeE2EESession(ushort protocolVersion)
    {
        lock (this.mlsLock)
        {
            this.koana.ReinitializeSession(protocolVersion);
            this.protocolVersion = protocolVersion;
        }
    }

    /// <summary>
    /// Sets the voice gateway as an external sender capable of adding members to the E2EE group.
    /// </summary>
    public void SetExternalSender(ReadOnlySpan<byte> payload)
    {
        lock (this.mlsLock)
        {
            this.koana.SetExternalSender(payload);
        }
    }

    /// <summary>
    /// Processes proposals and retuns a message with the E2EE client's response in turn.
    /// </summary>
    public byte[] ProcessProposals(ReadOnlySpan<byte> payload, ReadOnlySpan<ulong> roster)
    {
        lock (this.mlsLock)
        {
            return this.koana.ProcessProposals(payload, roster);
        }
    }

    /// <summary>
    /// Processes an otherwise unspecified commit.
    /// </summary>
    public void ProcessCommit(ReadOnlySpan<byte> payload)
    {
        lock (this.mlsLock)
        {
            if (this.koana.ProcessCommit(payload) == KoanaError.MlsCommitResetError)
            {
                this.koana.ReinitializeSession(this.protocolVersion);
            }
        }
    }

    /// <summary>
    /// Welcomes a new user to the E2EE group.
    /// </summary>
    public void ProcessWelcome(ReadOnlySpan<byte> payload, ReadOnlySpan<ulong> roster)
    {
        lock (this.mlsLock)
        {
            this.koana.ProcessWelcome(payload, roster);
        }
    }

    /// <summary>
    /// Writes the client's key package to the writer.
    /// </summary>
    public void WriteKeyPackage(ArrayPoolBufferWriter<byte> writer)
        => this.koana.GetMarshalledKeyPackage(writer);

    /// <summary>
    /// Decrypts the provided frame.
    /// </summary>
    /// <param name="userId">The snowflake identifier of the user who sent this payload.</param>
    /// <param name="encryptedFrame">The E2EE-encrypted frame data.</param>
    /// <param name="decryptedFrame">A buffer for the decrypted frame data, equal in length to the encrypted data.</param>
    /// <returns>The amount of bytes written to <paramref name="decryptedFrame"/>.</returns>
    public int DecryptFrame(ulong userId, ReadOnlySpan<byte> encryptedFrame, Span<byte> decryptedFrame)
    {
        lock (this.mlsLock)
        {
            return this.koana.DecryptFrame(userId, encryptedFrame, decryptedFrame);
        }
    }

    /// <summary>
    /// Encrypts the provided frame.
    /// </summary>
    /// <param name="unencryptedFrame">The unencrypted frame data.</param>
    /// <param name="encryptedFrame">A buffer for the E2EE-encrypted frame data.</param>
    /// <returns>The amount of bytes written to <paramref name="encryptedFrame"/>.</returns>
    public int EncryptFrame(ReadOnlySpan<byte> unencryptedFrame, ArrayPoolBufferWriter<byte> encryptedFrame)
    {
        lock (this.mlsLock)
        {
            int maxEncryptedSize = this.koana.GetMaxEncryptedSize(unencryptedFrame.Length);
            int encrypted = this.koana.EncryptFrame(unencryptedFrame, encryptedFrame.GetSpan(maxEncryptedSize), this.ssrc);
            encryptedFrame.Advance(encrypted);
            return encrypted;
        }
    }

    /// <inheritdoc />
    public int GetMaxEncryptedLength(int unencryptedLength)
        => this.koana.GetMaxEncryptedSize(unencryptedLength);

    /// <inheritdoc/>
    public void Dispose()
        => this.koana.Dispose();
}
