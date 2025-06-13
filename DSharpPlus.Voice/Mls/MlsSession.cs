using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

using DSharpPlus.Voice.Interop.Aerith;

namespace DSharpPlus.Voice.Mls;

/// <summary>
/// A class tracking and maintaining a MLS session for the lifetime of a single connection.
/// </summary>
internal unsafe class MlsSession : IDisposable
{
    private readonly AerithSession* nativeSession;
    private readonly ConcurrentDictionary<ulong, byte[]> users;
    private bool disposed = false;

    public IReadOnlyDictionary<ulong, byte[]> Users => this.users;

    public MlsSession(string sessionId, ulong channelId, ulong userId)
    {
        this.nativeSession = AerithInterop.CreateSession(sessionId);
        AerithInterop.InitializeSession(this.nativeSession, sessionId, channelId, userId);

        this.users = [];
    }

    public void ProcessWelcome(byte[] welcomePayload, ReadOnlySpan<ulong> users)
    {
        Dictionary<ulong, byte[]> mlsUsers = AerithInterop.ProcessWelcome(this.nativeSession, welcomePayload, users);

        foreach (KeyValuePair<ulong, byte[]> kvp in mlsUsers)
        {
            if (kvp.Value is [])
            {
                // how are we here.
                continue;
            }

            this.users.GetOrAdd(kvp.Key, kvp.Value);
        }
    }

    public void Dispose()
    {
        if (!this.disposed)
        {
            AerithInterop.DestroySession(this.nativeSession);
            this.disposed = true;
        }
    }
}
