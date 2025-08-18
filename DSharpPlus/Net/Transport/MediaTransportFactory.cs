using System;
using System.Collections.Concurrent;
using System.Net;

using DSharpPlus.Net.Transport;

/// <summary>
/// Factory for creating MediaTransportService instances
/// </summary>
public sealed class MediaTransportFactory : IMediaTransportFactory, IDisposable
{
    /// <summary>
    /// dictionary for storing already created UDP clients
    /// </summary>
    private readonly ConcurrentDictionary<(string? Local, int LPort, string? Remote, int RPort), MediaTransportService> map
        = new();

    /// <summary>
    /// Key consists of the main components of each endpoint l and r
    /// </summary>
    /// <param name="l"></param>
    /// <param name="r"></param>
    /// <returns></returns>
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
    /// Dispose of all cached IMediaTransportServices and clears the internal cache map
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
