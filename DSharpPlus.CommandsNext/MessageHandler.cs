using System.Threading.Tasks;

using DSharpPlus.EventArgs;

namespace DSharpPlus.CommandsNext;

internal sealed class MessageHandler : IEventHandler<MessageCreatedEventArgs>
{
    private readonly CommandsNextExtension extension;

    public MessageHandler(CommandsNextExtension ext)
        => this.extension = ext;

    public async Task HandleEventAsync(DiscordClient sender, MessageCreatedEventArgs eventArgs) 
        => await this.extension.HandleCommandsAsync(sender, eventArgs);
}
