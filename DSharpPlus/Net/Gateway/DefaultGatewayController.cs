using System.Threading.Tasks;

using DSharpPlus.Net.Gateway;

namespace DSharpPlus.Clients;

// intentionally doesn't do anything. if users want to customize this, their logic is likely to have so little in common
// with anything we could provide.
internal class DefaultGatewayController : IGatewayController
{
    /// <inheritdoc/>
    public Task HeartbeatedAsync(IGatewayClient client) => Task.CompletedTask;

    /// <inheritdoc/>
    public Task ReconnectFailedAsync(IGatewayClient client) => Task.CompletedTask;

    /// <inheritdoc/>
    public Task ReconnectRequestedAsync(IGatewayClient client) => Task.CompletedTask;

    /// <inheritdoc/>
    public Task SessionInvalidatedAsync(IGatewayClient client) => Task.CompletedTask;

    /// <inheritdoc/>
    public Task ZombiedAsync(IGatewayClient client) => Task.CompletedTask;
}
