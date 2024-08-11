using System.Threading.Tasks;

using DSharpPlus.Commands.Processors;
using DSharpPlus.Commands.Processors.MessageCommands;
using DSharpPlus.Commands.Processors.SlashCommands;
using DSharpPlus.Commands.Processors.TextCommands;
using DSharpPlus.Commands.Processors.UserCommands;
using DSharpPlus.EventArgs;

namespace DSharpPlus.Commands;

// this is a temporary measure until we can land proper IoC support
internal sealed class ProcessorInvokingHandlers
    : IEventHandler<ContextMenuInteractionCreatedEventArgs>,
    IEventHandler<InteractionCreatedEventArgs>,
    IEventHandler<MessageCreatedEventArgs>
{
    private readonly CommandsExtension extension;

    public ProcessorInvokingHandlers(CommandsExtension extension)
        => this.extension = extension;
    
    // user and message context menu commands
    public async Task HandleEventAsync(DiscordClient sender, ContextMenuInteractionCreatedEventArgs eventArgs)
    {
        if (this.extension.Processors.TryGetValue(typeof(UserCommandProcessor), out ICommandProcessor? userProcessor))
        {
            await ((UserCommandProcessor)userProcessor).ExecuteInteractionAsync(sender, eventArgs);
        }

        if (this.extension.Processors.TryGetValue(typeof(MessageCommandProcessor), out ICommandProcessor? msgProcessor))
        {
            await ((MessageCommandProcessor)msgProcessor).ExecuteInteractionAsync(sender, eventArgs);
        }
    }
    
    // slash commands
    public async Task HandleEventAsync(DiscordClient sender, InteractionCreatedEventArgs eventArgs)
    {
        if (this.extension.Processors.TryGetValue(typeof(SlashCommandProcessor), out ICommandProcessor? processor))
        {
            await ((SlashCommandProcessor)processor).ExecuteInteractionAsync(sender, eventArgs);
        }
    }
    
    // text commands
    public async Task HandleEventAsync(DiscordClient sender, MessageCreatedEventArgs eventArgs)
    {
        if (this.extension.Processors.TryGetValue(typeof(TextCommandProcessor), out ICommandProcessor? processor))
        {
            await ((TextCommandProcessor)processor).ExecuteTextCommandAsync(sender, eventArgs);
        }
    }
}
