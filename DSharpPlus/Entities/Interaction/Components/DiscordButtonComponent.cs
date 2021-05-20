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
using Newtonsoft.Json;

namespace DSharpPlus.Entities
{
    public sealed class DiscordButtonComponent : DiscordComponent
    {
        [JsonProperty("style", NullValueHandling = NullValueHandling.Ignore)]
        public int Style { get; set; }

        [JsonProperty("url", NullValueHandling = NullValueHandling.Ignore)]
        public string Url { get; set; }

        [JsonProperty("label", NullValueHandling = NullValueHandling.Ignore)]
        public string Label { get; set; }

        [JsonProperty("disabled", NullValueHandling = NullValueHandling.Ignore)]
        public bool Disabled { get; set; }

        [JsonProperty("emoji", NullValueHandling = NullValueHandling.Ignore)]
        public DiscordComponentEmoji Emoji { get; set; }

        public DiscordButtonComponent(int style = default, string url = null, string label = null, string customId = null, bool disabled = default, DiscordComponentEmoji emoji = null)
        {
            this.Style = style;
            this.Url = url;
            this.Label = label;
            this.CustomId = customId;
            this.Disabled = disabled;
            this.Emoji = emoji;
            this.Type = ComponentType.Button;
        }
    }
}
