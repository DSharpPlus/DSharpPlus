using DSharpPlus.Logging;
using DSharpPlus.Net.Serialization;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using System.Text;
using System.Threading.Channels;
using System.Threading.Tasks;
using System;

namespace DSharpPlus.Net.InboundWebhooks.Transport;

public sealed class WebhookEventTransportService : IWebhookTransportService
{
    private readonly ILogger<IInteractionTransportService> logger;
    private readonly ChannelWriter<DiscordWebhookEvent> writer;

    public WebhookEventTransportService
    (
        ILogger<IInteractionTransportService> logger,

        [FromKeyedServices("DSharpPlus.Webhooks.EventChannel")]
        Channel<DiscordWebhookEvent> channel
    )
    {
        this.logger = logger;
        this.writer = channel.Writer;
    }

    /// <inheritdoc/>
    public async Task HandleWebhookEventAsync(ArraySegment<byte> payload)
    {
        string payloadString = Encoding.UTF8.GetString(payload);

        // we lump this in with trace logs
        if (RuntimeFeatures.EnableInboundGatewayLogging && this.logger.IsEnabled(LogLevel.Trace))
        {
            this.logger.LogTrace("Received HTTP gateway payload: {Payload}", AnonymizationUtilities.Anonymize(payloadString));
        }

        JObject data = JObject.Parse(payloadString);

        DiscordWebhookEvent? @event = data.ToDiscordObject<DiscordWebhookEvent>();

        if (@event is null)
        {
            this.logger.LogError("Failed to deserialize HTTP gateway payload: {Payload}", AnonymizationUtilities.Anonymize(payloadString));
            return;
        }

        await this.writer.WriteAsync(@event);
    }
}
