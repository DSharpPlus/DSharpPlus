using System;
using System.Threading.Tasks;

namespace DSharpPlus.Net.InboundWebhooks.Transport;

public interface IWebhookTransportService
{
    public Task HandleWebhookEventAsync(ArraySegment<byte> payload);
}
