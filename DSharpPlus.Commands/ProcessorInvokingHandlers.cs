using System.Threading.Tasks;
using DSharpPlus.Commands.Processors.MessageCommands;
using DSharpPlus.Commands.Processors.SlashCommands;
using DSharpPlus.Commands.Processors.TextCommands;
using DSharpPlus.Commands.Processors.UserCommands;
using DSharpPlus.EventArgs;

namespace DSharpPlus.Commands;

// this is a temporary measure until we can land proper IoC support
internal sealed class ProcessorInvokingHandlers :
    IEventHandler<ContextMenuInteractionCreatedEventArgs>,
    IEventHandler<InteractionCreatedEventArgs>,
    IEventHandler<MessageCreatedEventArgs>,
    IEventHandler<MessageUpdatedEventArgs>
{
    private readonly CommandsExtension extension;

    public ProcessorInvokingHandlers(CommandsExtension extension) => this.extension = extension;

    // user and message context menu commands
    public async Task HandleEventAsync(DiscordClient sender, ContextMenuInteractionCreatedEventArgs eventArgs)
    {
        if (this.extension.TryGetProcessor(out UserCommandProcessor? userProcessor))
        {
            await userProcessor.ExecuteInteractionAsync(sender, eventArgs);
        }

        if (this.extension.TryGetProcessor(out MessageCommandProcessor? messageProcessor))
        {
            await messageProcessor.ExecuteInteractionAsync(sender, eventArgs);
        }
    }

    // slash commands
    public async Task HandleEventAsync(DiscordClient sender, InteractionCreatedEventArgs eventArgs)
    {
        if (this.extension.TryGetProcessor(out SlashCommandProcessor? slashProcessor))
        {
            await slashProcessor.ExecuteInteractionAsync(sender, eventArgs);
        }
    }

    public async Task HandleEventAsync(DiscordClient sender, MessageCreatedEventArgs eventArgs)
    {
        if (this.extension.TryGetProcessor(out TextCommandProcessor? processor))
        {
            await processor.ExecuteTextCommandAsync(sender, eventArgs);
        }
    }

    public async Task HandleEventAsync(DiscordClient sender, MessageUpdatedEventArgs eventArgs)
    {
        if (this.extension.TryGetProcessor(out TextCommandProcessor? processor))
        {
            await processor.ExecuteTextCommandAsync(sender, eventArgs);
        }
    }
}
