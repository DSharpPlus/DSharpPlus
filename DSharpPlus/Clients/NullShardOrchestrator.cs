using System;
using System.Threading.Tasks;
using DSharpPlus.Entities;

namespace DSharpPlus.Clients;

/// <summary>
/// Dummy orchestrator that does nothing. Useful for http interaction only clients.
/// </summary>
public sealed class NullShardOrchestrator : IShardOrchestrator
{
    /// <inheritdoc/>
    public bool AllShardsConnected { get; private set; }

    /// <inheritdoc/>
    public int TotalShardCount => 0;

    /// <inheritdoc/>
    public int ConnectedShardCount => 0;

    /// <inheritdoc/>
    public ValueTask BroadcastOutboundEventAsync(byte[] payload) => ValueTask.CompletedTask;

    /// <inheritdoc/>
    public TimeSpan GetConnectionLatency(ulong guildId) => TimeSpan.Zero;

    /// <inheritdoc/>
    public bool IsConnected(ulong guildId) => this.AllShardsConnected;

    /// <inheritdoc/>
    public ValueTask ReconnectAsync()
    {
        this.AllShardsConnected = true;
        return ValueTask.CompletedTask;
    }

    /// <summary>
    /// Sends an outbound event to Discord.
    /// </summary>
    public ValueTask SendOutboundEventAsync(byte[] payload, ulong _) => ValueTask.CompletedTask;

    /// <inheritdoc/>
    public ValueTask StartAsync(DiscordActivity? activity, DiscordUserStatus? status, DateTimeOffset? idleSince)
    {
        this.AllShardsConnected = true;
        return ValueTask.CompletedTask;
    }

    /// <inheritdoc/>
    public ValueTask StopAsync()
    {
        this.AllShardsConnected = false;
        return ValueTask.CompletedTask;
    }
}
