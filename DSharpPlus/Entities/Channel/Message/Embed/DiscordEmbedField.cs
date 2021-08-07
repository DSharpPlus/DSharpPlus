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
    /// <summary>
    /// Represents a field inside a discord embed.
    /// </summary>
    public sealed class DiscordEmbedField
    {
        /// <summary>
        /// Gets the number of characters in the field.
        /// </summary>
        [JsonIgnore]
        public int CharCount { get; private set; }

        /// <summary>
        /// Gets the name of the field.
        /// </summary>
        [JsonProperty("name", NullValueHandling = NullValueHandling.Ignore)]
        public string Name
        {
            get => _name;
            set
            {
                this._name = value;
                this.CharCount += value == null ? 0 : value.Length;
            }
        }
        private string _name;

        /// <summary>
        /// Gets the value of the field.
        /// </summary>
        [JsonProperty("value", NullValueHandling = NullValueHandling.Ignore)]
        public string Value 
        {
            get => _value;
            set
            {
                this._value = value;
                this.CharCount += value == null ? 0 : value.Length;
            }
        }
        private string _value;

        /// <summary>
        /// Gets whether or not this field should display inline.
        /// </summary>
        [JsonProperty("inline", NullValueHandling = NullValueHandling.Ignore)]
        public bool Inline { get; set; }

        internal DiscordEmbedField() { }
    }
}
