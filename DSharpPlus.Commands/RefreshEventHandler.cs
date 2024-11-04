using System.Threading.Tasks;

using DSharpPlus.EventArgs;

namespace DSharpPlus.Commands;

internal sealed class RefreshEventHandler : IEventHandler<ClientStartedEventArgs>
{
    private readonly CommandsExtension extension;

    public RefreshEventHandler(CommandsExtension extension)
        => this.extension = extension;

    public async Task HandleEventAsync(DiscordClient sender, ClientStartedEventArgs eventArgs)
        => await this.extension.RefreshAsync();
}
