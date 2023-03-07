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

using Newtonsoft.Json;

namespace DSharpPlus.Lavalink.Entities.Filters
{
    /// <summary>
    /// Mixes both channels (left and right), with a configurable factor on how much each channel affects the other. With the defaults, both channels are kept independent of each other. Setting all factors to 0.5 means both channels get the same audio.
    /// </summary>
    public class LavalinkChannelMixFilter : ILavalinkFilter
    {
        /// <summary>
        /// The left channel volume (0 to 1.0 where 0.0 is no effect and 1.0 is full effect)
        /// </summary>
        [JsonProperty("leftToLeft")]
        public float? LeftToLeft { get; set; }
        /// <summary>
        /// The left channel volume (0 to 1.0 where 0.0 is no effect and 1.0 is full effect)
        /// </summary>
        [JsonProperty("leftToRight")]
        public float? LeftToRight { get; set; }
        /// <summary>
        /// The right channel volume (0 to 1.0 where 0.0 is no effect and 1.0 is full effect)
        /// </summary>
        [JsonProperty("rightToRight")]
        public float? RightToRight { get; set; }
        /// <summary>
        /// The right channel volume (0 to 1.0 where 0.0 is no effect and 1.0 is full effect)
        /// </summary>
        [JsonProperty("rightToLeft")]
        public float? RightToLeft { get; set; }

        public LavalinkChannelMixFilter(float? leftToLeft = null, float? leftToRight = null, float? rightToRight = null, float? rightToLeft = null)
        {
            this.LeftToLeft = leftToLeft;
            this.LeftToRight = leftToRight;
            this.RightToRight = rightToRight;
            this.RightToLeft = rightToLeft;
        }
    }
}
