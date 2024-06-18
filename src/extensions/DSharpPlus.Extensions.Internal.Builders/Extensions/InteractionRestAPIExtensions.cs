// This Source Code form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

#pragma warning disable IDE0046 // we have a lot of early returns here that we don't want to become ternaries.

using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

using DSharpPlus.Entities;
using DSharpPlus.Extensions.Internal.Builders.Implementations;
using DSharpPlus.Extensions.Internal.Builders.Interactions;
using DSharpPlus.Extensions.Internal.Builders.Messages;
using DSharpPlus.Internal.Abstractions.Models;
using DSharpPlus.Internal.Abstractions.Rest;
using DSharpPlus.Internal.Abstractions.Rest.API;
using DSharpPlus.Results;

namespace DSharpPlus.Extensions.Internal.Builders.Extensions;

/// <summary>
/// Contains extension methods on <see cref="IInteractionRestAPI"/> to enable using builders.
/// </summary>
public static class InteractionRestAPIExtensions
{
    /// <summary>
    /// Responds to the specified interaction using a modal.
    /// </summary>
    /// <param name="underlying">The underlying interaction API.</param>
    /// <param name="interactionId">The snowflake identifier of the interaction.</param>
    /// <param name="interactionToken">The interaction token received with the interaction.</param>
    /// <param name="modal">The modal builder to respond with.</param>
    /// <param name="info">Additional instructions regarding this request.</param>
    /// <param name="ct">A cancellation token for this operation.</param>
    [AsyncMethodBuilder(typeof(PoolingAsyncValueTaskMethodBuilder<>))]
    public static async ValueTask<Result> RespondWithModalAsync
    (
        this IInteractionRestAPI underlying,
        Snowflake interactionId,
        string interactionToken,
        ModalBuilder modal,
        RequestInfo info = default,
        CancellationToken ct = default
    )
    {
        Result result = modal.Validate();

        if (!result.IsSuccess)
        {
            return result;
        }

        return await underlying.CreateInteractionResponseAsync
        (
            interactionId,
            interactionToken,
            modal.Build(),
            info,
            ct
        );
    }

    /// <summary>
    /// Responds to the specified interaction using an embed.
    /// </summary>
    /// <param name="underlying">The underlying interaction API.</param>
    /// <param name="interactionId">The snowflake identifier of the interaction.</param>
    /// <param name="interactionToken">The interaction token received with the interaction.</param>
    /// <param name="embed">The embed builder to respond with.</param>
    /// <param name="ephemeral">Specifies whether the response to this request should be ephemeral.</param>
    /// <param name="info">Additional instructions regarding this request.</param>
    /// <param name="ct">A cancellation token for this operation.</param>
    [AsyncMethodBuilder(typeof(PoolingAsyncValueTaskMethodBuilder<>))]
    public static async ValueTask<Result> RespondWithEmbedAsync
    (
        this IInteractionRestAPI underlying,
        Snowflake interactionId,
        string interactionToken,
        EmbedBuilder embed,
        bool ephemeral,
        RequestInfo info = default,
        CancellationToken ct = default
    )
    {
        Result result = embed.Validate();

        if (!result.IsSuccess)
        {
            return result;
        }

        BuiltInteractionResponse response = new()
        {
            Type = DiscordInteractionCallbackType.ChannelMessageWithSource,
            Data = new
            (
                new BuiltMessageCallbackData()
                {
                    Embeds = new Optional<IReadOnlyList<IEmbed>>([embed.Build()]),
                    Flags = ephemeral ? new(DiscordMessageFlags.Ephemeral) : Optional<DiscordMessageFlags>.None
                }
            )
        };

        return await underlying.CreateInteractionResponseAsync
        (
            interactionId,
            interactionToken,
            response,
            info,
            ct
        );
    }

    /// <summary>
    /// Creates a followup response containing an embed. If this is the first followup to a deferred interaction
    /// response as created by  <see cref="IInteractionRestAPI.CreateInteractionResponseAsync"/>,
    /// ephemerality of this message will be dictated by the <see cref="IInteractionResponse"/> specified originally,
    /// and <paramref name="ephemeral"/> will be ignored.
    /// </summary>
    /// <param name="underlying">The underlying interaction API.</param>
    /// <param name="applicationId">The snowflake identifier of your application.</param>
    /// <param name="interactionToken">The interaction token received with the interaction.</param>
    /// <param name="embed">The embed builder to respond with.</param>
    /// <param name="ephemeral">Specifies whether this response should be ephemeral.</param>
    /// <param name="info">Additional instructions regarding this request.</param>
    /// <param name="ct">A cancellation token for this operation.</param>
    public static async ValueTask<Result<IMessage>> FollowupWithEmbedAsync
    (
        this IInteractionRestAPI underlying,
        Snowflake applicationId,
        string interactionToken,
        EmbedBuilder embed,
        bool ephemeral,
        RequestInfo info = default,
        CancellationToken ct = default
    )
    {
        Result result = embed.Validate();

        if (!result.IsSuccess)
        {
            return Result<IMessage>.FromError(result.Error);
        }

        return await underlying.CreateFollowupMessageAsync
        (
            applicationId,
            interactionToken,
            new BuiltCreateFollowupMessagePayload
            {
                Embeds = new([embed.Build()]),
                Flags = ephemeral ? new(DiscordMessageFlags.Ephemeral) : Optional<DiscordMessageFlags>.None
            },
            info,
            ct
        );
    }

