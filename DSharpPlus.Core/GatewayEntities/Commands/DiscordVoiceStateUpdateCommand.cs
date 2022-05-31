using DSharpPlus.Core.RestEntities;
using Newtonsoft.Json;

namespace DSharpPlus.Core.GatewayEntities.Commands
{
    public sealed record DiscordVoiceStateUpdateCommand
    {
        /// <summary>
        /// The id of the guild.
        /// </summary>
        [JsonProperty("guild_id", NullValueHandling = NullValueHandling.Ignore)]
        public DiscordSnowflake GuildId { get; init; } = null!;

        /// <summary>
        /// The id of the voice channel the client wants to join (null if disconnecting).
        /// </summary>
        [JsonProperty("channel_id", NullValueHandling = NullValueHandling.Ignore)]
        public DiscordSnowflake? ChannelId { get; init; }

        /// <summary>
        /// Is the client muted.
        /// </summary>
        [JsonProperty("self_mute", NullValueHandling = NullValueHandling.Ignore)]
        public bool SelfMute { get; init; }

        /// <summary>
        /// Is the client deafened.
        /// </summary>
        [JsonProperty("self_deaf", NullValueHandling = NullValueHandling.Ignore)]
        public bool SelfDeaf { get; init; }
    }
}
