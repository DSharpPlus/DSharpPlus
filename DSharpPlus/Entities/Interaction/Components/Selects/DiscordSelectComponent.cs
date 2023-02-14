// This file is part of the DSharpPlus project.
//
// Copyright (c) 2015 Mike Santiago
// Copyright (c) 2016-2023 DSharpPlus Contributors
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
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

namespace DSharpPlus.Entities
{
    /// <summary>
    /// A select menu with multiple options to choose from.
    /// </summary>
    public sealed class DiscordSelectComponent : BaseDiscordSelectComponent
    {
        /// <summary>
        /// The options to pick from on this component.
        /// </summary>
        [JsonProperty("options", NullValueHandling = NullValueHandling.Ignore)]
        public IReadOnlyList<DiscordSelectComponentOption> Options { get; internal set; } = Array.Empty<DiscordSelectComponentOption>();

        /// <summary>
        /// Enables this component.
        /// </summary>
        /// <returns>The current component.</returns>
        public DiscordSelectComponent Enable()
        {
            this.Disabled = false;
            return this;
        }

        /// <summary>
        /// Disables this component.
        /// </summary>
        /// <returns>The current component.</returns>
        public DiscordSelectComponent Disable()
        {
            this.Disabled = true;
            return this;
        }

        internal DiscordSelectComponent()
        {
            this.Type = ComponentType.StringSelect;
        }

        public DiscordSelectComponent(string customId, string placeholder, IEnumerable<DiscordSelectComponentOption> options, bool disabled = false, int minOptions = 1, int maxOptions = 1) : this()
        {
            this.CustomId = customId;
            this.Options = options.ToArray();
            this.Placeholder = placeholder;
            this.Disabled = disabled;
            this.MinimumSelectedValues = minOptions;
            this.MaximumSelectedValues = maxOptions;
        }
    }
}
