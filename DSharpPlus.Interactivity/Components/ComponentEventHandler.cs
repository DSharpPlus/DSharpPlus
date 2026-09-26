using System.Threading.Tasks;

using DSharpPlus.EventArgs;

namespace DSharpPlus.Interactivity.Components;

internal sealed class ComponentEventHandler : IEventHandler<ComponentInteractionCreatedEventArgs>
{
    private readonly IComponentWaiter componentWaiter;

    public ComponentEventHandler(IComponentWaiter waiter)
        => this.componentWaiter = waiter;

    public async Task HandleEventAsync(DiscordClient sender, ComponentInteractionCreatedEventArgs eventArgs) 
        => await this.componentWaiter.HandleComponentInteractionAsync(sender, eventArgs);
}
