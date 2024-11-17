// This Source Code form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

#pragma warning disable IDE0046

using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

using DSharpPlus.Internal.Abstractions.Models;
using DSharpPlus.Internal.Abstractions.Rest;
using DSharpPlus.Internal.Abstractions.Rest.API;
using DSharpPlus.Internal.Abstractions.Rest.Errors;
using DSharpPlus.Internal.Abstractions.Rest.Payloads;
using DSharpPlus.Internal.Abstractions.Rest.Responses;
using DSharpPlus.Internal.Rest.Ratelimiting;
using DSharpPlus.Results;

namespace DSharpPlus.Internal.Rest.API;

/// <inheritdoc cref="ISoundboardRestAPI"/>
public sealed class SoundboardRestAPI(IRestClient restClient)
    : ISoundboardRestAPI
{
    /// <inheritdoc/>
    public async ValueTask<Result<ISoundboardSound>> CreateGuildSoundboardSoundAsync
    (
        Snowflake guildId,
        ICreateGuildSoundboardSoundPayload payload,
        string? reason = null,
        RequestInfo info = default,
        CancellationToken ct = default
    )
    {
        if (payload.Name.Length is < 2 or > 32)
        {
            return new ValidationError("The sound name must be between 2 and 32 characters.");
        }

        if (payload.Volume.TryGetNonNullValue(out double? value) && value is < 0.0 or > 1.0)
        {
            return new ValidationError("The sound volume must be between 0.0 and 1.0.");
        }

        if (payload.EmojiId.TryGetNonNullValue(out _) && payload.EmojiName.TryGetNonNullValue(out _))
        {
            return new ValidationError("A sound must not have a custom emoji and an unicode emoji defined at the same time.");
        }

        return await restClient.ExecuteRequestAsync<ISoundboardSound>
        (
            HttpMethod.Post,
            $"guilds/{guildId}/soundboard-sounds",
            b => b.WithPayload(payload)
                  .WithSimpleRoute(TopLevelResource.Guild, guildId)
                  .WithAuditLogReason(reason),
            info,
            ct
        );
    }

    /// <inheritdoc/>
    public async ValueTask<Result> DeleteGuildSoundboardSoundAsync
    (
        Snowflake guildId,
        Snowflake soundId,
        string? reason = null,
        RequestInfo info = default,
        CancellationToken ct = default
    )
    {
        return await restClient.ExecuteRequestAsync
        (
            HttpMethod.Post,
            $"guilds/{guildId}/soundboard-sounds/{soundId}",
            b => b.WithSimpleRoute(TopLevelResource.Guild, guildId)
                  .WithRoute($"POST guilds/{guildId}/soundboard-sounds/:sound-id")
                  .WithAuditLogReason(reason),
            info,
            ct
        );
    }

    /// <inheritdoc/>
    public async ValueTask<Result<ISoundboardSound>> GetGuildSoundboardSoundAsync
    (
        Snowflake guildId,
        Snowflake soundId,
        RequestInfo info = default,
        CancellationToken ct = default
    )
    {
        return await restClient.ExecuteRequestAsync<ISoundboardSound>
        (
            HttpMethod.Get,
            $"guilds/{guildId}/soundboard-sounds/{soundId}",
            b => b.WithSimpleRoute(TopLevelResource.Guild, guildId)
                  .WithRoute($"GET guilds/{guildId}/soundboard-sounds/:sound-id"),
            info,
            ct
        );
    }

    /// <inheritdoc/>
    public async ValueTask<Result<IReadOnlyList<ISoundboardSound>>> ListDefaultSoundboardSoundsAsync
    (
        RequestInfo info = default,
        CancellationToken ct = default
    )
    {
        return await restClient.ExecuteRequestAsync<IReadOnlyList<ISoundboardSound>>
        (
            HttpMethod.Get,
            "soundboard-default-sounds",
            info: info,
            ct: ct
        );
    }

    /// <inheritdoc/>
    public async ValueTask<Result<ListGuildSoundboardSoundsResponse>> ListGuildSoundboardSoundsAsync
    (
        Snowflake guildId,
        RequestInfo info = default,
        CancellationToken ct = default
    )
    {
        return await restClient.ExecuteRequestAsync<ListGuildSoundboardSoundsResponse>
        (
            HttpMethod.Get,
            $"guilds/{guildId}/soundboard-sounds/",
            b => b.WithSimpleRoute(TopLevelResource.Guild, guildId),
            info,
            ct
        );
    }

    /// <inheritdoc/>
    public async ValueTask<Result<ISoundboardSound>> ModifyGuildSoundboardSoundAsync
    (
        Snowflake guildId,
        Snowflake soundId,
        IModifyGuildSoundboardSoundPayload payload,
        string? reason = null,
        RequestInfo info = default,
        CancellationToken ct = default
    )
    {
        if (payload.Name.TryGetNonNullValue(out string? name) && name.Length is < 2 or > 32)
        {
            return new ValidationError("The sound name must be between 2 and 32 characters.");
        }

        if (payload.Volume.TryGetNonNullValue(out double? value) && value is < 0.0 or > 1.0)
        {
            return new ValidationError("The sound volume must be between 0.0 and 1.0.");
        }

        if (payload.EmojiId.TryGetNonNullValue(out _) && payload.EmojiName.TryGetNonNullValue(out _))
        {
            return new ValidationError("A sound must not have a custom emoji and an unicode emoji defined at the same time.");
        }

        return await restClient.ExecuteRequestAsync<ISoundboardSound>
        (
            HttpMethod.Patch,
            $"guilds/{guildId}/soundboard-sounds",
            b => b.WithPayload(payload)
                  .WithSimpleRoute(TopLevelResource.Guild, guildId)
                  .WithAuditLogReason(reason),
            info,
            ct
        );
    }

    /// <inheritdoc/>
    public async ValueTask<Result> SendSoundboardSoundAsync
    (
        Snowflake channelId,
        ISendSoundboardSoundPayload payload,
        RequestInfo info = default,
        CancellationToken ct = default
    )
    {
        return await restClient.ExecuteRequestAsync<ListGuildSoundboardSoundsResponse>
        (
            HttpMethod.Post,
            $"channels/{channelId}/send-soundboard-sound",
            b => b.WithSimpleRoute(TopLevelResource.Channel, channelId)
                  .WithPayload(payload),
            info,
            ct
        );
    }
}
