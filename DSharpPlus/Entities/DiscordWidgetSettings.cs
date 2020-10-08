using Newtonsoft.Json;

namespace DSharpPlus.Entities
{
	/// <summary>
	/// Represents a Discord guild's widget settings.
	/// </summary>
	public class DiscordWidgetSettings
    {
        internal DiscordGuild Guild;

        [JsonProperty("enabled", NullValueHandling = NullValueHandling.Ignore)]
        internal bool Enabled { get; set; }

        /// <summary>
        /// Gets if the guild's widget is enabled.
        /// </summary>
        public bool IsEnabled
            => this.Enabled;

        [JsonProperty("channel_id", NullValueHandling = NullValueHandling.Ignore)]
        internal ulong ChannelId { get; set; }

        /// <summary>
        /// Gets the guild's widget channel.
        /// </summary>
        public DiscordChannel Channel
            => this.Guild.GetChannel(this.ChannelId);

    }
}
