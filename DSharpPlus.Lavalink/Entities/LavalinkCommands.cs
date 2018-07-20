using System;
using Newtonsoft.Json;

namespace DSharpPlus.Lavalink.Entities
{
    internal sealed class LavalinkDestroy : LavalinkPayload
    {
        public LavalinkDestroy(LavalinkGuildConnection lvl)
            : base("destroy", lvl.GuildIdString)
        { }
    }

    internal sealed class LavalinkPlay : LavalinkPayload
    {
        [JsonProperty("track")]
        public string Track { get; }
        
        public LavalinkPlay(LavalinkGuildConnection lvl, LavalinkTrack track)
            : base("play", lvl.GuildIdString)
        {
            this.Track = track.TrackString;
        }
    }

    internal sealed class LavalinkPlayPartial : LavalinkPayload
    {
        [JsonProperty("track")]
        public string Track { get; }

        [JsonProperty("startTime")]
        public long StartTime { get; }

        [JsonProperty("stopTime")]
        public long StopTime { get; }

        public LavalinkPlayPartial(LavalinkGuildConnection lvl, LavalinkTrack track, TimeSpan start, TimeSpan stop)
            : base("play", lvl.GuildIdString)
        {
            this.Track = track.TrackString;
            this.StartTime = (long)start.TotalMilliseconds;
            this.StopTime = (long)stop.TotalMilliseconds;
        }
    }

    internal sealed class LavalinkPause : LavalinkPayload
    {
        [JsonProperty("pause")]
        public bool Pause { get; }

        public LavalinkPause(LavalinkGuildConnection lvl, bool pause)
            : base("pause", lvl.GuildIdString)
        {
            this.Pause = pause;
        }
    }

    internal sealed class LavalinkStop : LavalinkPayload
    {
        public LavalinkStop(LavalinkGuildConnection lvl)
            : base("stop", lvl.GuildIdString)
        { }
    }

    internal sealed class LavalinkSeek : LavalinkPayload
    {
        [JsonProperty("position")]
        public long Position { get; }

        public LavalinkSeek(LavalinkGuildConnection lvl, TimeSpan position)
            : base("seek", lvl.GuildIdString)
        {
            this.Position = (long)position.TotalMilliseconds;
        }
    }

    internal sealed class LavalinkVolume : LavalinkPayload
    {
        [JsonProperty("volume")]
        public int Volume { get; }

        public LavalinkVolume(LavalinkGuildConnection lvl, int volume)
            : base("volume", lvl.GuildIdString)
        {
            this.Volume = volume;
        }
    }
}
