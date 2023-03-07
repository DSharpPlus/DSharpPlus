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
    /// Distortion effect. It can generate some pretty unique audio effects.
    /// </summary>
    public class LavalinkDistortionFilter : ILavalinkFilter
    {
        /// <summary>
        /// The distortion level (0 to 1.0 where 0.0 is no effect and 1.0 is full effect)
        /// </summary>
        [JsonProperty("sinOffset")]
        public float? SinOffset { get; set; }
        /// <summary>
        /// The distortion level (0 to 1.0 where 0.0 is no effect and 1.0 is full effect)
        /// </summary>
        [JsonProperty("sinScale")]
        public float? SinScale { get; set; }
        /// <summary>
        /// The distortion level (0 to 1.0 where 0.0 is no effect and 1.0 is full effect)
        /// </summary>
        [JsonProperty("cosOffset")]
        public float? CosOffset { get; set; }
        /// <summary>
        /// The distortion level (0 to 1.0 where 0.0 is no effect and 1.0 is full effect)
        /// </summary>
        [JsonProperty("cosScale")]
        public float? CosScale { get; set; }
        /// <summary>
        /// The distortion level (0 to 1.0 where 0.0 is no effect and 1.0 is full effect)
        /// </summary>
        [JsonProperty("tanOffset")]
        public float? TanOffset { get; set; }
        /// <summary>
        /// The distortion level (0 to 1.0 where 0.0 is no effect and 1.0 is full effect)
        /// </summary>
        [JsonProperty("tanScale")]
        public float? TanScale { get; set; }
        /// <summary>
        /// The distortion level (0 to 1.0 where 0.0 is no effect and 1.0 is full effect)
        /// </summary>
        [JsonProperty("offset")]
        public float? Offset { get; set; }
        /// <summary>
        /// The distortion level (0 to 1.0 where 0.0 is no effect and 1.0 is full effect)
        /// </summary>
        [JsonProperty("scale")]
        public float? Scale { get; set; }

        public LavalinkDistortionFilter(float? sinOffset = null, float? sinScale = null, float? cosOffset = null, float? cosScale = null, float? tanOffset = null, float? tanScale = null, float? offset = null, float? scale = null)
        {
            this.SinOffset = sinOffset;
            this.SinScale = sinScale;
            this.CosOffset = cosOffset;
            this.CosScale = cosScale;
            this.TanOffset = tanOffset;
            this.TanScale = tanScale;
            this.Offset = offset;
            this.Scale = scale;
        }
    }
}
