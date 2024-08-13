using System.Threading.Tasks;

namespace DSharpPlus.Net.Gateway;

/// <summary>
/// A gateway controller implementation that automatically attempts to reconnect.
/// </summary>
public sealed class ReconnectingGatewayController : IGatewayController
{
    /// <inheritdoc/>
    public Task HeartbeatedAsync(IGatewayClient client) => Task.CompletedTask;

    /// <inheritdoc/>
    public Task ReconnectFailedAsync(IGatewayClient client) => Task.CompletedTask;

    /// <inheritdoc/>
    public Task ReconnectRequestedAsync(IGatewayClient client) => Task.CompletedTask;

    /// <inheritdoc/>
    public async Task SessionInvalidatedAsync(IGatewayClient client) => await client.ReconnectAsync();

    /// <inheritdoc/>
    public async Task ZombiedAsync(IGatewayClient client) => await client.ReconnectAsync();
}
