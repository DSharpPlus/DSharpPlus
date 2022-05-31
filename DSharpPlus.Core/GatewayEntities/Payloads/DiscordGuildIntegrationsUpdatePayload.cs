using DSharpPlus.Core.Attributes;
using DSharpPlus.Core.RestEntities;
using Newtonsoft.Json;

namespace DSharpPlus.Core.GatewayEntities.Payloads
{
    [DiscordGatewayPayload("GUILD_INTEGRATIONS_UPDATE")]
    public sealed record DiscordGuildIntegrationsUpdatePayload
    {
        /// <summary>
        /// The id of the guild whose integrations were updated.
        /// </summary>
        [JsonProperty("guild_id", NullValueHandling = NullValueHandling.Ignore)]
        public DiscordSnowflake GuildId { get; init; } = null!;
    }
}
