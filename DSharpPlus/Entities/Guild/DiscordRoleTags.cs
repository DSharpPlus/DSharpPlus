using Newtonsoft.Json;

namespace DSharpPlus.Entities
{
    public class DiscordRoleTags
    {
        /// <summary>
        /// Gets the id of the bot this role belongs to.
        /// </summary>
        [JsonProperty("bot_id", NullValueHandling = NullValueHandling.Ignore)]
        public ulong? BotID { get; internal set; }

        /// <summary>
        /// Gets the id of the integration this role belongs to.
        /// </summary>
        [JsonProperty("integration_id", NullValueHandling = NullValueHandling.Ignore)]
        public ulong? IntegrationID { get; internal set; }

        /// <summary>
        /// Gets whether this is the guild's premium subscriber role.
        /// </summary>
        [JsonIgnore]
        public bool IsPremiumSubscriber
        {
            get
            {
                if(_premium_subscriber.HasValue)
                {
                    return _premium_subscriber.Value;
                } else
                {
                    return true;
                }
            }
        }

        [JsonProperty("premium_subscriber", NullValueHandling = NullValueHandling.Include)]
        internal bool? _premium_subscriber = false;

    }
}
