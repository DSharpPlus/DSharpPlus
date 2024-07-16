using System;
using System.Threading.Tasks;

using DSharpPlus.Entities;

namespace DSharpPlus.Clients;

/// <summary>
/// Represents a mechanism for orchestrating one or more shards in one or more processes.
/// </summary>
public interface IShardOrchestrator
{
    /// <summary>
    /// Starts all shards associated with this orchestrator.
    /// </summary>
    public ValueTask StartAsync(DiscordActivity? activity, DiscordUserStatus? status, DateTimeOffset? idleSince);

    /// <summary>
    /// Stops all shards associated with this orchestrator.
    /// </summary>
    public ValueTask StopAsync();

    /// <summary>
    /// Reconnects all shards associated with this orchestrator.
    /// </summary>
    public ValueTask ReconnectAsync();

    /// <summary>
    /// Sends an outbound event to Discord from the specified guild. Pass 0 to send a guild-independent outbound event
    /// to shard 0.
    /// </summary>
    public ValueTask SendOutboundEventAsync(byte[] payload, ulong guildId);

    /// <summary>
    /// Sends an outbound event to Discord on all shards.
    /// </summary>
    public ValueTask BroadcastOutboundEventAsync(byte[] payload);

    /// <summary>
    /// Indicates whether all shards are connected.
    /// </summary>
    public bool AllShardsConnected { get; }

    /// <summary>
    /// Indicates whether the bot's connection to the given guild is functional.
    /// </summary>
    public bool IsConnected(ulong guildId);

    /// <summary>
    /// Gets the connection latency to a specific guild, otherwise known as ping.
    /// </summary>
    public TimeSpan GetConnectionLatency(ulong guildId);
}
