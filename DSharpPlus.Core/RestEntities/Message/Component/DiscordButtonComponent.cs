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
    public sealed record DiscordButtonComponent : IDiscordMessageComponent
    {
        /// <inheritdoc/>
        [JsonProperty("type", NullValueHandling = NullValueHandling.Ignore)]
        public DiscordComponentType Type { get; init; }

        /// <summary>
        /// One of the <see cref="DiscordButtonStyle">button styles</see>.
        /// </summary>
        /// <remarks>
        /// Non-link buttons must have a <see cref="CustomId"/>, and cannot have a url. Link buttons must have a url, and cannot have a <see cref="CustomId"/>. Link buttons do not send an <see cref="DiscordInteraction"/> to your app when clicked.
        /// </remarks>
        [JsonProperty("style", NullValueHandling = NullValueHandling.Ignore)]
        public DiscordButtonStyle Style { get; init; }

        /// <summary>
        /// Text that appears on the button, max 80 characters.
        /// </summary>
        [JsonProperty("label", NullValueHandling = NullValueHandling.Ignore)]
        public Optional<string> Label { get; init; }

        /// <summary>
        /// The name, id, and animated properties.
        /// </summary>
        [JsonProperty("emoji", NullValueHandling = NullValueHandling.Ignore)]
        public Optional<DiscordEmoji> Emoji { get; init; }

        /// <summary>
        /// A developer-defined identifier for the button, max 100 characters.
        /// </summary>
        /// <remarks>
        /// <see cref="CustomId"/> must be unique per component; multiple buttons on the same message must not share the same <see cref="CustomId"/>. This field is a string of max 100 characters, and can be used flexibly to maintain state or pass through other important data.
        /// </remarks>
        [JsonProperty("custom_id", NullValueHandling = NullValueHandling.Ignore)]
        public Optional<string> CustomId { get; init; }

        /// <summary>
        /// A url for <see cref="DiscordButtonStyle.Link"/> buttons.
        /// </summary>
        [JsonProperty("url", NullValueHandling = NullValueHandling.Ignore)]
        public Optional<string> Url { get; init; }

        /// <summary>
        /// Whether the button is disabled (default <see langword="false"/>).
        /// </summary>
        [JsonProperty("disabled", NullValueHandling = NullValueHandling.Ignore)]
        public Optional<bool> Disabled { get; init; }
    }
}
