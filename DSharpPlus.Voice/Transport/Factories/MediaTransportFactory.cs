using System;
using System.Collections.Concurrent;
using System.Net;

using DSharpPlus.Voice.Transport;

/// <inheritdoc/>
public sealed class MediaTransportFactory : IMediaTransportFactory, IDisposable
{
    // dictionary for storing already created UDP clients
    private readonly ConcurrentDictionary<(string? Local, int LPort, string? Remote, int RPort), MediaTransportService> map
        = new();

    // key consists of the main components of each endpoint l and r
    private static (string?, int, string?, int) Key(IPEndPoint? l, IPEndPoint? r)
        => (l?.Address?.ToString(), l?.Port ?? -1, r?.Address?.ToString(), r?.Port ?? -1);


    /// <inheritdoc/>
    public IMediaTransportService Create(IPEndPoint? localBind, IPEndPoint? remotePeer)
    {
        (string?, int, string?, int) key = Key(localBind, remotePeer);
        return this.map.GetOrAdd(key, _ => new MediaTransportService(localBind, remotePeer));
    }


    /// <inheritdoc/>
    public bool Remove(IPEndPoint localBind, IPEndPoint remotePeer)
    {
        (string?, int, string?, int) key = Key(localBind, remotePeer);
        if (this.map.TryRemove(key, out MediaTransportService? svc))
        {
            svc.Dispose();
            return true;
        }
        return false;
    }

    /// <summary>
    /// Disposes of all cached IMediaTransportServices and clears the internal cache map.
    /// </summary>
    public void Dispose()
    {
        foreach (System.Collections.Generic.KeyValuePair<(string? Local, int LPort, string? Remote, int RPort), MediaTransportService> kv in this.map)
        {
            kv.Value.Dispose();
        }

        this.map.Clear();
    }
}
