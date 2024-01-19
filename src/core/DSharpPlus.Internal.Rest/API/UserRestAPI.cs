// This Source Code form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

#pragma warning disable IDE0046

using System.Collections.Generic;
using System.Globalization;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

using DSharpPlus.Internal.Abstractions.Models;
using DSharpPlus.Internal.Abstractions.Rest;
using DSharpPlus.Internal.Abstractions.Rest.API;
using DSharpPlus.Internal.Abstractions.Rest.Errors;
using DSharpPlus.Internal.Abstractions.Rest.Payloads;
using DSharpPlus.Internal.Abstractions.Rest.Queries;
using DSharpPlus.Internal.Rest.Ratelimiting;

using Remora.Results;

namespace DSharpPlus.Internal.Rest.API;

/// <inheritdoc cref="IUserRestAPI"/>
public sealed class UserRestAPI(IRestClient restClient)
    : IUserRestAPI
{
    /// <inheritdoc/>
    public async ValueTask<Result<IChannel>> CreateDmAsync
    (
        ICreateDmPayload payload,
        RequestInfo info = default,
        CancellationToken ct = default
    )
    {
        return await restClient.ExecuteRequestAsync<IChannel>
        (
            HttpMethod.Post,
            "users/@me/channels",
            b => b.WithRoute($"POST users/@me/channels")
                 .WithPayload(payload),
            info,
            ct
        );
    }

    /// <inheritdoc/>
    public async ValueTask<Result<IChannel>> CreateGroupDmAsync
    (
        ICreateGroupDmPayload payload,
        RequestInfo info = default,
        CancellationToken ct = default
    )
    {
        return await restClient.ExecuteRequestAsync<IChannel>
        (
            HttpMethod.Post,
            "users/@me/channels",
            b => b.WithRoute($"POST users/@me/channels")
                 .WithPayload(payload),
            info,
            ct
        );
    }

    /// <inheritdoc/>
    public async ValueTask<Result<IApplicationRoleConnection>> GetCurrentUserApplicationRoleConnectionAsync
    (
        Snowflake applicationId,
        RequestInfo info = default,
        CancellationToken ct = default
    )
    {
        return await restClient.ExecuteRequestAsync<IApplicationRoleConnection>
        (
            HttpMethod.Get,
            $"users/@me/applications/{applicationId}/role-connection",
            b => b.WithRoute($"GET users/@me/applications/:application-id/role-connection"),
            info,
            ct
        );
    }

    /// <inheritdoc/>
    public async ValueTask<Result<IUser>> GetCurrentUserAsync
    (
        RequestInfo info = default,
        CancellationToken ct = default
    )
    {
        return await restClient.ExecuteRequestAsync<IUser>
        (
            HttpMethod.Get,
            "users/@me",
            b => b.WithRoute($"GET users/@me"),
            info,
            ct
        );
    }

    /// <inheritdoc/>
    public async ValueTask<Result<IReadOnlyList<IConnection>>> GetCurrentUserConnectionsAsync
    (
        RequestInfo info = default,
        CancellationToken ct = default
    )
    {
        return await restClient.ExecuteRequestAsync<IReadOnlyList<IConnection>>
        (
            HttpMethod.Get,
            "users/@me/connections",
            b => b.WithRoute("GET users/@me/connections"),
            info,
            ct
        );
    }

    /// <inheritdoc/>
    public async ValueTask<Result<IGuildMember>> GetCurrentUserGuildMemberAsync
    (
        Snowflake guildId,
        RequestInfo info = default,
        CancellationToken ct = default
    )
    {
        return await restClient.ExecuteRequestAsync<IGuildMember>
        (
            HttpMethod.Get,
            $"users/@me/guilds/{guildId}/member",
            b => b.WithSimpleRoute
                 (
                    new SimpleSnowflakeRatelimitRoute
                    {
                        Resource = TopLevelResource.Guild,
                        Id = guildId
                    }
                 )
                 .WithRoute($"GET users/@me/guilds/{guildId}/member"),
            info,
            ct
        );
    }

    /// <inheritdoc/>
    public async ValueTask<Result<IReadOnlyList<IPartialGuild>>> GetCurrentUserGuildsAsync
    (
        GetCurrentUserGuildsQuery query = default,
        RequestInfo info = default,
        CancellationToken ct = default
    )
    {
        QueryBuilder builder = new()
        {
            RootUri = "users/@me/guilds"
        };

        if (query.Limit is not null)
        {
            if (query.Limit.Value is < 1 or > 200)
            {
                return new ValidationError("The limit of guilds to request must be between 1 and 200.");
            }

            builder.AddParameter("limit", query.Limit.Value.ToString(CultureInfo.InvariantCulture));
        }

        if (query.Before is not null)
        {
            builder.AddParameter("before", query.Before.Value.ToString());
        }

        if (query.After is not null)
        {
            builder.AddParameter("after", query.After.Value.ToString());
        }

        if (query.WithCounts is not null)
        {
            builder.AddParameter("with_counts", query.WithCounts.Value.ToString().ToLowerInvariant());
        }

        return await restClient.ExecuteRequestAsync<IReadOnlyList<IPartialGuild>>
        (
            HttpMethod.Get,
            builder.Build(),
            b => b.WithRoute("GET users/@me/guilds"),
            info,
            ct
        );
    }

    /// <inheritdoc/>
    public async ValueTask<Result<IUser>> GetUserAsync
    (
        Snowflake userId,
        RequestInfo info = default,
        CancellationToken ct = default
    )
    {
        return await restClient.ExecuteRequestAsync<IUser>
        (
            HttpMethod.Get,
            $"users/{userId}",
            b => b.WithRoute("GET users/:user-id"),
            info,
            ct
        );
    }

    /// <inheritdoc/>
    public async ValueTask<Result> LeaveGuildAsync
    (
        Snowflake guildId,
        RequestInfo info = default,
        CancellationToken ct = default
    )
    {
        Result<HttpResponseMessage> response = await restClient.ExecuteRequestAsync
        (
            HttpMethod.Delete,
            $"users/@me/guilds/{guildId}",
            b => b.WithSimpleRoute
                 (
                    new SimpleSnowflakeRatelimitRoute
                    {
                        Resource = TopLevelResource.Guild,
                        Id = guildId
                    }
                 )
                 .WithRoute($"DELETE users/@me/guilds/{guildId}"),
            info,
            ct
        );

        return (Result)response;
    }

    /// <inheritdoc/>
    public async ValueTask<Result<IUser>> ModifyCurrentUserAsync
    (
        IModifyCurrentUserPayload payload,
        RequestInfo info = default,
        CancellationToken ct = default
    )
    {
        return await restClient.ExecuteRequestAsync<IUser>
        (
            HttpMethod.Patch,
            "users/@me",
            b => b.WithRoute("PATCH users/@me")
                 .WithPayload(payload),
            info,
            ct
        );
    }

    /// <inheritdoc/>
    public async ValueTask<Result<IApplicationRoleConnection>> UpdateCurrentUserApplicationRoleConnectionAsync
    (
        Snowflake applicationId,
        IUpdateCurrentUserApplicationRoleConnectionPayload payload,
        RequestInfo info = default,
        CancellationToken ct = default
    )
    {
        if (payload.PlatformName.HasValue && payload.PlatformName.Value.Length > 50)
        {
            return new ValidationError("The platform name of a role connection cannot exceed 50 characters.");
        }

        if (payload.PlatformUsername.HasValue && payload.PlatformUsername.Value.Length > 100)
        {
            return new ValidationError("The platform username of a role connection cannot exceed 100 characters.");
        }

        return await restClient.ExecuteRequestAsync<IApplicationRoleConnection>
        (
            HttpMethod.Put,
            $"users/@me/applications/{applicationId}/role-connection",
            b => b.WithRoute("PUT users/@me/applications/:application-id/role-connection")
                 .WithPayload(payload),
            info,
            ct
        );
    }
}
