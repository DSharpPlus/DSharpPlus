using System.Collections.Generic;
using DSharpPlus.Core.Enums;
using System.Text.Json.Serialization;

namespace DSharpPlus.Core.RestEntities
{
    /// <summary>
    /// Application commands are commands that an application can register to Discord. They provide users a first-class way of interacting directly with your application that feels deeply integrated into Discord.
    /// </summary>
    public sealed record DiscordApplicationCommand
    {
        /// <summary>
        /// The unique id of the command.
        /// </summary>
        [JsonPropertyName("id")]
        public DiscordSnowflake Id { get; init; } = null!;

        /// <summary>
        /// The type of command, defaults <see cref="DiscordApplicationCommandType.ChatInput"/> if not set.
        /// </summary>
        [JsonPropertyName("type")]
        public Optional<DiscordApplicationCommandType> Type { get; init; }

        /// <summary>
        /// The unique id of the parent application.
        /// </summary>
        [JsonPropertyName("application_id")]
        public DiscordSnowflake ApplicationId { get; init; } = null!;

        /// <summary>
        /// The guild id of the command, if not global.
        /// </summary>
        [JsonPropertyName("guild_id")]
        public Optional<DiscordSnowflake> GuildId { get; init; }

        /// <summary>
        /// A 1-32 character name that matches against the following Regex: <c>^[-_\p{L}\p{N}\p{sc=Deva}\p{sc=Thai}]{1,32}$</c> with the unicode flag set. If there is a lowercase variant of any letters used, you must use those. Characters with no lowercase variants and/or uncased letters are still allowed. <see cref="DiscordApplicationCommandType.User"/> and <see cref="DiscordApplicationCommandType.Message"/> commands may be mixed case and can include spaces.
        /// </summary>
        [JsonPropertyName("name")]
        public string Name { get; init; } = null!;

        /// <summary>
        /// Localization dictionary for the <c>name</c> field. Values follow the same restrictions as <c>name</c>.
        /// </summary>
        [JsonPropertyName("name_localizations")]
        public Optional<IReadOnlyDictionary<string, string>> NameLocalizations { get; init; }

        /// <summary>
        /// 1-100 character description for <see cref="DiscordApplicationCommandType.ChatInput"/> commands, empty string for <see cref="DiscordApplicationCommandType.User"/> and <see cref="DiscordApplicationCommandType.Message"/> commands.
        /// </summary>
        [JsonPropertyName("description")]
        public string Description { get; init; } = null!;

        /// <summary>
        /// Localization dictionary for the <c>description</c> field. Values follow the same restrictions as <c>description</c>.
        /// </summary>
        [JsonPropertyName("description_localizations")]
        public Optional<IReadOnlyDictionary<string, string>> DescriptionLocalizations { get; init; }

        /// <summary>
        /// The parameters for the command, max 25.
        /// </summary>
        /// <remarks>
        /// Required options must be listed before optional options.
        /// </remarks>
        [JsonPropertyName("options")]
        public Optional<DiscordApplicationCommandOption> Options { get; init; }

        /// <summary>
        /// Whether the command is enabled by default when the app is added to a guild (default <c>true</c>).
        /// </summary>
        [JsonPropertyName("default_permission")]
        public Optional<bool> DefaultPermission { get; init; }

        /// <summary>
        /// An autoincrementing version identifier updated during substantial record changes.
        /// </summary>
        [JsonPropertyName("version")]
        public DiscordSnowflake Version { get; init; } = null!;
    }
}
