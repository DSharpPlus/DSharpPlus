// This file is part of the DSharpPlus project.
//
// Copyright (c) 2015 Mike Santiago
// Copyright (c) 2016-2023

 DSharpPlus Contributors
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

#pragma warning disable 0649

using System;
using Newtonsoft.Json;

namespace DSharpPlus.Lavalink.Entities
{
    internal sealed class LavalinkState
    {
        [JsonIgnore]
        public DateTimeOffset Time => Utilities.GetDateTimeOffsetFromMilliseconds(this._time);
        [JsonProperty("time")]
        private readonly long _time;

        [JsonIgnore]
        public TimeSpan Position => TimeSpan.FromMilliseconds(this._position);
        [JsonProperty("position")]
        private readonly long _position;
    }

    /// <summary>
    /// Represents current state of given player.
    /// </summary>
    public sealed class LavalinkPlayerState
    {
        /// <summary>
        /// Gets the timestamp at which this state was last updated.
        /// </summary>
        public DateTimeOffset LastUpdate { get; internal set; }

        /// <summary>
        /// Gets the current playback position.
        /// </summary>
        public TimeSpan PlaybackPosition { get; internal set; }

        /// <summary>
        /// Gets the currently-played track.
        /// </summary>
        public LavalinkTrack CurrentTrack { get; internal set; }
    }

    internal sealed class LavalinkStats
    {
        [JsonProperty("playingPlayers")]
        public int ActivePlayers { get; set; }

        [JsonProperty("players")]
        public int TotalPlayers { get; set; }

        [JsonIgnore]
        public TimeSpan Uptime => TimeSpan.FromMilliseconds(this._uptime);
        [JsonProperty("uptime")]
        private readonly long _uptime;

        [JsonProperty("cpu")]
        public CpuStats Cpu { get; set; }

        [JsonProperty("memory")]
        public MemoryStats Memory { get; set; }

        [JsonProperty("frameStats")]
        public FrameStats Frames { get; set; }

        internal sealed class CpuStats
        {
            [JsonProperty("cores")]
            public int Cores { get; set; }

            [JsonProperty("systemLoad")]
            public double SystemLoad { get; set; }

            [JsonProperty("lavalinkLoad")]
            public double LavalinkLoad { get; set; }
        }

        internal sealed class MemoryStats
        {
            [JsonProperty("reservable")]
            public long Reservable { get; set; }

            [JsonProperty("used")]
            public long Used { get; set; }

            [JsonProperty("free")]
            public long Free { get; set; }

            [JsonProperty("allocated")]
            public long Allocated { get; set; }
        }

        internal sealed class FrameStats
        {
            [JsonProperty("sent")]
            public int Sent { get; set; }

            [JsonProperty("nulled")]
            public int Nulled { get; set; }

            [JsonProperty("deficit")]
            public int Deficit { get; set; }
        }
    }

    /// <summary>
    /// Represents statistics of Lavalink resource usage.
    /// </summary>
    public sealed class LavalinkStatistics
    {
        /// <summary>
        /// Gets the number of currently-playing players.
        /// </summary>
        public int ActivePlayers { get; private set; }

        /// <summary>
        /// Gets the total number of players.
        /// </summary>
        public int TotalPlayers { get; private set; }

        /// <summary>
        /// Gets the node uptime.
        /// </summary>
        public TimeSpan Uptime { get; private set; }

        /// <summary>
        /// Gets the number of CPU cores available.
        /// </summary>
        public int CpuCoreCount { get; private set; }

        /// <summary>
        /// Gets the total % of CPU resources in use on the system.
        /// </summary>
        public double CpuSystemLoad { get; private set; }

        /// <summary>
        /// Gets the total % of CPU resources used by lavalink.
        /// </summary>
        public double CpuLavalinkLoad { get; private set; }

        /// <summary>
        /// Gets the amount of reservable RAM, in bytes.
        /// </summary>
        public long RamReservable { get; private set; }

        /// <summary>
        /// Gets the amount of used RAM, in bytes.
        /// </summary>
        public long RamUsed { get; private set; }

        /// <summary>
        /// Gets the amount of free RAM, in bytes.
        /// </summary>
        public long RamFree { get; private set; }

        /// <summary>
        /// Gets the amount of allocated RAM, in bytes.
        /// </summary>
        public long RamAllocated { get; private set; }

        /// <summary>
        /// Gets the average number of sent frames per minute.
        /// </summary>
        public int AverageSentFramesPerMinute { get; private set; }

        /// <summary>
        /// Gets the average number of frames that were sent as null per minute.
        /// </summary>
        public int AverageNulledFramesPerMinute { get; private set; }

        /// <summary>
        /// Gets the average frame deficit per minute.
        /// </summary>
        public int AverageDeficitFramesPerMinute { get; private set; }

        internal bool _updated;

        internal LavalinkStatistics()
        {
            this._updated = false;
        }

        internal void Update(LavalinkStats newStats)
        {
            if (!this._updated)
                this._updated = true;

            this.ActivePlayers = newStats.ActivePlayers;
            this.TotalPlayers = newStats.TotalPlayers;
            this.Uptime = newStats.Uptime;

            this.CpuCoreCount = newStats.Cpu.Cores;
            this.CpuSystemLoad = newStats.Cpu.SystemLoad;
            this.CpuLavalinkLoad = newStats.Cpu.LavalinkLoad;

            this.RamReservable = newStats.Memory.Reservable;
            this.RamUsed = newStats.Memory.Used;
            this.RamFree = newStats.Memory.Free;
            this.RamAllocated = newStats.Memory.Allocated;
            this.RamReservable = newStats.Memory.Reservable;

            this.AverageSentFramesPerMinute = newStats.Frames?.Sent ?? 0;
            this.AverageNulledFramesPerMinute = newStats.Frames?.Nulled ?? 0;
            this.AverageDeficitFramesPerMinute = newStats.Frames?.Deficit ?? 0;
        }
    }
}
