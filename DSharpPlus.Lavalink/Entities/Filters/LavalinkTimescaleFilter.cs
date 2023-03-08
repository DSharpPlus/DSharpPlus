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
    /// Changes the speed, pitch, and rate. All default to 1.0.
    /// </summary>
    public class LavalinkTimescaleFilter : ILavalinkFilter
    {
        /// <summary>
        /// Playback speed (0.5 to 2.0 where 1.0 is normal speed)
        /// </summary>
        [JsonProperty("speed")]
        public float? Speed { get; set; }
        /// <summary>
        /// The pitch (0.5 to 2.0 where 1.0 is normal pitch)
        /// </summary>
        [JsonProperty("pitch")]
        public float? Pitch { get; set; }
        /// <summary>
        /// The rate (0.5 to 2.0 where 1.0 is normal rate)
        /// </summary>
        [JsonProperty("rate")]
        public float? Rate { get; set; }

        public LavalinkTimescaleFilter(float speed = 1, float pitch = 1, float rate = 1)
        {
            this.Speed = speed;
            this.Pitch = pitch;
            this.Rate = rate;
        }

        public void Reset() => this.Speed = this.Pitch = this.Rate = 1.0f;
    }
}
