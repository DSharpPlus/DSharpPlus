using Newtonsoft.Json;

namespace DSharpPlus.Core.RestEntities
{
    /// <summary>
    /// Active sessions are indicated with an "online", "idle", or "dnd" string per platform. If a user is offline or invisible, the corresponding field is not present.
    /// </summary>
    public sealed record DiscordClientStatus
    {
        /// <summary>
        /// The user's status set for an active desktop (Windows, Linux, Mac) application session.
        /// </summary>
        [JsonProperty("desktop", NullValueHandling = NullValueHandling.Ignore)]
        public Optional<string> Desktop { get; init; }

        /// <summary>
        /// The user's status set for an active mobile (iOS, Android) application session.
        /// </summary>
        [JsonProperty("mobile", NullValueHandling = NullValueHandling.Ignore)]
        public Optional<string> Mobile { get; init; }

        /// <summary>
        /// The user's status set for an active web (browser, bot account) application session.
        /// </summary>
        [JsonProperty("web", NullValueHandling = NullValueHandling.Ignore)]
        public Optional<string> Web { get; init; }
    }
}
