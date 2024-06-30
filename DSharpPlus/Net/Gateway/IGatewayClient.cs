using System;
using System.Threading.Tasks;

using DSharpPlus.Entities;
using DSharpPlus.Net.Abstractions;

namespace DSharpPlus.Net.Gateway;

/// <summary>
/// Represents a gateway client handling all system events.
/// </summary>
public interface IGatewayClient
{
    public ValueTask ConnectAsync
    (
        string url,
        DiscordActivity? activity = null,
        DiscordUserStatus? status = null,
        DateTimeOffset? idleSince = null,
        ShardInfo? shardInfo = null
    );

    public ValueTask DisconnectAsync();

    public ValueTask ReconnectAsync();

    public ValueTask WriteAsync(byte[] payload);

    public bool IsConnected { get; }

    public TimeSpan Ping { get; }
}
