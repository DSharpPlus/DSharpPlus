using Newtonsoft.Json;
using System;

namespace DSharpPlus.Entities
{
    /// <summary>
    /// Represents data from the original message.
    /// </summary>
    public class DiscordMessageReference
    {
        /// <summary>
        /// Gets the original message.
        /// </summary>
        [JsonIgnore]
        public DiscordMessage Message { get; internal set; }

        /// <summary>
        /// Gets the channel of the original message.
        /// </summary>
        [JsonIgnore]
        public DiscordChannel Channel { get; internal set; }

        /// <summary>
        /// Gets the guild of the original message.
        /// </summary>
        [JsonIgnore]
        public DiscordGuild Guild { get; internal set; }

        [JsonProperty("message_id")]
        internal ulong? messageId { get; set; }

        [JsonProperty("channel_id")]
        internal ulong channelId { get; set; }

        [JsonProperty("guild_id")]
        internal ulong? guildId { get; set; }

        [JsonIgnore]
        internal DiscordClient _client { get; set; }

        internal DiscordMessageReference()
        {
            if (this.guildId.HasValue)
                if (this._client._guilds.TryGetValue(this.guildId.Value, out var g))
                    this.Guild = g;

                else this.Guild = new DiscordGuild
                {
                    Id = this.guildId.Value,
                    Discord = this._client,
                };

            var channel = this._client.InternalGetCachedChannel(this.channelId);

            if (channel == null)
                this.Channel = new DiscordChannel
                {
                    Id = this.channelId,
                    GuildId = this.guildId.Value,
                    Discord = this._client
                };

            else this.Channel = channel;

            this.Message = new DiscordMessage
            {
                ChannelId = this.channelId,
                Discord = this._client
            };

            if (messageId.HasValue)
                if (this._client.MessageCache.TryGet(m => m.Id == messageId.Value && m.ChannelId == channelId, out var msg))
                    this.Message = msg;

                else this.Message.Id = this.messageId.Value;
        }
    }
}
