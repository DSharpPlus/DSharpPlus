using System.Threading.Tasks;

using DSharpPlus.EventArgs;
using DSharpPlus.Interactivity.InteractiveMessages.Pagination;

namespace DSharpPlus.Interactivity.InteractiveMessages;

internal sealed class InteractiveEventHandler : IEventHandler<ComponentInteractionCreatedEventArgs>
{
    private readonly IInteractiveMessageHandler interactivity;

    public InteractiveEventHandler(IInteractiveMessageHandler interactivity)
        => this.interactivity = interactivity;

    public async Task HandleEventAsync(DiscordClient sender, ComponentInteractionCreatedEventArgs eventArgs)
    {
        PaginationInputType inputType = eventArgs.Id switch
        {
            "pagination-skip-left" => PaginationInputType.SkipLeft,
            "pagination-left" => PaginationInputType.Left,
            "pagination-right" => PaginationInputType.Right,
            "pagination-skip-right" => PaginationInputType.SkipRight,
            "pagination-stop" => PaginationInputType.Stop,
            _ => PaginationInputType.None
        };

        if (inputType != PaginationInputType.None)
        {
            await this.interactivity.HandlePaginationInputAsync(eventArgs, inputType);
        }

        if (eventArgs.Id == "pagination-page-select" && eventArgs.Values is not null and not [])
        {
            await this.interactivity.HandleSelectPageAsync(eventArgs);
        }
    }
}
