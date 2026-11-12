using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using DSharpPlus.Interactivity.InteractiveMessages.Pagination;

using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;

namespace DSharpPlus.Interactivity.InteractiveMessages;

/// <inheritdoc cref="IInteractiveMessageHandler"/>
public sealed class DefaultInteractiveMessageHandler : IInteractiveMessageHandler
{
    private readonly InteractivityOptions options;
    private readonly IMemoryCache cache;
    private readonly IServiceProvider services;

    public DefaultInteractiveMessageHandler
    (
        IOptions<InteractivityOptions> options,
        IMemoryCache cache,
        IServiceProvider services
    )
    {
        this.options = options.Value;
        this.cache = cache;
        this.services = services;
    }

    /// <inheritdoc/>
    public async Task HandlePaginationInputAsync(DiscordMessage message, DiscordUser user, PaginationInputType input)
    {
        if (!this.cache.TryGetValue($"dsharpplus.interactivity.message-state.{message.Id}", out InteractiveMessage? interactive))
        {
            return;
        }

        if (!interactive!.Condition(user))
        {
            return;
        }

        InteractiveMessage newState = interactive with
        {
            State = interactive.State with
            {
                Page = input switch
                {
                    PaginationInputType.SkipLeft => 0,
                    PaginationInputType.Left => interactive.State.Page != 0 ? interactive.State.Page - 1 : interactive.PageCount,
                    PaginationInputType.Right => interactive.State.Page != interactive.PageCount ? interactive.State.Page + 1 : 0,
                    PaginationInputType.SkipRight => interactive.PageCount,
                    _ => interactive.State.Page
                },
                IsDisabled = input == PaginationInputType.Stop
            }
        };

        this.cache.Set($"dsharpplus.interactivity.message-state.{message.Id}", newState);

        DiscordWebhookBuilder newPage = interactive.MessageFactory(newState.State);
        DiscordMessageBuilder convertedPage = new(newPage);

        await message.ModifyAsync(convertedPage);
    }

    /// <inheritdoc/>
    public async Task HandlePaginationInputAsync(ComponentInteractionCreatedEventArgs interaction, PaginationInputType input)
    {
        if (!this.cache.TryGetValue($"dsharpplus.interactivity.message-state.{interaction.Message.Id}", out InteractiveMessage? interactive))
        {
            return;
        }

        if (!interactive!.Condition(interaction.User))
        {
            await interaction.Interaction.CreateResponseAsync
            (
                DiscordInteractionResponseType.ChannelMessageWithSource,
                this.options.ResponseMessageFactory(interaction, this.services)
            );
        }

        InteractiveMessage newState = interactive with
        {
            State = interactive.State with
            {
                Page = input switch
                {
                    PaginationInputType.SkipLeft => 0,
                    PaginationInputType.Left => interactive.State.Page != 0 ? interactive.State.Page - 1 : interactive.PageCount,
                    PaginationInputType.Right => interactive.State.Page != interactive.PageCount ? interactive.State.Page + 1 : 0,
                    PaginationInputType.SkipRight => interactive.PageCount,
                    _ => interactive.State.Page
                },
                IsDisabled = input == PaginationInputType.Stop
            }
        };

        this.cache.Set($"dsharpplus.interactivity.message-state.{interaction.Message.Id}", newState);

        DiscordWebhookBuilder newPage = interactive.MessageFactory(newState.State);
        DiscordInteractionResponseBuilder convertedPage = new(newPage);

        await interaction.Interaction.CreateResponseAsync(DiscordInteractionResponseType.UpdateMessage, convertedPage);
    }

    /// <inheritdoc/>
    public async Task HandleSelectPageAsync(ComponentInteractionCreatedEventArgs interaction)
    {
        if (!this.cache.TryGetValue($"dsharpplus.interactivity.message-state.{interaction.Message.Id}", out InteractiveMessage? interactive))
        {
            return;
        }

        if (!interactive!.Condition(interaction.User))
        {
            await interaction.Interaction.CreateResponseAsync
            (
                DiscordInteractionResponseType.ChannelMessageWithSource,
                this.options.ResponseMessageFactory(interaction, this.services)
            );
        }

        InteractiveMessage newState = interactive with
        {
            State = interactive.State with
            {
                Page = int.Parse(interaction.Values[0])
            }
        };

        this.cache.Set($"dsharpplus.interactivity.message-state.{interaction.Message.Id}", newState);

        DiscordWebhookBuilder newPage = interactive.MessageFactory(newState.State);
        DiscordInteractionResponseBuilder convertedPage = new(newPage);

        await interaction.Interaction.CreateResponseAsync(DiscordInteractionResponseType.UpdateMessage, convertedPage);
    }

