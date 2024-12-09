using System;
using System.Text;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;

using DSharpPlus.Entities;
using DSharpPlus.Logging;
using DSharpPlus.Net.Abstractions;
using DSharpPlus.Net.Serialization;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using Newtonsoft.Json.Linq;

namespace DSharpPlus.Net.InboundWebhooks.Transport;

/// <inheritdoc/>
public sealed class InteractionTransportService : IInteractionTransportService
{
    private readonly ILogger<IInteractionTransportService> logger;
    private readonly ChannelWriter<DiscordHttpInteractionPayload> writer;

    public InteractionTransportService
    (
        ILogger<IInteractionTransportService> logger,

        [FromKeyedServices("DSharpPlus.Interactions.EventChannel")]
        Channel<DiscordHttpInteractionPayload> channel
    )
    {
        this.logger = logger;
        this.writer = channel.Writer;
    }

    /// <inheritdoc/>
    public async Task<byte[]> HandleHttpInteractionAsync(ArraySegment<byte> payload, CancellationToken token)
    {
        string bodyString = Encoding.UTF8.GetString(payload);

        // we lump this in with trace logs
        if (RuntimeFeatures.EnableInboundGatewayLogging && this.logger.IsEnabled(LogLevel.Trace))
        {
            this.logger.LogTrace("Received HTTP interaction payload: {Payload}", AnonymizationUtilities.Anonymize(bodyString));
        }

        JObject data = JObject.Parse(bodyString);

        DiscordHttpInteraction? interaction = data.ToDiscordObject<DiscordHttpInteraction>() 
            ?? throw new ArgumentException("Unable to parse provided request body to DiscordHttpInteraction");

        if (interaction.Type is DiscordInteractionType.Ping)
        {
            DiscordInteractionResponsePayload responsePayload = new() 
            { 
                Type = DiscordInteractionResponseType.Pong 
            };

            string responseString = DiscordJson.SerializeObject(responsePayload);
            byte[] responseBytes = Encoding.UTF8.GetBytes(responseString);

            return responseBytes;
        }

        token.Register(() => interaction.Cancel());

        await this.writer.WriteAsync(new(interaction, data), token);

        return await interaction.GetResponseAsync();
    }
}
