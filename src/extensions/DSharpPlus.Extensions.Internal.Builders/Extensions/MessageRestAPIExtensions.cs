// This Source Code form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

#pragma warning disable IDE0046 // we have a lot of early returns here that we don't want to become ternaries.

using System.Threading;
using System.Threading.Tasks;

using DSharpPlus.Extensions.Internal.Builders.Implementations;
using DSharpPlus.Extensions.Internal.Builders.Messages;
using DSharpPlus.Internal.Abstractions.Models;
using DSharpPlus.Internal.Abstractions.Rest;
using DSharpPlus.Internal.Abstractions.Rest.API;
using DSharpPlus.Results;

namespace DSharpPlus.Extensions.Internal.Builders.Extensions;

/// <summary>
/// Contains extension methods on <see cref="IMessageRestAPI"/> to enable using builders.
/// </summary>
public static class MessageRestAPIExtensions
{
    /// <summary>
    /// Creates a new message comprising the specified embed in a channel.
    /// </summary>
    /// <param name="underlying">The underlying message API.</param>
    /// <param name="channelId">The snowflake identifier of the message's target channel.</param>
    /// <param name="embed">The embed this message is to be comprised of.</param>
    /// <param name="info">Additional instructions regarding this request.</param>
    /// <param name="ct">A cancellation token for this operation.</param>
    /// <returns>The newly created message object.</returns>
    public static async ValueTask<Result<IMessage>> SendEmbedAsync
    (
        this IMessageRestAPI underlying,
        Snowflake channelId,
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

        return await underlying.CreateMessageAsync
        (
            channelId,
            new BuiltCreateMessagePayload
            {
                Embeds = new([embed.Build()])
            },
            info,
            ct
        );
    }

    /// <summary>
    /// Modifies the specified message to comprise the specified embed. This will only update embeds for this message.
    /// </summary>
    /// <param name="underlying">The underlying message API.</param>
    /// <param name="channelId">The snowflake identifier of the channel this message was sent in.</param>
    /// <param name="messageId">The snowflake identifier of the message to modify with the embed.</param>
    /// <param name="embed">The embed this message is to be comprised of.</param>
    /// <param name="info">Additional instructions regarding this request.</param>
    /// <param name="ct">A cancellation token for this operation.</param>
    /// <returns>The newly modified message object.</returns>
    public static async ValueTask<Result<IMessage>> ModifyEmbedAsync
    (
        this IMessageRestAPI underlying,
        Snowflake channelId,
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

        return await underlying.EditMessageAsync
        (
            channelId,
            messageId,
            new BuiltEditMessagePayload
            {
                Embeds = new([embed.Build()])
            },
            info,
            ct
        );
    }
}
