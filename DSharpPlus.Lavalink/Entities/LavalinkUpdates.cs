namespace DSharpPlus.Lavalink.Entities;
#pragma warning disable 0649

using System;
using Newtonsoft.Json;

internal sealed class LavalinkState
{
    [JsonIgnore]
    public DateTimeOffset Time => Utilities.GetDateTimeOffsetFromMilliseconds(_time);
    [JsonProperty("time")]
    private readonly long _time;

    [JsonIgnore]
    public TimeSpan Position => TimeSpan.FromMilliseconds(_position);
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
    public TimeSpan Uptime => TimeSpan.FromMilliseconds(_uptime);
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

    internal LavalinkStatistics() => _updated = false;

    internal void Update(LavalinkStats newStats)
    {
        if (!_updated)
        {
            _updated = true;
        }

        ActivePlayers = newStats.ActivePlayers;
        TotalPlayers = newStats.TotalPlayers;
        Uptime = newStats.Uptime;

        CpuCoreCount = newStats.Cpu.Cores;
        CpuSystemLoad = newStats.Cpu.SystemLoad;
        CpuLavalinkLoad = newStats.Cpu.LavalinkLoad;

        RamReservable = newStats.Memory.Reservable;
        RamUsed = newStats.Memory.Used;
        RamFree = newStats.Memory.Free;
        RamAllocated = newStats.Memory.Allocated;
        RamReservable = newStats.Memory.Reservable;

        AverageSentFramesPerMinute = newStats.Frames?.Sent ?? 0;
        AverageNulledFramesPerMinute = newStats.Frames?.Nulled ?? 0;
        AverageDeficitFramesPerMinute = newStats.Frames?.Deficit ?? 0;
    }
}
