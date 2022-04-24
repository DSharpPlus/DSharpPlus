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

namespace DSharpPlus.Core.Entities
{
    public sealed record DiscordTextInputComponent : IDiscordMessageComponent
    {
        /// <inheritdoc/>
        [JsonProperty("type", NullValueHandling = NullValueHandling.Ignore)]
        public DiscordComponentType Type { get; init; }

        /// <summary>
        /// A developer-defined identifier for the input, max 100 characters.
        /// </summary>
        [JsonProperty("custom_id", NullValueHandling = NullValueHandling.Ignore)]
        public string CustomId { get; init; } = null!;

        /// <summary>
        /// The <see cref="DiscordTextInputStyle"/> Text Input Style</see>.
        /// </summary>
        [JsonProperty("style", NullValueHandling = NullValueHandling.Ignore)]
        public DiscordTextInputStyle Style { get; init; }

        /// <summary>
        /// The label for this component, max 45 characters.
        /// </summary>
        [JsonProperty("label", NullValueHandling = NullValueHandling.Ignore)]
        public string Label { get; init; } = null!;

        /// <summary>
        /// The minimum input length for a text input, min 0, max 4000.
        /// </summary>
        [JsonProperty("min_length", NullValueHandling = NullValueHandling.Ignore)]
        public Optional<int> MinLength { get; init; }

        /// <summary>
        /// The maximum input length for a text input, min 1, max 4000.
        /// </summary>
        [JsonProperty("max_length", NullValueHandling = NullValueHandling.Ignore)]
        public Optional<int> MaxLength { get; init; }

        /// <summary>
        /// Whether this component is required to be filled, default true.
        /// </summary>
        [JsonProperty("required", NullValueHandling = NullValueHandling.Ignore)]
        public Optional<bool> Required { get; init; }

        /// <summary>
        /// A pre-filled value for this component, max 4000 characters.
        /// </summary>
        [JsonProperty("value", NullValueHandling = NullValueHandling.Ignore)]
        public Optional<string> Value { get; init; }

        /// <summary>
        /// A custom placeholder text if the input is empty, max 100 characters.
        /// </summary>
        [JsonProperty("placeholder", NullValueHandling = NullValueHandling.Ignore)]
        public Optional<string> Placeholder { get; init; }
    }
}
