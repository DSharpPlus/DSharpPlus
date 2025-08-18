using System;
using System.Net.WebSockets;
using System.Threading.Tasks;

namespace DSharpPlus.Voice.Transport;

public interface IDiscordTransportServiceBuilder
{
    /// <summary>
    /// Adds a callback method to run when binary data is received from discord with the specified opCode
    /// </summary>
    /// <param name="opCode"></param>
    /// <param name="handler"></param>
    public void AddBinaryHandler(int opCode, Func<ReadOnlyMemory<byte>, DiscordTransportService, Task> handler);
    /// <summary>
    /// Adds a callback method to run when json data is received from discord with the specified opCode.
    /// It will attempt to deserialze the data to type T
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="opCode"></param>
    /// <param name="handler"></param>
    public void AddJsonHandler<T>(int opCode, Func<T, DiscordTransportService, Task> handler);

    /// <summary>
    /// Adds a callback method that will be called with the ClientWebSocketOptions when the websocket client is created
    /// </summary>
    /// <param name="configureOptions"></param>
    public void ConfigureWebSocketOptions(Action<ClientWebSocketOptions> configureOptions);


    /// <summary>
    /// Returns the created DiscordTransportService with the configured handlers, WebSocket Options, and Uri
    /// </summary>
    /// <param name="uri"></param>
    /// <returns></returns>
    public ITransportService Build(Uri uri);
}
