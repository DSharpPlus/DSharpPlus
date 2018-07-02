using DSharpPlus.Entities;
using Newtonsoft.Json;

namespace DSharpPlus.EventArgs
{
    /// <summary>
    /// Represents arguments for <see cref="DiscordClient.VoiceServerUpdated"/> event.
    /// </summary>
    public class VoiceServerUpdateEventArgs : DiscordEventArgs
    {
        /// <summary>
        /// Gets the guild for which the update occured.
        /// </summary>
		[JsonIgnore]
        public DiscordGuild Guild { get; internal set; }

		[JsonProperty("guild_id")]
		internal ulong GuildId { get { return Guild.Id; } set { } }

        /// <summary>
        /// Gets the new voice endpoint.
        /// </summary>
		[JsonProperty("endpoint")]
        public string Endpoint { get; internal set; }

		/// <summary>
		/// Gets the voice connection token. Do not share this.
		/// </summary>
		[JsonProperty("token")]
		public string VoiceToken { get; set; }

        internal VoiceServerUpdateEventArgs(DiscordClient client) : base(client) { }
    }
}
