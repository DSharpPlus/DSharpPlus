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
    public sealed class DiscordChannelSelectComponent : BaseDiscordSelectComponent
    {
        [JsonProperty("channel_types", NullValueHandling = NullValueHandling.Ignore)]
        public IReadOnlyList<ChannelType> ChannelTypes { get; internal set; }

        /// <summary>
        /// Enables this component.
        /// </summary>
        /// <returns>The current component.</returns>
        public DiscordChannelSelectComponent Enable()
        {
            this.Disabled = false;
            return this;
        }

        /// <summary>
        /// Disables this component.
        /// </summary>
        /// <returns>The current component.</returns>
        public DiscordChannelSelectComponent Disable()
        {
            this.Disabled = true;
            return this;
        }

        internal DiscordChannelSelectComponent()
        {
            this.Type = ComponentType.ChannelSelect;
        }

        /// <summary>
        /// Creates a new channel select component.
        /// </summary>
        /// <param name="customId">The ID of this component.</param>
        /// <param name="placeholder">Placeholder text that's shown when no options are selected.</param>
        /// <param name="channelTypes">Optional channel types to filter by.</param>
        /// <param name="disabled">Whether this component is disabled.</param>
        /// <param name="minOptions">The minimum amount of options to be selected.</param>
        /// <param name="maxOptions">The maximum amount of options to be selected, up to 25.</param>
        public DiscordChannelSelectComponent(string customId, string placeholder, IEnumerable<ChannelType>? channelTypes = null, bool disabled = false, int minOptions = 1, int maxOptions = 1) : this()
        {
            this.CustomId = customId;
            this.Placeholder = placeholder;
            this.Disabled = disabled;
            this.MinimumSelectedValues = minOptions;
            this.MaximumSelectedValues = maxOptions;
        }
    }
}
