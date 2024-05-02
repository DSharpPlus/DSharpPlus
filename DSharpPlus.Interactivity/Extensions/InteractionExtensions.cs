namespace DSharpPlus.Interactivity.Extensions;

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity.Enums;
using DSharpPlus.Interactivity.EventHandling;

public static class InteractionExtensions
{
    /// <summary>
    /// Sends a paginated message in response to an interaction.
    /// <para>
    /// <b>Pass the interaction directly. Interactivity will ACK it.</b>
    /// </para>
    /// </summary>
    /// <param name="interaction">The interaction to create a response to.</param>
    /// <param name="ephemeral">Whether the response should be ephemeral.</param>
    /// <param name="user">The user to listen for button presses from.</param>
    /// <param name="pages">The pages to paginate.</param>
    /// <param name="buttons">Optional: custom buttons</param>
    /// <param name="behaviour">Pagination behaviour.</param>
    /// <param name="deletion">Deletion behaviour</param>
    /// <param name="asEditResponse">If the response as edit of previous response.</param>
    /// <param name="token">A custom cancellation token that can be cancelled at any point.</param>
    public static Task SendPaginatedResponseAsync(this DiscordInteraction interaction, bool ephemeral, DiscordUser user, IEnumerable<Page> pages, PaginationButtons buttons = null, PaginationBehaviour? behaviour = default, ButtonPaginationBehavior? deletion = default, bool asEditResponse = false, CancellationToken token = default)
        => ChannelExtensions.GetInteractivity(interaction.Channel).SendPaginatedResponseAsync(interaction, ephemeral, user, pages, buttons, behaviour, deletion, asEditResponse, default, default, token);
}
