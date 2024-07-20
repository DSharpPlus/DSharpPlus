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
    public ValueTask ZombiedAsync(IGatewayClient client);

    /// <summary>
    /// Called when the gateway heartbeated correctly and got an ACK from Discord
    /// </summary>
    /// <param name="client">The gateway client who recieved the heartbeat ACK.</param>
    public Task HeartbeatedAsync(IGatewayClient client);
}
