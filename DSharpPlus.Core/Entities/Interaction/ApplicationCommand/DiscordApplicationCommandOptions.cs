// This file is part of the DSharpPlus project.
//
// Copyright (c) 2015 Mike Santiago
// Copyright (c) 2016-2022 DSharpPlus Contributors
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.

using System.Collections.Generic;
using DSharpPlus.Core.Enums;
using Newtonsoft.Json;

namespace DSharpPlus.Core.Entities
{
    public sealed record DiscordApplicationCommandOption
    {
        /// <summary>
        /// The type of option.
        /// </summary>
        [JsonProperty("type", NullValueHandling = NullValueHandling.Ignore)]
        public DiscordApplicationCommandOptionType Type { get; init; }

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
        /// 1-100 character description for <see cref="DiscordApplicationCommandType.ChatInput"/> commands, empty string for <see cref="DiscordApplicationCommandType.User"/> and <see cref="DiscordApplicationCommandType.Message"/> commands.
        /// </summary>
        [JsonProperty("description", NullValueHandling = NullValueHandling.Ignore)]
        public string Description { get; init; } = null!;

        /// <summary>
        /// Localization dictionary for the <c>description</c> field. Values follow the same restrictions as <c>description</c>.
        /// </summary>
        [JsonProperty("description_localizations", NullValueHandling = NullValueHandling.Ignore)]
        public Optional<IReadOnlyDictionary<string, string>> DescriptionLocalizations { get; init; }

        /// <summary>
        /// If the parameter is required or optional. Defaults to false.
        /// </summary>
        [JsonProperty("required", NullValueHandling = NullValueHandling.Ignore)]
        public Optional<bool> Required { get; init; }

        /// <summary>
        /// The choices for <see cref="DiscordApplicationCommandOptionType.String"/>, <see cref="DiscordApplicationCommandOptionType.Integer"/>, and <see cref="DiscordApplicationCommandOptionType.Number"/> types for the user to pick from, max 25.
        /// </summary>
        [JsonProperty("choices", NullValueHandling = NullValueHandling.Ignore)]
        public Optional<DiscordApplicationCommandOptionChoice> Choices { get; init; }

        /// <summary>
        /// If the option is a subcommand or subcommand group type, these nested options will be the parameters.
        /// </summary>
        [JsonProperty("options", NullValueHandling = NullValueHandling.Ignore)]
        public Optional<DiscordApplicationCommandOption> Options { get; init; }

        /// <summary>
        /// If the option is a channel type, the channels shown will be restricted to these types.
        /// </summary>
        [JsonProperty("channel_types", NullValueHandling = NullValueHandling.Ignore)]
        public Optional<DiscordChannelType> ChannelTypes { get; init; }

        /// <summary>
        /// If the option is an <see cref="DiscordApplicationCommandOptionType.Integer"/>, or <see cref="DiscordApplicationCommandOptionType.Number"/> type, the minimum value permitted.
        /// </summary>
        [JsonProperty("min_value", NullValueHandling = NullValueHandling.Ignore)]
        public Optional<object> MinValue { get; init; }

        /// <summary>
        /// If the option is an <see cref="DiscordApplicationCommandOptionType.Integer"/>, or <see cref="DiscordApplicationCommandOptionType.Number"/> type, the maximum value permitted.
        /// </summary>
        [JsonProperty("max_value", NullValueHandling = NullValueHandling.Ignore)]
        public Optional<object> MaxValue { get; init; }

        /// <summary>
        /// If autocomplete interactions are enabled for this <see cref="DiscordApplicationCommandOptionType.String"/>, <see cref="DiscordApplicationCommandOptionType.Integer"/>, or <see cref="DiscordApplicationCommandOptionType.Number"/> type option.
        /// </summary>
        /// <remarks>
        /// Autocomplete may not be set to true if <see cref="Choices"/> are present.
        /// </remarks>
        [JsonProperty("autocomplete", NullValueHandling = NullValueHandling.Ignore)]
        public Optional<bool> AutoComplete { get; init; }
    }
}