    /// <inheritdoc/>
    public async Task<DiscordMessage> SendInteractiveMessageAsync
    (
        DiscordChannel channel,
        Func<InteractiveMessageState, DiscordWebhookBuilder> interactivity,
        Func<DiscordUser, bool> condition,
        int pageCount,
        TimeSpan? timeoutOverride = null
    )
    {
        InteractiveMessageState state = new();

        DiscordWebhookBuilder firstPage = interactivity(state);
        DiscordMessageBuilder convertedFirstPage = new(firstPage);

        DiscordMessage message = await channel.SendMessageAsync(convertedFirstPage);

        TimeSpan timeout = timeoutOverride ?? this.options.Timeout;
        TaskCompletionSource<InteractiveMessageState> tcs = new();

        InteractiveMessage interactive = new(tcs, interactivity, state, condition, message.Id, pageCount);

        this.cache.CreateEntry($"dsharpplus.interactivity.message-state.{message.Id}")
            .SetAbsoluteExpiration(timeout)
            .SetValue(interactive)
            .RegisterPostEvictionCallback((key, value, reason, state) =>
            {
                if (reason == EvictionReason.Expired)
                {
                    InteractiveMessage concreteValue = (InteractiveMessage)value;
                    concreteValue.Completion.SetResult(concreteValue.State);
                }
            })
            .Dispose();

        _ = tcs.Task.ContinueWith(async (parentTask) =>
        {
            switch (this.options.InteractiveTimeoutBehaviour)
            {
                case TimeoutBehaviour.Ignore:
                    break;

                case TimeoutBehaviour.Disable:

                    InteractiveMessageState finalState = parentTask.Result with { IsDisabled = true };

                    DiscordWebhookBuilder lastPage = interactivity(finalState);
                    DiscordMessageBuilder convertedLastPage = new(lastPage);

                    await message.ModifyAsync(convertedLastPage);

                    break;

                case TimeoutBehaviour.DeleteMessage:

                    await message.DeleteAsync();

                    break;
            }
        });

        return message;
    }

    /// <inheritdoc/>
    public async Task<DiscordMessage> SendInteractiveMessageAsync
    (
        DiscordInteraction interaction,
        Func<InteractiveMessageState, DiscordWebhookBuilder> interactivity,
        Func<DiscordUser, bool> condition,
        int pageCount,
        bool ephemeral = false,
        TimeSpan? timeoutOverride = null
    )
    {
        InteractiveMessageState state = new();

        DiscordWebhookBuilder firstPage = interactivity(state);

        DiscordMessage message;
        bool wasOriginalResponse;

        if (interaction.ResponseState is DiscordInteractionResponseState.Unacknowledged or DiscordInteractionResponseState.Deferred)
        {
            DiscordInteractionResponseBuilder convertedFirstPage = new(firstPage);
            convertedFirstPage.AsEphemeral(ephemeral);

            if (interaction.ResponseState == DiscordInteractionResponseState.Unacknowledged)
            {
                await interaction.CreateResponseAsync(DiscordInteractionResponseType.ChannelMessageWithSource, convertedFirstPage);
            }
            else
            {
                await interaction.EditOriginalResponseAsync(firstPage);
            }

            message = await interaction.GetOriginalResponseAsync();
            wasOriginalResponse = true;
        }
        else
        {
            DiscordFollowupMessageBuilder convertedFirstPage = new(firstPage);
            convertedFirstPage.AsEphemeral(ephemeral);

            message = await interaction.CreateFollowupMessageAsync(convertedFirstPage);
            wasOriginalResponse = false;
        }

        TimeSpan timeout = timeoutOverride ?? this.options.Timeout;
        TaskCompletionSource<InteractiveMessageState> tcs = new();

        InteractiveMessage interactive = new(tcs, interactivity, state, condition, message.Id, pageCount);

        this.cache.CreateEntry($"dsharpplus.interactivity.message-state.{message.Id}")
            .SetAbsoluteExpiration(timeout)
            .SetValue(interactive)
            .RegisterPostEvictionCallback((key, value, reason, state) =>
            {
                if (reason == EvictionReason.Expired)
                {
                    InteractiveMessage concreteValue = (InteractiveMessage)value;
                    concreteValue.Completion.SetResult(concreteValue.State);
                }
            })
            .Dispose();

        _ = tcs.Task.ContinueWith(async (parentTask) =>
        {
            switch (this.options.InteractiveTimeoutBehaviour)
            {
                case TimeoutBehaviour.Ignore:
                    break;

                case TimeoutBehaviour.Disable:

                    InteractiveMessageState finalState = parentTask.Result with { IsDisabled = true };

                    DiscordWebhookBuilder lastPage = interactivity(finalState);

                    if (wasOriginalResponse)
                    {
                        await interaction.EditOriginalResponseAsync(lastPage);
                    }
                    else
                    {
                        await interaction.EditFollowupMessageAsync(message.Id, lastPage);
                    }

                    break;

                case TimeoutBehaviour.DeleteMessage:

                    await message.DeleteAsync();

                    break;
            }
        });

        return message;
    }

    // these just forward to their above friends, they exist only to support legacy messages

    /// <inheritdoc/>
    public async Task<DiscordMessage> SendPaginatedMessageAsync
    (
        DiscordChannel channel,
        IReadOnlyList<Page> pages,
        Func<DiscordUser, bool> condition,
        TimeSpan? timeoutOverride = null
    )
    {
        return await SendInteractiveMessageAsync
        (
            channel,
            state => this.options.DefaultPaginatedMessageTemplate.Build(this.services, pages, state),
            condition,
            pages.Count,
            timeoutOverride
        );
    }

    /// <inheritdoc/>
    public async Task<DiscordMessage> SendPaginatedMessageAsync
    (
        DiscordInteraction interaction,
        IReadOnlyList<Page> pages,
        Func<DiscordUser, bool> condition,
        bool ephemeral = false, 
        TimeSpan? timeoutOverride = null
    )
    {
        return await SendInteractiveMessageAsync
        (
            interaction,
            state => this.options.DefaultPaginatedMessageTemplate.Build(this.services, pages, state),
            condition,
            pages.Count,
            ephemeral,
            timeoutOverride
        );
    }

    private record InteractiveMessage
    (
        TaskCompletionSource<InteractiveMessageState> Completion,
        Func<InteractiveMessageState, DiscordWebhookBuilder> MessageFactory,
        InteractiveMessageState State,
        Func<DiscordUser, bool> Condition,
        ulong MessageId,
        int PageCount
    );
}
