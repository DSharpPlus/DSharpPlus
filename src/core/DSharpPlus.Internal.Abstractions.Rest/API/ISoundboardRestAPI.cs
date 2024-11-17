// This Source Code form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

using DSharpPlus.Internal.Abstractions.Models;
using DSharpPlus.Internal.Abstractions.Rest.Payloads;
using DSharpPlus.Internal.Abstractions.Rest.Responses;
using DSharpPlus.Results;

namespace DSharpPlus.Internal.Abstractions.Rest.API;

/// <summary>
/// Provides access to soundboard-related REST API calls.
/// </summary>
public interface ISoundboardRestAPI
{
    /// <summary>
    /// Sends a soundboard sound in the voice channel the current user is connected to.
    /// </summary>
    /// <remarks>
    /// In addition to the relevant soundboard permissions, this also requires that the user
    /// is not muted, suppressed, or, importantly, deafened/self-deafened.
    /// </remarks>
    /// <param name="channelId">The snowflake identifier of the channel the user is connected to.</param>
    /// <param name="payload">The sound ID and source.</param>
    /// <param name="info">Additional instructions regarding this request.</param>
    /// <param name="ct">A cancellation token for this operation.</param>
    /// <returns>A value indicating whether the operation was successful.</returns>
    public ValueTask<Result> SendSoundboardSoundAsync
    (
        Snowflake channelId,
        ISendSoundboardSoundPayload payload,
        RequestInfo info = default,
        CancellationToken ct = default
    );

    /// <summary>
    /// Lists the default soundboard sounds that can be used by all users.
    /// </summary>
    /// <param name="info">Additional instructions regarding this request.</param>
    /// <param name="ct">A cancellation token for this operation.</param>
    public ValueTask<Result<IReadOnlyList<ISoundboardSound>>> ListDefaultSoundboardSoundsAsync
    (
        RequestInfo info = default,
        CancellationToken ct = default
    );

    /// <summary>
    /// Lists the soundboard sounds available in the given guild.
    /// </summary>
    /// <param name="guildId">The snowflake identifier of the guild to query.</param>
    /// <param name="info">Additional instructions regarding this request.</param>
    /// <param name="ct">A cancellation token for this operation.</param>
    public ValueTask<Result<ListGuildSoundboardSoundsResponse>> ListGuildSoundboardSoundsAsync
    (
        Snowflake guildId,
        RequestInfo info = default,
        CancellationToken ct = default
    );

    /// <summary>
    /// Gets a soundboard sound from the specified guild.
    /// </summary>
    /// <param name="guildId">The snowflake identifier of the given guild.</param>
    /// <param name="soundId">The snowflake identifier of the given sound.</param>
    /// <param name="info">Additional instructions regarding this request.</param>
    /// <param name="ct">A cancellation token for this operation.</param>
    public ValueTask<Result<ISoundboardSound>> GetGuildSoundboardSoundAsync
    (
        Snowflake guildId,
        Snowflake soundId,
        RequestInfo info = default,
        CancellationToken ct = default
    );

    /// <summary>
    /// Creates a soundboard sound in the specified guild.
    /// </summary>
    /// <param name="guildId">The snowflake identifier of the given guild.</param>
    /// <param name="payload">The infomration necessary to create the sound.</param>
    /// <param name="reason">An optional audit log reason.</param>
    /// <param name="info">Additional instructions regarding this request.</param>
    /// <param name="ct">A cancellation token for this operation.</param>
    /// <returns>The newly created soundboard sound.</returns>
    public ValueTask<Result<ISoundboardSound>> CreateGuildSoundboardSoundAsync
    (
        Snowflake guildId,
        ICreateGuildSoundboardSoundPayload payload,
        string? reason = null,
        RequestInfo info = default,
        CancellationToken ct = default
    );

    /// <summary>
    /// Modifies the specified soundboard sound in the specified guild.
    /// </summary>
    /// <param name="guildId">The snowflake identifier of the given guild.</param>
    /// <param name="soundId">The snowflake identifier of the sound to modify.</param>
    /// <param name="payload">The infomration necessary to create the sound.</param>
    /// <param name="reason">An optional audit log reason.</param>
    /// <param name="info">Additional instructions regarding this request.</param>
    /// <param name="ct">A cancellation token for this operation.</param>
    /// <returns>The modified soundboard sound.</returns>
    public ValueTask<Result<ISoundboardSound>> ModifyGuildSoundboardSoundAsync
    (
        Snowflake guildId,
        Snowflake soundId,
        IModifyGuildSoundboardSoundPayload payload,
        string? reason = null,
        RequestInfo info = default,
        CancellationToken ct = default
    );

    /// <summary>
    /// Deletes the specified soundboard sound in the specified guild.
    /// </summary>
    /// <param name="guildId">The snowflake identifier of the given guild.</param>
    /// <param name="soundId">The snowflake identifier of the sound to delete.</param>
    /// <param name="reason">An optional audit log reason.</param>
    /// <param name="info">Additional instructions regarding this request.</param>
    /// <param name="ct">A cancellation token for this operation.</param>
    /// <returns>A value indicating whether deletion was successful.</returns>
    public ValueTask<Result> DeleteGuildSoundboardSoundAsync
    (
        Snowflake guildId,
        Snowflake soundId,
        string? reason = null,
        RequestInfo info = default,
        CancellationToken ct = default
    );
}
