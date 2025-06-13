using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

namespace DSharpPlus.Voice.Interop.Aerith;

/// <summary>
/// Provides utilities to consume native methods from managed code.
/// </summary>
internal static unsafe partial class AerithInterop
{
    [UnmanagedCallersOnly]
    public static void HandleNativeMlsError(byte* site, byte* reason)
    {

    }

    /// <summary>
    /// Creates a new session object. The session ID here must match the session ID we later initialize with.
    /// </summary>
    public static AerithSession* CreateSession(string sessionId)
    {
        Span<byte> ascii = stackalloc byte[sessionId.Length];
        Encoding.ASCII.GetBytes(sessionId, ascii);

        fixed (byte* pSessionId = ascii)
        {
            return Bindings.AerithCreateSession(pSessionId, (nuint)ascii.Length, (delegate* unmanaged<byte*, byte*, void>)&HandleNativeMlsError);
        }
    }

    /// <summary>
    /// Initializes the previously created session. 
    /// </summary>
    public static void InitializeSession(AerithSession* session, string sessionId, ulong groupId, ulong currentUser)
    {
        Span<byte> sessionIdAscii = stackalloc byte[sessionId.Length];

        Encoding.ASCII.GetBytes(sessionId, sessionIdAscii);

        fixed (byte* pSessionId = sessionIdAscii)
        {
            AerithSignaturePrivateKey* key = Bindings.AerithGetSignaturePrivateKey(pSessionId, (nuint)sessionId.Length);
            Bindings.AerithInitSession(session, groupId, currentUser, key);
        }
    }

    /// <summary>
    /// Gets the authenticator key for the last epoch.
    /// </summary>
    public static byte[] GetLastEpochAuthenticator(AerithSession* session)
    {
        VectorWrapper* nativeVector = Bindings.AerithGetLastEpochAuthenticator(session);
        return UnwrapVector(nativeVector);
    }

    /// <summary>
    /// Sets the voice gateway as the external sender.
    /// </summary>
    public static void SetExternalSender(AerithSession* session, byte[] data)
    {
        fixed (byte* pData = data)
        {
            Bindings.AerithSetExternalSender(session, pData, (nuint)data.Length);
        }
    }

    /// <summary>
    /// Processes a batch of proposals and returns either an empty array or a response to the voice gateway.
    /// </summary>
    public static byte[] ProcessProposals(AerithSession* session, byte[] proposals, ReadOnlySpan<ulong> userIds)
    {
        fixed (byte* pProposals = proposals)
        fixed (ulong* pUserIds = userIds)
        {
            VectorWrapper* ack = Bindings.AerithProcessProposals(session, pProposals, (nuint)proposals.Length, pUserIds, userIds.Length);
            return UnwrapVector(ack);
        }
    }

    /// <summary>
    /// Processes a commit and returns a dictionary of added and removed users.
    /// </summary>
    public static (AerithCommitError, Dictionary<ulong, byte[]>) ProcessCommit(AerithSession* session, byte[] commit)
    {
        AerithCommitError error;
        RosterWrapper* nativeRoster;

        fixed (byte* pCommit = commit)
        {
            nativeRoster = Bindings.AerithProcessCommit(session, pCommit, (nuint)commit.Length, (int*)&error);
        }

        Dictionary<ulong, byte[]> changeRoster = error == AerithCommitError.Success ? UnwrapRoster(nativeRoster) : null!;
        return (error, changeRoster);
    }

    /// <summary>
    /// Processes a welcome message and returns a dictionary of added users.
    /// </summary>
    public static Dictionary<ulong, byte[]> ProcessWelcome(AerithSession* session, byte[] welcome, ReadOnlySpan<ulong> userIds)
    {
        fixed (byte* pWelcome = welcome)
        fixed (ulong* pUserIds = userIds)
        {
            RosterWrapper* nativeRoster = Bindings.AerithProcessWelcome(session, pWelcome, (nuint)welcome.Length, pUserIds, userIds.Length);
            return UnwrapRoster(nativeRoster);
        }
    }

    /// <summary>
    /// Gets the marshalled key package for the current session.
    /// </summary>
    public static byte[] GetMarshalledKeyPackage(AerithSession* session)
    {
        VectorWrapper* nativeKeyPackage = Bindings.AerithGetMarshalledKeyPackage(session);
        return UnwrapVector(nativeKeyPackage);
    }

    /// <summary>
    /// Resets the current session.
    /// </summary>
    public static void ResetSession(AerithSession* session) => Bindings.AerithResetSession(session);

    /// <summary>
    /// Destroys the current session at the end of its lifetime.
    /// </summary>
    public static void DestroySession(AerithSession* session) => Bindings.AerithDestroySession(session);

    /// <summary>
    /// Unwraps a <c>std::vector&lt;uint8_t&gt;</c> into a c# byte array.
    /// </summary>
    /// <param name="nativeVector"></param>
    /// <returns></returns>
    private static byte[] UnwrapVector(VectorWrapper* nativeVector)
    {
        if (nativeVector == null)
        {
            return [];
        }

        if (nativeVector->error == AerithWrapperError.OutOfMemory)
        {
            ThrowHelper.ThrowNativeOutOfMemory();
        }

        byte[] buffer = new byte[nativeVector->length];
        Span<byte> nativeMemory = new(nativeVector->data, nativeVector->length);

        nativeMemory.CopyTo(buffer);

        Bindings.AerithDestroyVectorWrapper(nativeVector);

        return buffer;
    }

    /// <summary>
    /// Unwraps a user roster into a c# dictionary of IDs to keys.
    /// </summary>
    private static Dictionary<ulong, byte[]> UnwrapRoster(RosterWrapper* nativeRoster)
    {
        if (nativeRoster == null)
        {
            return [];
        }

        if (nativeRoster->error == AerithWrapperError.OutOfMemory)
        {
            ThrowHelper.ThrowNativeOutOfMemory();
        }

        if (nativeRoster->error == AerithWrapperError.LengthMismatch)
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

        Bindings.AerithDestroyRoster(nativeRoster);

        return roster;
    }
}

static file class ThrowHelper
{
    [DebuggerHidden]
    [StackTraceHidden]
    public static void ThrowNativeOutOfMemory() 
        => throw new OutOfMemoryException("Failed to allocate enough memory for an operation in native MLS code.");

    [DebuggerHidden]
    [StackTraceHidden]
    public static void ThrowNativeInvalidData()
        => throw new InvalidDataException("Encountered invalid data in native MLS code.");
}
