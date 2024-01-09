// This Source Code form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

using DSharpPlus.Internal.Abstractions.Models;
using DSharpPlus.Internal.Abstractions.Rest.Payloads;

using Remora.Results;

namespace DSharpPlus.Internal.Abstractions.Rest.API;

/// <summary>
/// Provides access to emoji-related rest API calls.
/// </summary>
public interface IEmojiRestAPI
{
    /// <summary>
    /// Fetches a list of emojis for the given guild.
    /// </summary>
    /// <param name="guildId">The snowflake identifier of the guild in question.</param>
    /// <param name="info">Additional instructions regarding this request.</param>
    /// <param name="ct">A cancellation token for this operation.</param>
    public ValueTask<Result<IReadOnlyList<IEmoji>>> ListGuildEmojisAsync
    (
        Snowflake guildId,
        RequestInfo info = default,
        CancellationToken ct = default
    );

    /// <summary>
    /// Returns the specified emoji.
    /// </summary>
    /// <param name="guildId">The snowflake identifier of the guild owning the emoji.</param>
    /// <param name="emojiId">The snowflake identifier of the emoji in question.</param>
    /// <param name="info">Additional instructions regarding this request.</param>
    /// <param name="ct">A cancellation token for this operation.</param>
    public ValueTask<Result<IEmoji>> GetGuildEmojiAsync
    (
        Snowflake guildId,
        Snowflake emojiId,
        RequestInfo info = default,
        CancellationToken ct = default
    );

    /// <summary>
    /// Creates a new guild emoji in the specified guild.
    /// </summary>
    /// <param name="guildId">The snowflake identifier of the guild in question.</param>
    /// <param name="payload">The payload containing information on the emoji.</param>
    /// <param name="reason">An optional audit log reason.</param>
    /// <param name="info">Additional instructions regarding this request.</param>
    /// <param name="ct">A cancellation token for this operation.</param>
    /// <returns>The newly created emoji.</returns>
    public ValueTask<Result<IEmoji>> CreateGuildEmojiAsync
    (
        Snowflake guildId,
        ICreateGuildEmojiPayload payload,
        string? reason = null,
        RequestInfo info = default,
        CancellationToken ct = default
    );

    /// <summary>
    /// Modifies the given emoji.
    /// </summary>
    /// <param name="guildId">The snowflake identifier of the guild owning the emoji.</param>
    /// <param name="emojiId">The snowflake identifier of the emoji in question.</param>
    /// <param name="payload">A payload detailing the edit to make to this emoji.</param>
    /// <param name="reason">An optional audit log reason.</param>
    /// <param name="info">Additional instructions regarding this request.</param>
    /// <param name="ct">A cancellation token for this operation.</param>
    /// <returns>The newly updated emoji.</returns>
    public ValueTask<Result<IEmoji>> ModifyGuildEmojiAsync
    (
        Snowflake guildId,
        Snowflake emojiId,
        IModifyGuildEmojiPayload payload,
        string? reason = null,
        RequestInfo info = default,
        CancellationToken ct = default
    );

    /// <summary>
    /// Deletes the given emoji.
    /// </summary>
    /// <param name="guildId">The snowflake identifier of the guild owning this emoji.</param>
    /// <param name="emojiId">The snowflake identifier of the emoji to be deleted.</param>
    /// <param name="reason">An optional audit log reason.</param>
    /// <param name="info">Additional instructions regarding this request.</param>
    /// <param name="ct">A cancellation token for this operation.</param>
    /// <returns>Whether the deletion was successful.</returns>
    public ValueTask<Result> DeleteGuildEmojiAsync
    (
        Snowflake guildId,
        Snowflake emojiId,
        string? reason = null,
        RequestInfo info = default,
        CancellationToken ct = default
    );
}
