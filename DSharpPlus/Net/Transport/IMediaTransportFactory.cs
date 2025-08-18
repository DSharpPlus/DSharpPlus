using System;
using System.Net;

using DSharpPlus.Net.Transport;

public interface IMediaTransportFactory : IDisposable
{
    public IMediaTransportService GetOrCreate(IPEndPoint localBind, IPEndPoint remotePeer);
    public bool Remove(IPEndPoint localBind, IPEndPoint remotePeer);
}
