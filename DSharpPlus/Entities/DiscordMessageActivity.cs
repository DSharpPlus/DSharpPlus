using Newtonsoft.Json;

namespace DSharpPlus.Entities
{
    public class DiscordMessageActivity
    {
        /// <summary>
        /// Gets the activity type.
        /// </summary>
        [JsonProperty("type")]
        public int Type { get; internal set; }

        /// <summary>
        /// Gets the party id of the activity.
        /// </summary>
        [JsonProperty("party_id")]
        public string PartyId { get; internal set; }

        internal DiscordMessageActivity() { }
    }
}
