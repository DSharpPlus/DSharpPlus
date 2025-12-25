using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Security.Cryptography;

using CommunityToolkit.HighPerformance;
using CommunityToolkit.HighPerformance.Buffers;

using Microsoft.Extensions.Logging;

namespace DSharpPlus.Voice.Interop.Koana;

/// <summary>
/// Represents the interop boundary between C# and the native library used for MLS.
/// </summary>
internal unsafe partial struct KoanaInterop : IDisposable
{
    private static ILogger<KoanaContext>? logger;
    private static bool logNativeDebuggingMessages;

    private readonly KoanaContext* context;
    private readonly ulong channelId;
    private readonly ulong userId;

    [UnmanagedCallersOnly]
    public static void HandleNativeMlsError(byte* pSite, byte* pReason)
    {
        string site = new((sbyte*)pSite);
        string reason = new((sbyte*)pReason);

        logger?.LogError("A MLS error occurred in {site}: {reason}", site, reason);
    }

    [UnmanagedCallersOnly]
    public static void HandleKoanaLog(KoanaLogLevel logLevel, byte* pMessage)
    {
        string message = new((sbyte*)pMessage);
        LogLevel level = (LogLevel)(int)logLevel;

        if (logNativeDebuggingMessages)
        {
            logger?.Log(level, "{message}", message);
        }
    }

    /// <summary>
    /// Sets the logger used to bubble up any native errors and whether to also log native debugging messages.
    /// </summary>
    public static void SetLogger(ILogger<KoanaContext> logger, bool logNativeMessages)
    {
        KoanaInterop.logger = logger;
        KoanaInterop.logNativeDebuggingMessages = logNativeMessages;
    }

    /// <summary>
    /// Initializes the native library state for use.
    /// </summary>
    public static void Initialize()
        => koana_set_mls_error_callback((delegate* unmanaged<byte*, byte*, void>)&HandleNativeMlsError);

    /// <summary>
    /// Creates a new session with Koana.
    /// </summary>
    /// <param name="protocolVersion">The protocol version to use, currently always 1 (if we create a session at all).</param>
    /// <param name="channelId">The snowflake identifier of the voice channel.</param>
    /// <param name="userId">The snowflake identifier of the bot user.</param>
    public KoanaInterop(ushort protocolVersion, ulong channelId, ulong userId)
    {
        KoanaContext* context = koana_create_context((delegate* unmanaged<KoanaLogLevel, byte*, void>)&HandleKoanaLog);
        koana_reinit_context(context, protocolVersion, channelId, userId);

        this.context = context;
        this.channelId = channelId;
        this.userId = userId;
    }

    /// <summary>
    /// Reinitializes the current session.
    /// </summary>
    /// <param name="protocolVersion">The protocol version to use, currently always 1.</param>
    public readonly void ReinitializeSession(ushort protocolVersion)
    {
        koana_reset_context(this.context);
        koana_reinit_context(this.context, protocolVersion, this.channelId, this.userId);
    }

    /// <summary>
    /// Sets the voice gateway as the external sender.
    /// </summary>
    /// <param name="data">The external sender package received.</param>
    public readonly void SetExternalSender(ReadOnlySpan<byte> data)
    {
        fixed (byte* pData = data)
        {
            koana_set_external_sender(this.context, pData, data.Length);
        }
    }

    /// <summary>
    /// Processes a batch of proposals and returns either an empty array or a response to the voice gateway.
    /// </summary>
    /// <param name="proposals">The batch of proposals received.</param>
    /// <param name="userIds">The user IDs known to be part of the MLS group.</param>
    public readonly byte[] ProcessProposals(ReadOnlySpan<byte> proposals, ReadOnlySpan<ulong> userIds)
    {
        NativeVector* response;

        fixed (byte* pProposals = proposals)
        fixed (ulong* pUserIds = userIds)
        {
            response = koana_process_proposals(this.context, pProposals, proposals.Length, pUserIds, userIds.Length);
        }

        if (response == null)
        {
            return [];
        }

        if (response->error == KoanaError.OutOfMemory)
        {
            ThrowHelper.ThrowNativeOutOfMemory();
        }

        byte[] buffer = new byte[response->length];
        Span<byte> nativeMemory = new(response->data, response->length);

        nativeMemory.CopyTo(buffer);

        koana_destroy_vector(response);
        return buffer;
    }

    /// <summary>
    /// Processes a commit and returns a code indicating whether the session needs to be reset.
    /// </summary>
    public readonly KoanaError ProcessCommit(ReadOnlySpan<byte> commit)
    {
        fixed (byte* pCommit = commit)
        {
            return koana_process_commit(this.context, pCommit, commit.Length);
        }
    }

    /// <summary>
    /// Processes a welcome message.
    /// </summary>
    public readonly void ProcessWelcome(ReadOnlySpan<byte> welcome, ReadOnlySpan<ulong> userIds)
    {
        fixed (byte* pWelcome = welcome)
        fixed (ulong* pUserIds = userIds)
        {
            koana_process_welcome(this.context, pWelcome, welcome.Length, pUserIds, userIds.Length);
        }
    }

