using System;
using System.Text;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;

using DSharpPlus.Entities;
using DSharpPlus.Net.Abstractions;
using DSharpPlus.Net.Serialization;

using Microsoft.Extensions.Logging;

using Newtonsoft.Json.Linq;

namespace DSharpPlus.Net.InboundWebhooks.Integration;

/// <inheritdoc/>
public sealed class InteractionTransportService : IInteractionTransportService
{
    private readonly ILogger<IInteractionTransportService> logger;
    private readonly ChannelWriter<DiscordHttpInteractionPayload> writer;

    /// <inheritdoc/>
    public async Task<byte[]> HandleHttpInteractionAsync(ArraySegment<byte> payload, CancellationToken token)
    {
        string bodyString = Encoding.UTF8.GetString(payload);

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
