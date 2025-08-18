using System;
using System.Threading;
using System.Threading.Tasks;

namespace DSharpPlus.Voice.Transport;
public interface IDiscordTransportService
{
    public Task ConnectAsync();
    public Task OnBaseBinary(ReadOnlyMemory<byte> binaryResponse);
    public Task OnBaseText(string messageText);
    public Task Send<T>(T data, CancellationToken? token = null);
    public Task Send(ReadOnlyMemory<byte> data, CancellationToken? token = null);
}
