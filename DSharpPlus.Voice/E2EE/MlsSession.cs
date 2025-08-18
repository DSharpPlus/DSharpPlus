using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading;

using CommunityToolkit.HighPerformance.Buffers;

using DSharpPlus.Voice.Interop.Koana;

namespace DSharpPlus.Voice.E2EE;

/// <inheritdoc/>
public sealed class MlsSession : IE2EESession
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

    /// <inheritdoc/>
    public void ReinitializeE2EESession(ushort protocolVersion)
    {
        lock (this.mlsLock)
        {
            this.koana.ReinitializeSession(protocolVersion);
            this.protocolVersion = protocolVersion;
        }
    }

    /// <inheritdoc/>
    public void SetExternalSender(byte[] payload)
    {
        lock (this.mlsLock)
        {
            this.koana.SetExternalSender(payload);
        }
    }

    /// <inheritdoc/>
    public byte[] ProcessProposals(byte[] payload)
    {
        List<ulong> roster = this.koana.GetUserList();

        lock (this.mlsLock)
        {
            Span<ulong> rosterSpan = CollectionsMarshal.AsSpan(roster);
            return this.koana.ProcessProposals(payload, rosterSpan);
        }
    }

    /// <inheritdoc/>
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

    /// <inheritdoc/>
    public void ProcessWelcome(byte[] payload)
    {
        List<ulong> roster = this.koana.GetUserList();

        lock (this.mlsLock)
        {
            Span<ulong> rosterSpan = CollectionsMarshal.AsSpan(roster);
            this.koana.ProcessWelcome(payload, rosterSpan);
        }
    }

    /// <inheritdoc/>
    public void WriteKeyPackage(ArrayPoolBufferWriter<byte> writer)
        => this.koana.GetMarshalledKeyPackage(writer);

    /// <inheritdoc/>
    public int DecryptFrame(ulong userId, ReadOnlySpan<byte> encryptedFrame, Span<byte> decryptedFrame)
    {
        lock (this.mlsLock)
        {
            return this.koana.DecryptFrame(userId, encryptedFrame, decryptedFrame);
        }
    }

    /// <inheritdoc/>
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
