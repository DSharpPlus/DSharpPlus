using System.Reflection;
using System.Threading.Tasks;

using DSharpPlus.AsyncEvents;
using DSharpPlus.EventArgs;

namespace DSharpPlus.Interactivity;

internal sealed class InteractivityEventHandler 
    : IEventHandler<DiscordEventArgs>,
    IEventHandler<ComponentInteractionCreatedEventArgs>,
    IEventHandler<ModalSubmittedEventArgs>,
    IEventHandler<MessageReactionAddedEventArgs>,
    IEventHandler<MessageReactionRemovedEventArgs>,
    IEventHandler<MessageReactionsClearedEventArgs>
{
    private readonly InteractivityExtension extension;

    public InteractivityEventHandler(InteractivityExtension ext)
        => this.extension = ext;

    public async Task HandleEventAsync(DiscordClient sender, DiscordEventArgs eventArgs)
    {
        if (this.extension.eventDistributor.TryGetValue(eventArgs.GetType(), out AsyncEvent? value))
        {
            MethodInfo invoke = typeof(AsyncEvent<,>)
                .MakeGenericType(typeof(DiscordClient), eventArgs.GetType())
                .GetMethod("InvokeAsync")!;

            await (Task)invoke.Invoke(value, [sender, eventArgs])!;
        }
    }

    public async Task HandleEventAsync(DiscordClient sender, ComponentInteractionCreatedEventArgs eventArgs)
    {
        await Task.WhenAll
        (
            this.extension.ComponentEventWaiter.HandleAsync(sender, eventArgs),
            this.extension.compPaginator.HandleAsync(sender, eventArgs)
        );
    }

    public async Task HandleEventAsync(DiscordClient sender, MessageReactionAddedEventArgs eventArgs)
    {
        await Task.WhenAll
        (
            this.extension.Paginator.HandleReactionAdd(sender, eventArgs),
            this.extension.Poller.HandleReactionAdd(sender, eventArgs)
        );
    }

    public async Task HandleEventAsync(DiscordClient sender, MessageReactionRemovedEventArgs eventArgs)
    {
        await Task.WhenAll
        (
            this.extension.Paginator.HandleReactionRemove(sender, eventArgs),
            this.extension.Poller.HandleReactionRemove(sender, eventArgs)
        );
    }

    public async Task HandleEventAsync(DiscordClient sender, MessageReactionsClearedEventArgs eventArgs)
    {
        await Task.WhenAll
        (
            this.extension.Paginator.HandleReactionClear(sender, eventArgs),
            this.extension.Poller.HandleReactionClear(sender, eventArgs)
        );
    }

    public async Task HandleEventAsync(DiscordClient sender, ModalSubmittedEventArgs eventArgs)
        => await this.extension.ModalEventWaiter.Handle(sender, eventArgs);
}