    /// <summary>
    /// Edits the original response with the specified embed. This will leave all fields but embeds intact.
    /// Ephemerality is dictated by the original response.
    /// </summary>
    /// <param name="underlying">The underlying interaction API.</param>
    /// <param name="applicationId">The snowflake identifier of your application.</param>
    /// <param name="interactionToken">The interaction token received with the interaction.</param>
    /// <param name="embed">The embed builder to respond with.</param>
    /// <param name="info">Additional instructions regarding this request.</param>
    /// <param name="ct">A cancellation token for this operation.</param>
    public static async ValueTask<Result<IMessage>> ModifyResponseWithEmbedAsync
    (
        this IInteractionRestAPI underlying,
        Snowflake applicationId,
        string interactionToken,
        EmbedBuilder embed,
        RequestInfo info = default,
        CancellationToken ct = default
    )
    {
        Result result = embed.Validate();

        if (!result.IsSuccess)
        {
            return Result<IMessage>.FromError(result.Error);
        }

        return await underlying.EditInteractionResponseAsync
        (
            applicationId,
            interactionToken,
            new BuiltEditInteractionResponsePayload
            {
                Embeds = new([embed.Build()])
            },
            info,
            ct
        );
    }

    /// <summary>
    /// Edits the original response with the specified embed. This will leave all fields but embeds intact.
    /// Ephemerality is dictated by the original response.
    /// </summary>
    /// <param name="underlying">The underlying interaction API.</param>
    /// <param name="applicationId">The snowflake identifier of your application.</param>
    /// <param name="interactionToken">The interaction token received with the interaction.</param>
    /// <param name="messageId">The snowflake identifier of the followup message.</param>
    /// <param name="embed">The embed builder to respond with.</param>
    /// <param name="info">Additional instructions regarding this request.</param>
    /// <param name="ct">A cancellation token for this operation.</param>
    public static async ValueTask<Result<IMessage>> ModifyFollowupWithEmbedAsync
    (
        this IInteractionRestAPI underlying,
        Snowflake applicationId,
        string interactionToken,
        Snowflake messageId,
        EmbedBuilder embed,
        RequestInfo info = default,
        CancellationToken ct = default
    )
    {
        Result result = embed.Validate();

        if (!result.IsSuccess)
        {
            return Result<IMessage>.FromError(result.Error);
        }

        return await underlying.EditFollowupMessageAsync
        (
            applicationId,
            interactionToken,
            messageId,
            new BuiltEditFollowupMessagePayload
            {
                Embeds = new([embed.Build()])
            },
            info,
            ct
        );
    }

    /// <inheritdoc cref="RespondWithEmbedAsync(IInteractionRestAPI, Snowflake, string, EmbedBuilder, bool, RequestInfo, CancellationToken)"/>
    [StackTraceHidden]
    [DebuggerStepThrough]
    public static ValueTask<Result> RespondWithEmbedAsync
    (
        this IInteractionRestAPI underlying,
        Snowflake interactionId,
        string interactionToken,
        EmbedBuilder embed,
        RequestInfo info = default,
        CancellationToken ct = default
    )
    {
        return underlying.RespondWithEmbedAsync
        (
            interactionId,
            interactionToken,
            embed,
            false,
            info,
            ct
        );
    }

    /// <inheritdoc cref="FollowupWithEmbedAsync(IInteractionRestAPI, Snowflake, string, EmbedBuilder, bool, RequestInfo, CancellationToken)"/>
    [StackTraceHidden]
    [DebuggerStepThrough]
    public static ValueTask<Result<IMessage>> FollowupWithEmbedAsync
    (
        this IInteractionRestAPI underlying,
        Snowflake applicationId,
        string interactionToken,
        EmbedBuilder embed,
        RequestInfo info = default,
        CancellationToken ct = default
    )
    {
        return underlying.FollowupWithEmbedAsync
        (
            applicationId,
            interactionToken,
            embed,
            false,
            info,
            ct
        );
    }
}
