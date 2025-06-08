using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

namespace DSharpPlus.Voice.Interop.Aerith;

internal static unsafe partial class AerithInterop
{
    [UnmanagedCallersOnly]
    public static void HandleNativeMlsError(byte* site, byte* reason)
    {

    }

    public static AerithSession* CreateSession(string sessionId)
    {
        Span<byte> ascii = stackalloc byte[20];
        Encoding.ASCII.GetBytes(sessionId, ascii);

        fixed (byte* pSessionId = ascii)
        {
            return Bindings.AerithCreateSession(pSessionId, (nuint)ascii.Length, (delegate* unmanaged<byte*, byte*, void>)&HandleNativeMlsError);
        }
    }

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

    public static byte[] GetLastEpochAuthenticator(AerithSession* session)
    {
        VectorWrapper* nativeVector = Bindings.AerithGetLastEpochAuthenticator(session);
        return UnwrapVector(nativeVector);
    }

    public static void SetExternalSender(AerithSession* session, byte[] data)
    {
        fixed (byte* pData = data)
        {
            Bindings.AerithSetExternalSender(session, pData, (nuint)data.Length);
        }
    }

    public static byte[] ProcessProposals(AerithSession* session, byte[] proposals, ReadOnlySpan<ulong> userIds)
    {
        fixed (byte* pProposals = proposals)
        fixed (ulong* pUserIds = userIds)
        {
            VectorWrapper* ack = Bindings.AerithProcessProposals(session, pProposals, (nuint)proposals.Length, pUserIds, userIds.Length);
            return ack == null ? [] : UnwrapVector(ack);
        }
    }

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

    public static Dictionary<ulong, byte[]> ProcessWelcome(AerithSession* session, byte[] welcome, ReadOnlySpan<ulong> userIds)
    {
        fixed (byte* pWelcome = welcome)
        fixed (ulong* pUserIds = userIds)
        {
            RosterWrapper* nativeRoster = Bindings.AerithProcessWelcome(session, pWelcome, (nuint)welcome.Length, pUserIds, userIds.Length);
            return UnwrapRoster(nativeRoster);
        }
    }

    public static byte[] GetMarshalledKeyPackage(AerithSession* session)
    {
        VectorWrapper* nativeKeyPackage = Bindings.AerithGetMarshalledKeyPackage(session);
        return UnwrapVector(nativeKeyPackage);
    }

    private static byte[] UnwrapVector(VectorWrapper* nativeVector)
    {
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

    private static Dictionary<ulong, byte[]> UnwrapRoster(RosterWrapper* nativeRoster)
    {
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
