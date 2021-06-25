// This file is part of the DSharpPlus project.
//
// Copyright (c) 2015 Mike Santiago
// Copyright (c) 2016-2021 DSharpPlus Contributors
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

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Newtonsoft.Json;

namespace DSharpPlus.Entities
{
    /// <summary>
    /// Represents a parameter for a <see cref="DiscordApplicationCommand"/>.
    /// </summary>
    public sealed class DiscordApplicationCommandOption
    {
        /// <summary>
        /// Gets the type of this command parameter.
        /// </summary>
        [JsonProperty("type")]
        public ApplicationCommandOptionType Type { get; internal set; }

        /// <summary>
        /// Gets the name of this command parameter.
        /// </summary>
        [JsonProperty("name")]
        public string Name { get; internal set; }

        /// <summary>
        /// Gets the description of this command parameter.
        /// </summary>
        [JsonProperty("description")]
        public string Description { get; internal set; }

        /// <summary>
        /// Gets whether this command parameter is required.
        /// </summary>
        [JsonProperty("required", NullValueHandling = NullValueHandling.Ignore)]
        public bool? Required { get; internal set; }

        /// <summary>
        /// Gets the optional choices for this command parameter.
        /// </summary>
        [JsonProperty("choices", NullValueHandling = NullValueHandling.Ignore)]
        public IReadOnlyCollection<DiscordApplicationCommandOptionChoice> Choices { get; internal set; }

        /// <summary>
        /// Gets the optional subcommand parameters for this parameter.
        /// </summary>
        [JsonProperty("options", NullValueHandling = NullValueHandling.Ignore)]
        public IReadOnlyCollection<DiscordApplicationCommandOption> Options { get; internal set; }

        /// <summary>
        /// Creates a new instance of a <see cref="DiscordApplicationCommandOption"/>.
        /// </summary>
        /// <param name="name">The name of this parameter.</param>
        /// <param name="description">The description of the parameter.</param>
        /// <param name="type">The type of this parameter.</param>
        /// <param name="required">Whether the parameter is required.</param>
        /// <param name="choices">The optional choice selection for this parameter.</param>
        /// <param name="options">The optional subcommands for this parameter.</param>
        public DiscordApplicationCommandOption(string name, string description, ApplicationCommandOptionType type, bool? required = null, IEnumerable<DiscordApplicationCommandOptionChoice> choices = null, IEnumerable<DiscordApplicationCommandOption> options = null)
        {
            if (!Utilities.IsValidSlashCommandName(name))
                throw new ArgumentException("Invalid slash command option name specified. It must be below 32 characters and not contain any whitespace.", nameof(name));
            if (name.Any(ch => char.IsUpper(ch)))
                throw new ArgumentException("Slash command option name cannot have any upper case characters.", nameof(name));
            if (description.Length > 100)
                throw new ArgumentException("Slash command option description cannot exceed 100 characters.", nameof(description));

            var choiceList = choices != null ? new ReadOnlyCollection<DiscordApplicationCommandOptionChoice>(choices.ToList()) : null;
            var optionList = options != null ? new ReadOnlyCollection<DiscordApplicationCommandOption>(options.ToList()) : null;

            this.Name = name;
            this.Description = description;
            this.Type = type;
            this.Required = required;
            this.Choices = choiceList;
            this.Options = optionList;
        }
    }
}
