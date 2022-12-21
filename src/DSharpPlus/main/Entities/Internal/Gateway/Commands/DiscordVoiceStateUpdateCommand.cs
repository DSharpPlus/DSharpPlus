using DSharpPlus.Core.Entities;
using System.Text.Json.Serialization;

namespace DSharpPlus.Core.Entities.Gateway.Commands
{
    public sealed record DiscordVoiceStateUpdateCommand
    {
        /// <summary>
        /// The id of the guild.
        /// </summary>
        [JsonPropertyName("guild_id")]
        public DiscordSnowflake GuildId { get; init; } = null!;

        /// <summary>
        /// The id of the voice channel the client wants to join (null if disconnecting).
        /// </summary>
        [JsonPropertyName("channel_id")]
        public DiscordSnowflake? ChannelId { get; init; }

        /// <summary>
        /// Is the client muted.
        /// </summary>
        [JsonPropertyName("self_mute")]
        public bool SelfMute { get; init; }

        /// <summary>
        /// Is the client deafened.
        /// </summary>
        [JsonPropertyName("self_deaf")]
        public bool SelfDeaf { get; init; }
    }
}
