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

using DSharpPlus.Core.Enums;
using Newtonsoft.Json;

namespace DSharpPlus.Core.RestEntities
{
    public sealed record DiscordSelectMenuComponent : IDiscordMessageComponent
    {
        /// <inheritdoc/>
        [JsonProperty("type", NullValueHandling = NullValueHandling.Ignore)]
        public DiscordComponentType Type { get; init; }

        /// <summary>
        /// A developer-defined identifier for the select menu, max 100 characters.
        /// </summary>
        [JsonProperty("custom_id", NullValueHandling = NullValueHandling.Ignore)]
        public string CustomId { get; init; } = null!;

        /// <summary>
        /// The choices in the select, max 25.
        /// </summary>
        [JsonProperty("options", NullValueHandling = NullValueHandling.Ignore)]
        public DiscordSelectMenuOptionComponent[] Options { get; init; } = null!;

        /// <summary>
        /// The custom placeholder text if nothing is selected, max 150 characters.
        /// </summary>
        [JsonProperty("placeholder", NullValueHandling = NullValueHandling.Ignore)]
        public Optional<string> Placeholder { get; init; }

        /// <summary>
        /// The minimum number of items that must be chosen; default 1, min 0, max 25.
        /// </summary>
        [JsonProperty("min_values", NullValueHandling = NullValueHandling.Ignore)]
        public Optional<int> MinValues { get; init; }

        /// <summary>
        /// The maximum number of items that can be chosen; default 1, max 25.
        /// </summary>
        [JsonProperty("max_values", NullValueHandling = NullValueHandling.Ignore)]
        public Optional<int> MaxValues { get; init; }

        /// <summary>
        /// Whether to disable the select, default false.
        /// </summary>
        [JsonProperty("disabled", NullValueHandling = NullValueHandling.Ignore)]
        public Optional<bool> Disabled { get; init; }
    }
}
