using System;
using System.Net;

using DSharpPlus.Net.Transport;

public interface IMediaTransportFactory : IDisposable
{
    /// <summary>
    /// Returns an instance of IMediaTransportService with the local and remote bindings given to the method
    /// </summary>
    /// <param name="localBind">local bindings</param>
    /// <param name="remotePeer">remote bindings</param>
    /// <returns></returns>
    public IMediaTransportService Create(IPEndPoint localBind, IPEndPoint remotePeer);
    /// <summary>
    /// Disposes of any cached IMediaTransportService instances
    /// </summary>
    /// <param name="localBind"></param>
    /// <param name="remotePeer"></param>
    /// <returns></returns>
    public bool Remove(IPEndPoint localBind, IPEndPoint remotePeer);
}
