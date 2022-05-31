using System.Collections.Generic;
using Newtonsoft.Json;

namespace DSharpPlus.Core.RestEntities
{
    /// <summary>
    /// The are the only valid values for a user to pick, used on <see cref="DiscordApplicationCommandOption.Choices"/>.
    /// </summary>
    public sealed record DiscordApplicationCommandOptionChoice
    {
        /// <summary>
        /// A 1-32 character name that matches against the following Regex: <c>^[-_\p{L}\p{N}\p{sc=Deva}\p{sc=Thai}]{1,32}$</c> with the unicode flag set. If there is a lowercase variant of any letters used, you must use those. Characters with no lowercase variants and/or uncased letters are still allowed. <see cref="DiscordApplicationCommandType.User"/> and <see cref="DiscordApplicationCommandType.Message"/> commands may be mixed case and can include spaces.
        /// </summary>
        [JsonProperty("name", NullValueHandling = NullValueHandling.Ignore)]
        public string Name { get; init; } = null!;

        /// <summary>
        /// Localization dictionary for the <c>name</c> field. Values follow the same restrictions as <c>name</c>.
        /// </summary>
        [JsonProperty("name_localizations", NullValueHandling = NullValueHandling.Ignore)]
        public Optional<IReadOnlyDictionary<string, string>> NameLocalizations { get; init; }

        /// <summary>
        /// Value of the choice, up to 100 characters if string.
        /// </summary>
        /// <remarks>
        /// A string, integer, or double.
        /// </remarks>
        [JsonProperty("value", NullValueHandling = NullValueHandling.Ignore)]
        public object Value { get; init; } = null!;
    }
}
