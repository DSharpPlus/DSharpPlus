using System.Net;

namespace DSharpPlus.Net;

/// <summary>
/// Represents a network connection IP endpoint.
/// </summary>
public struct IpEndpoint
{
    /// <summary>
    /// Gets or sets the hostname associated with this endpoint.
    /// </summary>
    public IPAddress Address { get; set; }

    /// <summary>
    /// Gets or sets the port associated with this endpoint.
    /// </summary>
    public int Port { get; set; }

    /// <summary>
    /// Creates a new IP endpoint structure.
    /// </summary>
    /// <param name="address">IP address to connect to.</param>
    /// <param name="port">Port to use for connection.</param>
    public IpEndpoint(IPAddress address, int port)
    {
        Address = address;
        Port = port;
    }
}
