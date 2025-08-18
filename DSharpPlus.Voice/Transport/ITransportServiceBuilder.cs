using System;
using System.Net.WebSockets;
using System.Threading.Tasks;

namespace DSharpPlus.Voice.Transport;

/// <summary>
/// An abstraction for creating a transport service.
/// </summary>
public interface ITransportServiceBuilder
{
    /// <summary>
    /// Adds a handler for the specified opcode for binary messages..
    /// </summary>
    public void AddBinaryHandler(int opcode, Func<ReadOnlyMemory<byte>, TransportService, Task> handler);

    /// <summary>
    /// Adds a handler for the specified opcode for json messages.
    /// </summary>
    /// <typeparam name="T">The payload type to deserialize into.</typeparam>
    public void AddJsonHandler<T>(int opcode, Func<T, TransportService, Task> handler);

    /// <summary>
    /// Builds the desired transport service.
    /// </summary>
    public ITransportService Build(Uri uri);
}
