using System.Threading.Tasks;

using DSharpPlus.Net.Gateway;

namespace DSharpPlus.Clients;

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
}
