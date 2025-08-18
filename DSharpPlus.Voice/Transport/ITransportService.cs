using System;
using System.Threading;
using System.Threading.Tasks;

namespace DSharpPlus.Voice.Transport;

/// <summary>
/// Represents a mechanism for transporting data to and from the voice gateway server.
/// </summary>
public interface ITransportService
{
    /// <summary>
    /// Connects the client to the configured endpoint
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public Task ConnectAsync(CancellationToken? cancellationToken = null);

    /// <summary>
    /// Sends bytes data to the active connection
    /// </summary>
    /// <param name="data"></param>
    /// <param name="token"></param>
    /// <returns></returns>
    public Task SendAsync(ReadOnlyMemory<byte> data, CancellationToken? token = null);

    /// <summary>
    /// Converts T data to json and sends it as UTF8 bytes to the active connection
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="data"></param>
    /// <param name="token"></param>
    /// <returns></returns>
    public Task SendAsync<T>(T data, CancellationToken? token = null);
}
