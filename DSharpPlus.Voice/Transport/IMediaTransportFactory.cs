using System;
using System.Net;

using DSharpPlus.Voice.Transport;

/// <summary>
/// Provides a mechanism to obtain and cache <see cref="IMediaTransportService"/>s.
/// </summary>
public interface IMediaTransportFactory : IDisposable
{
    /// <summary>
    /// Returns an instance of IMediaTransportService with the local and remote bindings given to the method.
    /// </summary>
    /// <param name="localBind">The local bind of this connection.</param>
    /// <param name="remotePeer">The remote bind of this connection.</param>
    public IMediaTransportService Create(IPEndPoint localBind, IPEndPoint remotePeer);

    /// <summary>
    /// Disposes of any cached IMediaTransportService instances matching the specified addresses.
    /// </summary>
    public bool Remove(IPEndPoint localBind, IPEndPoint remotePeer);
}
