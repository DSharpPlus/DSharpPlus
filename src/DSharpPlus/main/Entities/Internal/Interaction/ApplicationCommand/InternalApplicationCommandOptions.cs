using System.Collections.Generic;
using DSharpPlus.Core.Enums;
using System.Text.Json.Serialization;

namespace DSharpPlus.Core.Entities
{
    public sealed record InternalApplicationCommandOption
    {
        /// <summary>
        /// The type of option.
        /// </summary>
        [JsonPropertyName("type")]
        public InternalApplicationCommandOptionType Type { get; init; }

        /// <summary>
        /// A 1-32 character name that matches against the following Regex: <c>^[-_\p{L}\p{N}\p{sc=Deva}\p{sc=Thai}]{1,32}$</c> with the unicode flag set. If there is a lowercase variant of any letters used, you must use those. Characters with no lowercase variants and/or uncased letters are still allowed. <see cref="InternalApplicationCommandType.User"/> and <see cref="InternalApplicationCommandType.Message"/> commands may be mixed case and can include spaces.
        /// </summary>
        [JsonPropertyName("name")]
        public string Name { get; init; } = null!;

        /// <summary>
        /// Localization dictionary for the <c>name</c> field. Values follow the same restrictions as <c>name</c>.
        /// </summary>
        [JsonPropertyName("name_localizations")]
        public Optional<IReadOnlyDictionary<string, string>> NameLocalizations { get; init; }

        /// <summary>
        /// 1-100 character description for <see cref="InternalApplicationCommandType.ChatInput"/> commands, empty string for <see cref="InternalApplicationCommandType.User"/> and <see cref="InternalApplicationCommandType.Message"/> commands.
        /// </summary>
        [JsonPropertyName("description")]
        public string Description { get; init; } = null!;

        /// <summary>
        /// Localization dictionary for the <c>description</c> field. Values follow the same restrictions as <c>description</c>.
        /// </summary>
        [JsonPropertyName("description_localizations")]
        public Optional<IReadOnlyDictionary<string, string>> DescriptionLocalizations { get; init; }

        /// <summary>
        /// If the parameter is required or optional. Defaults to false.
        /// </summary>
        [JsonPropertyName("required")]
        public Optional<bool> Required { get; init; }

        /// <summary>
        /// The choices for <see cref="InternalApplicationCommandOptionType.String"/>, <see cref="InternalApplicationCommandOptionType.Integer"/>, and <see cref="InternalApplicationCommandOptionType.Number"/> types for the user to pick from, max 25.
        /// </summary>
        [JsonPropertyName("choices")]
        public Optional<InternalApplicationCommandOptionChoice> Choices { get; init; }

        /// <summary>
        /// If the option is a subcommand or subcommand group type, these nested options will be the parameters.
        /// </summary>
        [JsonPropertyName("options")]
        public Optional<InternalApplicationCommandOption> Options { get; init; }

        /// <summary>
        /// If the option is a channel type, the channels shown will be restricted to these types.
        /// </summary>
        [JsonPropertyName("channel_types")]
        public Optional<InternalChannelType> ChannelTypes { get; init; }

        /// <summary>
        /// If the option is an <see cref="InternalApplicationCommandOptionType.Integer"/>, or <see cref="InternalApplicationCommandOptionType.Number"/> type, the minimum value permitted.
        /// </summary>
        [JsonPropertyName("min_value")]
        public Optional<object> MinValue { get; init; }

        /// <summary>
        /// If the option is an <see cref="InternalApplicationCommandOptionType.Integer"/>, or <see cref="InternalApplicationCommandOptionType.Number"/> type, the maximum value permitted.
        /// </summary>
        [JsonPropertyName("max_value")]
        public Optional<object> MaxValue { get; init; }

        /// <summary>
        /// If autocomplete interactions are enabled for this <see cref="InternalApplicationCommandOptionType.String"/>, <see cref="InternalApplicationCommandOptionType.Integer"/>, or <see cref="InternalApplicationCommandOptionType.Number"/> type option.
        /// </summary>
        /// <remarks>
        /// Autocomplete may not be set to true if <see cref="Choices"/> are present.
        /// </remarks>
        [JsonPropertyName("autocomplete")]
        public Optional<bool> AutoComplete { get; init; }
    }
}
