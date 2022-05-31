using DSharpPlus.Core.Enums;
using Newtonsoft.Json;

namespace DSharpPlus.Core.RestEntities
{
    public sealed record DiscordActivity
    {
        /// <summary>
        /// The activity's name.
        /// </summary>
        [JsonProperty("name", NullValueHandling = NullValueHandling.Ignore)]
        public string Name { get; init; } = null!;

        /// <summary>
        ///The activity type.
        /// </summary>
        [JsonProperty("type", NullValueHandling = NullValueHandling.Ignore)]
        public DiscordActivityType Type { get; init; }

        /// <summary>
        /// The stream url, is validated when type is <see cref="DiscordActivityType.Streaming"/>.
        /// </summary>
        [JsonProperty("url", NullValueHandling = NullValueHandling.Ignore)]
        public Optional<string?> Url { get; init; }

        /// <summary>
        /// A unix timestamp (in milliseconds) of when the activity was added to the user's session.
        /// </summary>
        [JsonProperty("created_at", NullValueHandling = NullValueHandling.Ignore)]
        public int CreatedAt { get; init; }

        /// <summary>
        /// Timestamps for start and/or end of the game.
        /// </summary>
        [JsonProperty("timestamps", NullValueHandling = NullValueHandling.Ignore)]
        public Optional<DiscordActivityTimestamps> Timestamps { get; init; }

        /// <summary>
        /// The application id for the game.
        /// </summary>
        [JsonProperty("application_id", NullValueHandling = NullValueHandling.Ignore)]
        public Optional<DiscordSnowflake> ApplicationId { get; init; }

        /// <summary>
        /// What the player is currently doing.
        /// </summary>
        [JsonProperty("details", NullValueHandling = NullValueHandling.Ignore)]
        public Optional<string?> Details { get; init; }

        /// <summary>
        /// The user's current party status.
        /// </summary>
        [JsonProperty("state", NullValueHandling = NullValueHandling.Ignore)]
        public Optional<string?> State { get; init; }

        /// <summary>
        /// The emoji used for a custom status.
        /// </summary>
        [JsonProperty("emoji", NullValueHandling = NullValueHandling.Ignore)]
        public Optional<DiscordActivityEmoji?> Emoji { get; init; }

        /// <summary>
        /// The information for the current party of the player.
        /// </summary>
        [JsonProperty("party", NullValueHandling = NullValueHandling.Ignore)]
        public Optional<DiscordActivityParty> Party { get; init; }

        /// <summary>
        /// Images for the presence and their hover texts.
        /// </summary>
        [JsonProperty("assets", NullValueHandling = NullValueHandling.Ignore)]
        public Optional<DiscordActivityAssets> Assets { get; init; }

        /// <summary>
        /// Secrets for Rich Presence joining and spectating.
        /// </summary>
        [JsonProperty("secrets", NullValueHandling = NullValueHandling.Ignore)]
        public Optional<DiscordActivitySecrets> Secrets { get; init; }

        /// <summary>
        /// Whether or not the activity is an instanced game session.
        /// </summary>
        [JsonProperty("instance", NullValueHandling = NullValueHandling.Ignore)]
        public Optional<bool> Instance { get; init; }

        /// <summary>
        /// <see cref="DiscordActivityFlags"/> <c>OR</c>'d together, describes what the payload includes.
        /// </summary>
        [JsonProperty("flags", NullValueHandling = NullValueHandling.Ignore)]
        public Optional<DiscordActivityFlags> Flags { get; init; }

        /// <summary>
        /// The custom buttons shown in the Rich Presence (max 2).
        /// </summary>
        [JsonProperty("buttons", NullValueHandling = NullValueHandling.Ignore)]
        public Optional<DiscordActivityButton> Buttons { get; init; }
    }
}
