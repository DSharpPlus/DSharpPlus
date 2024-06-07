using System;
using System.Threading.Tasks;

using DSharpPlus.Entities;

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
        DateTimeOffset? idleSince = null
    );

    public ValueTask DisconnectAsync();

    public ValueTask WriteAsync(byte[] payload);
}
