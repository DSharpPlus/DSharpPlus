using System.Threading.Tasks;

using DSharpPlus.EventArgs;

namespace DSharpPlus.SlashCommands;

internal class SlashCommandsEventHandler
    : IEventHandler<SessionCreatedEventArgs>,
    IEventHandler<InteractionCreatedEventArgs>,
    IEventHandler<ContextMenuInteractionCreatedEventArgs>
{
    private readonly SlashCommandsExtension extension;

    public SlashCommandsEventHandler(SlashCommandsExtension ext)
        => this.extension = ext;

    public async Task HandleEventAsync(DiscordClient sender, SessionCreatedEventArgs eventArgs)
        => await this.extension.Update(sender, eventArgs);

    public async Task HandleEventAsync(DiscordClient sender, InteractionCreatedEventArgs eventArgs)
        => await this.extension.InteractionHandler(sender, eventArgs);

    public async Task HandleEventAsync(DiscordClient sender, ContextMenuInteractionCreatedEventArgs eventArgs)
        => await this.extension.ContextMenuHandler(sender, eventArgs);
}