    /// <summary>
    /// Gets the marshalled key package for the current session and writes it to the provided writer.
    /// </summary>
    public readonly void GetMarshalledKeyPackage(ArrayPoolBufferWriter<byte> writer)
    {
        NativeVector* vector = koana_get_marshalled_key_package(this.context);

        Span<byte> package = new(vector->data, vector->length);
        writer.Write<byte>(package);

        koana_destroy_vector(vector);
    }

    /// <summary>
    /// Gets the roster of users currently present in the MLS group.
    /// </summary>
    public readonly Dictionary<ulong, byte[]> GetUserRoster()
    {
        NativeRoster* nativeRoster = koana_get_cached_roster(this.context);

        if (nativeRoster == null)
        {
            return [];
        }

        if (nativeRoster->error == KoanaError.OutOfMemory)
        {
            ThrowHelper.ThrowNativeOutOfMemory();
        }

        if (nativeRoster->error == KoanaError.LengthMismatch)
        {
            ThrowHelper.ThrowNativeInvalidData();
        }

        int count = nativeRoster->length;

        Dictionary<ulong, byte[]> roster = new(count);

        for (int i = 0; i < count; i++)
        {
            ulong key = nativeRoster->keys[i];

            byte[] buffer = new byte[nativeRoster->valueLengths[i]];
            Span<byte> nativeMemory = new(nativeRoster->values[i], nativeRoster->valueLengths[i]);

            nativeMemory.CopyTo(buffer);

            roster.Add(key, buffer);
        }

        koana_destroy_roster(nativeRoster);

        return roster;
    }

    /// <summary>
    /// Gets the list of user IDs currently part of the MLS group.
    /// </summary>
    public readonly List<ulong> GetUserList()
    {
        NativeRoster* nativeRoster = koana_get_cached_roster(this.context);

        if (nativeRoster == null)
        {
            return [];
        }

        if (nativeRoster->error == KoanaError.OutOfMemory)
        {
            ThrowHelper.ThrowNativeOutOfMemory();
        }

        if (nativeRoster->error == KoanaError.LengthMismatch)
        {
            ThrowHelper.ThrowNativeInvalidData();
        }

        List<ulong> list = new(nativeRoster->length);
        Span<ulong> backing = CollectionsMarshal.AsSpan(list);

        Span<ulong> native = new(nativeRoster->keys, nativeRoster->length);
        native.CopyTo(backing);

        return list;
    }

    /// <summary>
    /// Decrypts an audio frame.
    /// </summary>
    /// <param name="userId">The snowflake identifier of the user who sent the audio frame.</param>
    /// <param name="encryptedFrame">The encrypted frame data.</param>
    /// <param name="decryptedFrame">A buffer for the decrypted frame data, equal in length to the encrypted frame.</param>
    /// <returns>The amount of data that was decrypted.</returns>
    public readonly int DecryptFrame(ulong userId, ReadOnlySpan<byte> encryptedFrame, Span<byte> decryptedFrame)
    {
        Debug.Assert(encryptedFrame.Length <= decryptedFrame.Length);

        fixed (byte* pEncrypted = encryptedFrame)
        fixed (byte* pDecrypted = decryptedFrame)
        {
            int decrypted = koana_decrypt_frame(this.context, userId, pEncrypted, encryptedFrame.Length, pDecrypted, decryptedFrame.Length);

            Debug.Assert(decrypted == encryptedFrame.Length);
            return decrypted;
        }
    }

    /// <summary>
    /// Encrypts an audio frame.
    /// </summary>
    /// <param name="unencryptedFrame">The unencrypted frame data.</param>
    /// <param name="encryptedFrame">A buffer for the encrypted frame data, equal in length to the unencrypted form.</param>
    /// <param name="ssrc">The sender's SSRC for this frame.</param>
    /// <returns>The amount of data that was encrypted.</returns>
    public readonly int EncryptFrame(ReadOnlySpan<byte> unencryptedFrame, Span<byte> encryptedFrame, uint ssrc)
    {
        Debug.Assert(unencryptedFrame.Length <= encryptedFrame.Length);
        int size;

        fixed (byte* pUnencrypted = unencryptedFrame)
        fixed (byte* pEncrypted = encryptedFrame)
        {
            KoanaError error = koana_encrypt_frame(this.context, ssrc, pUnencrypted, unencryptedFrame.Length, pEncrypted, encryptedFrame.Length, &size);

            if (error == KoanaError.EncryptionFailure)
            {
                ThrowHelper.ThrowEncryptionFailure();
            }
        }

        Debug.Assert(size == unencryptedFrame.Length);
        return size;
    }

    public readonly ushort GetProtocolVersion()
        => koana_get_current_protocol_version(this.context);

    public readonly void Dispose()
        => koana_destroy_context(this.context);
}

file class ThrowHelper
{
    [DebuggerHidden]
    [StackTraceHidden]
    public static void ThrowNativeOutOfMemory()
        => throw new OutOfMemoryException("Failed to allocate enough memory for an operation in native MLS code.");

    [DebuggerHidden]
    [StackTraceHidden]
    public static void ThrowNativeInvalidData()
        => throw new InvalidDataException("Encountered invalid data in native MLS code.");

    [DebuggerHidden]
    [StackTraceHidden]
    public static void ThrowEncryptionFailure()
        => throw new CryptographicException("Failed to encrypt a frame in native MLS code.");
}
