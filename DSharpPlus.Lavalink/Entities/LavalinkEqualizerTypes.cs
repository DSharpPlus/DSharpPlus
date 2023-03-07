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

using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace DSharpPlus.Lavalink.Entities
{
    /// <summary>
    /// Represents Lavalink equalizer band adjustment. This is used to alter the sound output by using Lavalink's equalizer.
    /// </summary>
    public struct LavalinkBandAdjustment
    {
        /// <summary>
        /// Gets the ID of the band to adjust.
        /// </summary>
        [JsonProperty("band")]
        public int BandId { get; }

        /// <summary>
        /// Gets the gain of the specified band.
        /// </summary>
        [JsonProperty("gain")]
        public float Gain { get; }

        /// <summary>
        /// Creates a new band adjustment with specified parameters.
        /// </summary>
        /// <param name="bandId">Which band to adjust. Must be in 0-14 range.</param>
        /// <param name="gain">By how much to adjust the band. Must be greater than or equal to -0.25 (muted), and less than or equal to +1.0. +0.25 means the band is doubled.</param>
        public LavalinkBandAdjustment(int bandId, float gain)
        {
            if (bandId < 0 || bandId > 14)
                throw new ArgumentOutOfRangeException(nameof(bandId), "Band ID cannot be lower than 0 or greater than 14.");

            if (gain < -0.25 || gain > 1.0)
                throw new ArgumentOutOfRangeException(nameof(gain), "Gain cannot be lower than -0.25 or greater than 1.0.");

            this.BandId = bandId;
            this.Gain = gain;
        }
    }

    internal class LavalinkBandAdjustmentComparer : IEqualityComparer<LavalinkBandAdjustment>
    {
        public bool Equals(LavalinkBandAdjustment x, LavalinkBandAdjustment y)
            => x.BandId == y.BandId;

        public int GetHashCode(LavalinkBandAdjustment obj)
            => obj.BandId;
    }
}
