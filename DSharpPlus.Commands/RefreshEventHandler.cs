using System.Threading.Tasks;

using DSharpPlus.EventArgs;

namespace DSharpPlus.Commands;

internal sealed class RefreshEventHandler : IEventHandler<SessionCreatedEventArgs>
{
    private readonly CommandsExtension extension;

    public RefreshEventHandler(CommandsExtension extension)
        => this.extension = extension;

    public async Task HandleEventAsync(DiscordClient sender, SessionCreatedEventArgs eventArgs)
        => await this.extension.RefreshAsync();
}