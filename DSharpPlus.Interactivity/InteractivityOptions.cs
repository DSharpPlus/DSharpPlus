using System;

using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using DSharpPlus.Interactivity.Components;
using DSharpPlus.Interactivity.InteractiveMessages;
using DSharpPlus.Interactivity.InteractiveMessages.Components;
using DSharpPlus.Interactivity.InteractiveMessages.Pagination;

namespace DSharpPlus.Interactivity;

/// <summary>
/// Configuration class for your Interactivity extension
/// </summary>
public sealed class InteractivityOptions
{
    /// <summary>
    /// Sets the default interactivity action timeout. Defaults to 1 minute. This should not be increased excessively: interactivity is designed to work
    /// in relatively short timespans.
    /// </summary>
    public TimeSpan Timeout { internal get; set; } = TimeSpan.FromMinutes(1);

    /// <summary>
    /// Buttons to use for pagination.
    /// </summary>
    public PaginationButtons PaginationButtons { internal get; set; } = new();

    /// <summary>
    /// How to handle buttons after an interactive message expires.
    /// </summary>
    public TimeoutBehaviour InteractiveTimeoutBehaviour { internal get; set; } = TimeoutBehaviour.Disable;

    /// <summary>
    /// How to handle pagination running over the end of the provided page list. Defaults to WrapAround.
    /// </summary>
    public PaginationWrapAroundBehaviour PaginationWrapAroundBehaviour { internal get; set; } = PaginationWrapAroundBehaviour.WrapAround;

    /// <summary>
    /// How to handle invalid component interactions. Defaults to <see cref="ComponentInteractionResponseBehavior.Ignore"/>
    /// </summary>
    public ComponentInteractionResponseBehavior ResponseBehavior { internal get; set; } = ComponentInteractionResponseBehavior.Ignore;

    /// <summary>
    /// Provides a string factory to generate a response when processing invalid interactions. This is ignored if <see cref="ResponseBehavior"/> is not 
    /// <see cref="ComponentInteractionResponseBehavior.Respond"/>
    /// </summary>
    /// <remarks>
    /// An invalid interaction in this case is considered an interaction on a component if the user-defined condition does not match, such as the interacting
    /// user not being specified as the user to wait for.
    /// </remarks>
    public Func<InteractionCreatedEventArgs, IServiceProvider, DiscordInteractionResponseBuilder> ResponseMessageFactory { internal get; set; } 
        = (_, _) => new DiscordInteractionResponseBuilder().WithContent("The maze is not meant for you.").AsEphemeral();

    /// <summary>
    /// The template to use for pagination when no <see cref="InteractiveMessageBuilder"/> is explicitly specified. 
    /// </summary>
    public InteractiveMessageBuilder DefaultPaginatedMessageTemplate { internal get; set; }
        = new InteractiveMessageBuilder().EnableV2Components().AddContainerComponent(new(
        [
            new PaginatedTextDisplayComponent(), 
            new DiscordSeparatorComponent(), 
            new PaginationButtonRowComponent()
        ],
        color: DiscordColor.Blurple
    ));

    /// <summary>
    /// Internal mirror of <see cref="LegacyFeatures.UseReactionInteractivity"/> to be accessible in DI contexts.
    /// </summary>
    internal bool UseReactionsForPagination { get; set; }
}
