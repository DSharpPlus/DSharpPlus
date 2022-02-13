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
using Newtonsoft.Json;

namespace DSharpPlus.Entities
{
    internal class DiscordInteractionApplicationCommandCallbackData
    {
        [JsonProperty("tts", NullValueHandling = NullValueHandling.Ignore)]
        public bool? IsTTS { get; internal set; }

        [JsonProperty("content", NullValueHandling = NullValueHandling.Ignore)]
        public string Content { get; internal set; }

        [JsonProperty("embeds", NullValueHandling = NullValueHandling.Ignore)]
        public IEnumerable<DiscordEmbed> Embeds { get; internal set; }

        [JsonProperty("allowed_mentions", NullValueHandling = NullValueHandling.Ignore)]
        public DiscordMentions Mentions { get; internal set; }

        [JsonProperty("flags", NullValueHandling = NullValueHandling.Ignore)]
        public MessageFlags? Flags { get; internal set; }

        [JsonProperty("components", NullValueHandling = NullValueHandling.Ignore)]
        public IReadOnlyCollection<DiscordActionRowComponent> Components { get; internal set; }

        [JsonProperty("choices")]
        public IReadOnlyCollection<DiscordAutoCompleteChoice> Choices { get; internal set; }
    }
}
