
using System;
using System.Threading;

using CommunityToolkit.HighPerformance.Buffers;

using DSharpPlus.Voice.Interop.Koana;

namespace DSharpPlus.Voice.E2EE;

/// <summary>
/// Provides the E2EE implementation for DSharpPlus.Voice, based on DAVE 1.1.4.
/// </summary>
public sealed class MlsSession : IDisposable
{
    private readonly KoanaInterop koana;
    private readonly Lock mlsLock;

    private readonly uint ssrc;
    private ushort protocolVersion;

    /// <summary>
    /// Creates a new MLS session.
    /// </summary>
    /// <param name="protocolVersion">The DAVE protocol version used by this connection, currently always 1.</param>
    /// <param name="channelId">The snowflake identifier of the voice channel.</param>
    /// <param name="userId">The snowflake identifier of the bot user.</param>
    /// <param name="ssrc">The SSRC of the bot user.</param>
    public MlsSession(ushort protocolVersion, ulong channelId, ulong userId, uint ssrc)
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
    public void SetExternalSender(byte[] payload)
    {
        lock (this.mlsLock)
        {
            this.koana.SetExternalSender(payload);
        }
    }

    /// <summary>
    /// Processes proposals and retuns a message with the E2EE client's response in turn.
    /// </summary>
    public byte[] ProcessProposals(byte[] payload, ulong[] roster)
    {
        lock (this.mlsLock)
        {
            return this.koana.ProcessProposals(payload, roster);
        }
    }

    /// <summary>
    /// Processes an otherwise unspecified commit.
    /// </summary>
    public void ProcessCommit(byte[] payload)
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
    public void ProcessWelcome(byte[] payload, ulong[] roster)
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
    public int EncryptFrame(ReadOnlySpan<byte> unencryptedFrame, Span<byte> encryptedFrame)
    {
        lock (this.mlsLock)
        {
            return this.koana.EncryptFrame(unencryptedFrame, encryptedFrame, this.ssrc);
        }
    }

    /// <inheritdoc/>
    public void Dispose()
        => this.koana.Dispose();
}
