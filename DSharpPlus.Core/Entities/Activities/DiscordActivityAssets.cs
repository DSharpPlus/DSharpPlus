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

namespace DSharpPlus.Core.Entities
{
    public sealed record DiscordActivityAssets
    {
        /// <summary>
        /// Activity asset images are arbitrary strings which usually contain snowflake IDs or prefixed image IDs. Treat data within this field carefully, as it is user-specifiable and not sanitized.
        /// To use an external image via media proxy, specify the URL as the field's value when sending. You will only receive the <c>mp:</c> prefix via the gateway.
        /// </summary>
        [JsonPropertyName("large_image")]
        public Optional<string> LargeImage { get; init; }

        /// <summary>
        /// Text displayed when hovering over the large image of the activity.
        /// </summary>
        [JsonPropertyName("large_text")]
        public Optional<string> LargeText { get; init; }

        /// <summary>
        /// Activity asset images are arbitrary strings which usually contain snowflake IDs or prefixed image IDs. Treat data within this field carefully, as it is user-specifiable and not sanitized.
        /// To use an external image via media proxy, specify the URL as the field's value when sending. You will only receive the <c>mp:</c> prefix via the gateway.
        /// </summary>
        [JsonPropertyName("small_image")]
        public Optional<string> SmallImage { get; init; }

        /// <summary>
        /// Text displayed when hovering over the small image of the activity.
        /// </summary>
        [JsonPropertyName("small_text")]
        public Optional<string> SmallText { get; init; }
    }
}
