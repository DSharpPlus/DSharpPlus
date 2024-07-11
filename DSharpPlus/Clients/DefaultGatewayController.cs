using System.Threading.Tasks;

using DSharpPlus.Net.Gateway;

namespace DSharpPlus.Clients;

// intentionally doesn't do anything. if users want to customize this, their logic is likely to have so little in common
// with anything we could provide.
internal class DefaultGatewayController : IGatewayController
{
    public ValueTask ZombiedAsync(IGatewayClient client) => ValueTask.CompletedTask;
    public Task HeartbeatedAsync(IGatewayClient client) => Task.CompletedTask;
}
