// This Source Code form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

using Remora.Results;

using DSharpPlus.Core.Abstractions.Models;
using DSharpPlus.Core.Abstractions.Rest.Responses;
using DSharpPlus.Core.Abstractions.Rest.Payloads;

namespace DSharpPlus.Core.Abstractions.Rest.API;

/// <summary>
/// Provides access to sticker-related API calls.
/// </summary>
public interface IStickerRestAPI
{
    /// <summary>
    /// Fetches a sticker by its identifier.
    /// </summary>
    /// <param name="stickerId">The snowflake identifier of the sticker in question.</param>
    /// <param name="ct">Cancellation token for this request.</param>
    public ValueTask<Result<ISticker>> GetStickerAsync
    (
        Snowflake stickerId,
        CancellationToken ct = default
    );

    /// <summary>
    /// Returns a list of available sticker packs.
    /// </summary>
    /// <param name="ct">Cancellation token for this request.</param>
    public ValueTask<Result<ListStickerPacksResponse>> ListStickerPacksAsync
    (
        CancellationToken ct = default
    );

    /// <summary>
    /// Fetches the sticker objects for the given guild.
    /// </summary>
    /// <param name="guildId">The snowflake identifier of the guild in question.</param>
    /// <param name="ct">Cancellation token for this request.</param>
    public ValueTask<Result<IReadOnlyList<ISticker>>> ListGuildStickersAsync
    (
        Snowflake guildId,
        CancellationToken ct = default
    );

    /// <summary>
    /// Returns the specified guild sticker.
    /// </summary>
    /// <param name="guildId">The snowflake identifier of the guild owning this sticker.</param>
    /// <param name="stickerId">The snowflake identifier of the sticker in question.</param>
    /// <param name="ct">Cancellation token for this request.</param>
    public ValueTask<Result<ISticker>> GetGuildStickerAsync
    (
        Snowflake guildId,
        Snowflake stickerId,
        CancellationToken ct = default
    );

    /// <summary>
    /// Creates a sticker in the specified guild.
    /// </summary>
    /// <param name="guildId">The snowflake identifier of the guild in question.</param>
    /// <param name="payload">The information to initialize the sticker with.</param>
    /// <param name="reason">An optional audit log reason.</param>
    /// <param name="ct">Cancellation token for this request.</param>
    /// <returns>The newly created sticker object.</returns>
    public ValueTask<Result<ISticker>> CreateGuildStickerAsync
    (
        Snowflake guildId,
        ICreateGuildStickerPayload payload,
        string? reason = null,
        CancellationToken ct = default
    );

    /// <summary>
    /// Modifies the given sticker.
    /// </summary>
    /// <param name="guildId">The snowflake identifier of the guild owning the sticker.</param>
    /// <param name="stickerId">The snowflake identifier of the sticker in question.</param>
    /// <param name="payload">The new information for this sticker.</param>
    /// <param name="reason">An optional audit log reason.</param>
    /// <param name="ct">Cancellation token for this request.</param>
    /// <returns>The newly updated sticker object.</returns>
    public ValueTask<Result<ISticker>> ModifyGuildStickerAsync
    (
        Snowflake guildId,
        Snowflake stickerId,
        IModifyGuildStickerPayload payload,
        string? reason = null,
        CancellationToken ct = default
    );

    /// <summary>
    /// Deletes the specified sticker.
    /// </summary>
    /// <param name="guildId">The snowflake identifier of the guild owning the sticker.</param>
    /// <param name="stickerId">The snowflake identifier of the sticker in question.</param>
    /// <param name="reason">An optional audit log reason.</param>
    /// <param name="ct">Cancellation token for this request.</param>
    public ValueTask<Result> DeleteGuildStickerAsync
    (
        Snowflake guildId,
        Snowflake stickerId,
        string? reason = null,
        CancellationToken ct = default
    );
}
