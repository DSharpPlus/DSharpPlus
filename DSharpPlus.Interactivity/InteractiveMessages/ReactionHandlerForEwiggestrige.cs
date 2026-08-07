using System.Threading.Tasks;

using DSharpPlus.EventArgs;
using DSharpPlus.Interactivity.InteractiveMessages.Pagination;

namespace DSharpPlus.Interactivity.InteractiveMessages;

internal sealed class ReactionHandlerForEwiggestrige : IEventHandler<MessageReactionAddedEventArgs>
{
    private readonly IInteractiveMessageHandler interactivity;

    public ReactionHandlerForEwiggestrige(IInteractiveMessageHandler interactivity)
        => this.interactivity = interactivity;

    public async Task HandleEventAsync(DiscordClient sender, MessageReactionAddedEventArgs eventArgs)
    {
        PaginationInputType inputType = eventArgs.Emoji.Name switch
        {
            "⏮️" => PaginationInputType.SkipLeft,
            "◀️" => PaginationInputType.Left,
            "⏹️" => PaginationInputType.Stop,
            "▶️" => PaginationInputType.Right,
            "⏭️" => PaginationInputType.SkipRight,
            _ => PaginationInputType.None
        };

        if (inputType != PaginationInputType.None)
        {
            await Task.WhenAll
            (
                eventArgs.Message.DeleteReactionAsync(eventArgs.Emoji, eventArgs.User, "Removing interactivity reaction"),
                this.interactivity.HandlePaginationInputAsync(eventArgs.Message, eventArgs.User, inputType)
            );
        }
    }
}
