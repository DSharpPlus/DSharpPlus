using System.Threading.Tasks;

namespace DSharpPlus.Net.Gateway;

/// <summary>
/// Provides a low-level interface for controlling individual gateway clients and their connections.
/// </summary>
public interface IGatewayController
{
    /// <summary>
    /// Called when the gateway connection zombies.
    /// </summary>
    /// <param name="client">The gateway client whose connection zombied.</param>
    public Task ZombiedAsync(IGatewayClient client);

    /// <summary>
    /// Called when the gateway heartbeated correctly and got an ACK from Discord
    /// </summary>
    /// <param name="client">The gateway client who recieved the heartbeat ACK.</param>
    public Task HeartbeatedAsync(IGatewayClient client);

    /// <summary>
    /// Called when Discord requests a reconnect. This does not imply that DSharpPlus' reconnection attempt failed.
    /// </summary>
    /// <param name="client">The gateway client reconnection was requested from.</param>
    public Task ReconnectRequestedAsync(IGatewayClient client);

    /// <summary>
    /// Called when a reconnecting attempt definitively failed and DSharpPlus can no longer reconnect on its own.
    /// </summary>
    /// <param name="client">The gateway client reconnection was requested from.</param>
    public Task ReconnectFailedAsync(IGatewayClient client);

    /// <summary>
    /// Called when a session was invalidated and DSharpPlus failed to resume or reconnect.
    /// </summary>
    /// <param name="client">The gateway client reconnection was requested from.</param>
    public Task SessionInvalidatedAsync(IGatewayClient client);
}
