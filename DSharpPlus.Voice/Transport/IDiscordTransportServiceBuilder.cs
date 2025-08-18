using System;
using System.Net.WebSockets;
using System.Threading.Tasks;

namespace DSharpPlus.Voice.Transport;

public interface IDiscordTransportServiceBuilder
{
    public void AddBinaryHandler(int opCode, Func<ReadOnlyMemory<byte>, DiscordTransportService, Task> handler);
    public void AddJsonHandler<T>(int opCode, Func<T, DiscordTransportService, Task> handler);
    public DiscordTransportService Build(Uri uri);
    public void ConfigureWebSocketOptions(Action<ClientWebSocketOptions> configureOptions);
}
