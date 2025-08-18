using System;
using System.Collections.Concurrent;
using System.Net;

using DSharpPlus.Net.Transport;

public sealed class MediaTransportFactory : IMediaTransportFactory, IDisposable
{
    private readonly ConcurrentDictionary<(string? Local, int LPort, string? Remote, int RPort), MediaTransportService> map
        = new();

    // Key consists of the main components of each endpoint
    private static (string?, int, string?, int) Key(IPEndPoint? l, IPEndPoint? r)
        => (l?.Address?.ToString(), l?.Port ?? -1, r?.Address?.ToString(), r?.Port ?? -1);

    public IMediaTransportService GetOrCreate(IPEndPoint? localBind, IPEndPoint? remotePeer)
    {
        (string?, int, string?, int) key = Key(localBind, remotePeer);
        return this.map.GetOrAdd(key, _ => new MediaTransportService(localBind, remotePeer));
    }

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

    public void Dispose()
    {
        foreach (System.Collections.Generic.KeyValuePair<(string? Local, int LPort, string? Remote, int RPort), MediaTransportService> kv in this.map)
        {
            kv.Value.Dispose();
        }

        this.map.Clear();
    }
}
