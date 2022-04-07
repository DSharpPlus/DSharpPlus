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

using System.Text.Json.Serialization;
using DSharpPlus.Core.Enums;

namespace DSharpPlus.Core.Entities
{
    /// <summary>
    /// All options have names, and an option can either be a parameter and input value--in which case value will be set--or it can denote a subcommand or group--in which case it will contain a top-level key and another array of options.
    /// </summary>
    public sealed record DiscordApplicationInteractionDataOption
    {
        /// <summary>
        /// The name of the parameter.
        /// </summary>
        [JsonPropertyName("name")]
        public string Name { get; init; } = null!;

        /// <summary>
        /// The value of <see cref="DiscordApplicationCommandOptionType"/>.
        /// </summary>
        [JsonPropertyName("type")]
        public DiscordApplicationCommandOptionType Type { get; init; }

        /// <summary>
        /// The value of the option resulting from user input.
        /// </summary>
        /// <remarks>
        /// A string, integer, or double. Mutually exclusive with <see cref="Options"/>.
        /// </remarks>
        [JsonPropertyName("value")]
        public Optional<object> Value { get; init; }

        /// <summary>
        /// Only present if this option is a <see cref="DiscordApplicationCommandOptionType.SubCommand"/> or <see cref="DiscordApplicationCommandOptionType.SubCommandGroup"/>.
        /// </summary>
        /// <remarks>
        /// Mutually exclusive with <see cref="Value"/>.
        /// </remarks>
        [JsonPropertyName("options")]
        public Optional<DiscordApplicationInteractionDataOption[]> Options { get; init; }

        /// <summary>
        /// True if this option is the currently focused option for autocomplete.
        /// </summary>
        [JsonPropertyName("focused")]
        public Optional<bool> Focused { get; init; }
    }
}
