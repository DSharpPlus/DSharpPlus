using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

namespace DSharpPlus.Hosting;

public class DiscordClientService : IHostedService
{
    private readonly DiscordClient client;
    private readonly DiscordClientStartupOptions options;

    public DiscordClientService(DiscordClient client, IOptions<DiscordClientStartupOptions> options)
    {
        this.client = client;
        this.options = options.Value;
    }

    public async Task StartAsync(CancellationToken cancellationToken) 
        => await this.client.ConnectAsync(this.options.Activity, this.options.Status, this.options.IdleSince);

    public async Task StopAsync(CancellationToken cancellationToken)
        => await this.client.DisconnectAsync();
}
