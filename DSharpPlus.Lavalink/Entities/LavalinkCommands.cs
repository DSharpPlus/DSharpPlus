namespace DSharpPlus.Lavalink.Entities;
using System;
using System.Collections.Generic;
using Newtonsoft.Json;

[Obsolete("DSharpPlus.Lavalink is deprecated for removal.", true)]
internal sealed class LavalinkConfigureResume : LavalinkPayload
{
    [JsonProperty("key")]
    public string Key { get; }

    [JsonProperty("timeout")]
    public int Timeout { get; }

    public LavalinkConfigureResume(string key, int timeout)
        : base("configureResuming")
    {
        Key = key;
        Timeout = timeout;
    }
}

[Obsolete("DSharpPlus.Lavalink is deprecated for removal.", true)]
internal sealed class LavalinkDestroy : LavalinkPayload
{
    public LavalinkDestroy(LavalinkGuildConnection lvl)
        : base("destroy", lvl.GuildIdString)
    { }
}

[Obsolete("DSharpPlus.Lavalink is deprecated for removal.", true)]
internal sealed class LavalinkPlay : LavalinkPayload
{
    [JsonProperty("track")]
    public string Track { get; }

    public LavalinkPlay(LavalinkGuildConnection lvl, LavalinkTrack track)
        : base("play", lvl.GuildIdString) => Track = track.TrackString;
}

[Obsolete("DSharpPlus.Lavalink is deprecated for removal.", true)]
internal sealed class LavalinkPlayPartial : LavalinkPayload
{
    [JsonProperty("track")]
    public string Track { get; }

    [JsonProperty("startTime")]
    public long StartTime { get; }

    [JsonProperty("endTime")]
    public long StopTime { get; }

    public LavalinkPlayPartial(LavalinkGuildConnection lvl, LavalinkTrack track, TimeSpan start, TimeSpan stop)
        : base("play", lvl.GuildIdString)
    {
        Track = track.TrackString;
        StartTime = (long)start.TotalMilliseconds;
        StopTime = (long)stop.TotalMilliseconds;
    }
}

[Obsolete("DSharpPlus.Lavalink is deprecated for removal.", true)]
internal sealed class LavalinkPause : LavalinkPayload
{
    [JsonProperty("pause")]
    public bool Pause { get; }

    public LavalinkPause(LavalinkGuildConnection lvl, bool pause)
        : base("pause", lvl.GuildIdString) => Pause = pause;
}

[Obsolete("DSharpPlus.Lavalink is deprecated for removal.", true)]
internal sealed class LavalinkStop : LavalinkPayload
{
    public LavalinkStop(LavalinkGuildConnection lvl)
        : base("stop", lvl.GuildIdString)
    { }
}

[Obsolete("DSharpPlus.Lavalink is deprecated for removal.", true)]
internal sealed class LavalinkSeek : LavalinkPayload
{
    [JsonProperty("position")]
    public long Position { get; }

    public LavalinkSeek(LavalinkGuildConnection lvl, TimeSpan position)
        : base("seek", lvl.GuildIdString) => Position = (long)position.TotalMilliseconds;
}

[Obsolete("DSharpPlus.Lavalink is deprecated for removal.", true)]
internal sealed class LavalinkVolume : LavalinkPayload
{
    [JsonProperty("volume")]
    public int Volume { get; }

    public LavalinkVolume(LavalinkGuildConnection lvl, int volume)
        : base("volume", lvl.GuildIdString) => Volume = volume;
}

[Obsolete("DSharpPlus.Lavalink is deprecated for removal.", true)]
internal sealed class LavalinkEqualizer : LavalinkPayload
{
    [JsonProperty("bands")]
    public IEnumerable<LavalinkBandAdjustment> Bands { get; }

    public LavalinkEqualizer(LavalinkGuildConnection lvl, IEnumerable<LavalinkBandAdjustment> bands)
        : base("equalizer", lvl.GuildIdString) => Bands = bands;
}
