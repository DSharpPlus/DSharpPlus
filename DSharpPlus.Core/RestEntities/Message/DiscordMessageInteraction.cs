using DSharpPlus.Core.Enums;
using Newtonsoft.Json;

namespace DSharpPlus.Core.RestEntities
{
    /// <summary>
    /// This is sent on the message object when the message is a response to an Interaction without an existing message.
    /// </summary>
    /// <remarks>
    /// This means responses to Message Components do not include this property, instead including a message reference object as components always exist on preexisting messages.
    /// </remarks>
    public sealed record DiscordMessageInteraction
    {
        /// <summary>
        /// The id of the interaction.
        /// </summary>
        [JsonProperty("id", NullValueHandling = NullValueHandling.Ignore)]
        public DiscordSnowflake Id { get; init; } = null!;

        /// <summary>
        /// The type of interaction.
        /// </summary>
        [JsonProperty("type", NullValueHandling = NullValueHandling.Ignore)]
        public DiscordInteractionType Type { get; init; }

        /// <summary>
        /// The name of the application command.
        /// </summary>
        [JsonProperty("name", NullValueHandling = NullValueHandling.Ignore)]
        public string Name { get; init; } = null!;

        /// <summary>
        /// The user who invoked the interaction.
        /// </summary>
        [JsonProperty("user", NullValueHandling = NullValueHandling.Ignore)]
        public DiscordUser User { get; init; } = null!;

        /// <summary>
        /// The member who invoked the interaction in the guild.
        /// </summary>
        [JsonProperty("member", NullValueHandling = NullValueHandling.Ignore)]
        public Optional<DiscordGuildMember> Member { get; init; }
    }
}
