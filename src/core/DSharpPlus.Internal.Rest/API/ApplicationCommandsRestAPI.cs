// This Source Code form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

#pragma warning disable IDE0046

using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

using DSharpPlus.Entities;
using DSharpPlus.Internal.Abstractions.Models;
using DSharpPlus.Internal.Abstractions.Rest;
using DSharpPlus.Internal.Abstractions.Rest.API;
using DSharpPlus.Internal.Abstractions.Rest.Errors;
using DSharpPlus.Internal.Abstractions.Rest.Payloads;
using DSharpPlus.Internal.Abstractions.Rest.Queries;
using DSharpPlus.Internal.Abstractions.Rest.Responses;
using DSharpPlus.Internal.Rest.Ratelimiting;

using Remora.Results;

namespace DSharpPlus.Internal.Rest.API;

/// <inheritdoc cref="IApplicationCommandsRestAPI"/>
public sealed class ApplicationCommandsRestAPI
(
    IRestClient restClient
)
    : IApplicationCommandsRestAPI
{
    /// <inheritdoc/>
    public async ValueTask<Result<IReadOnlyList<IApplicationCommand>>> BulkOverwriteGlobalApplicationCommandsAsync
    (
        Snowflake applicationId, 
        IReadOnlyList<ICreateGlobalApplicationCommandPayload> payload, 
        RequestInfo info = default, 
        CancellationToken ct = default
    )
    {
        foreach(ICreateGlobalApplicationCommandPayload command in payload)
        {
            if (command.Name.Length is > 32 or < 1)
            {
                return new ValidationError("The name of an application command must be between 1 and 32 characters.");
            }

            if (command.Name.Length is > 100 or < 1)
            {
                return new ValidationError("The description of an application command must be between 1 and 100 characters.");
            }

            if (command.Options.HasValue && command.Type != DiscordApplicationCommandType.ChatInput)
            {
                return new ValidationError("Only chat input commands can have options and subcommands.");
            }

            if (command.Options.HasValue && command.Options.Value!.Count > 25)
            {
                return new ValidationError("An application command can only have up to 25 options and subcommands.");
            }
        }

        if (payload.Count > 100)
        {
            return new ValidationError("An application can only have up to 100 global commands.");
        }

        return await restClient.ExecuteRequestAsync<IReadOnlyList<IApplicationCommand>>
        (
            HttpMethod.Put,
            $"applications/{applicationId}/commands",
            b => b.WithSimpleRoute
                 (
                    new SimpleStringRatelimitRoute
                    {
                        IsFracturable = false,
                        Resource = TopLevelResource.Other,
                        Route = "applications/:application-id/commands"
                    }
                 )
                 .WithPayload(payload),
            info,
            ct
        );
    }

    public ValueTask<Result<IReadOnlyList<IApplicationCommand>>> BulkOverwriteGuildApplicationCommandsAsync(Snowflake applicationId, Snowflake guildId, IReadOnlyList<ICreateGuildApplicationCommandPayload> payload, RequestInfo info = default, CancellationToken ct = default) => throw new System.NotImplementedException();
    public ValueTask<Result<CreateApplicationCommandResponse>> CreateGlobalApplicationCommandAsync(Snowflake applicationId, ICreateGlobalApplicationCommandPayload payload, RequestInfo info = default, CancellationToken ct = default) => throw new System.NotImplementedException();
    public ValueTask<Result<IApplicationCommand>> CreateGuildApplicationCommandAsync(Snowflake applicationId, Snowflake guildId, ICreateGuildApplicationCommandPayload payload, RequestInfo info = default, CancellationToken ct = default) => throw new System.NotImplementedException();
    public ValueTask<Result> DeleteGlobalApplicationCommandAsync(Snowflake applicationId, Snowflake commandId, RequestInfo info = default, CancellationToken ct = default) => throw new System.NotImplementedException();
    public ValueTask<Result> DeleteGuildApplicationCommandAsync(Snowflake applicationId, Snowflake guildId, Snowflake commandId, RequestInfo info = default, CancellationToken ct = default) => throw new System.NotImplementedException();
    public ValueTask<Result<IApplicationCommand>> EditGlobalApplicationCommandAsync(Snowflake applicationId, Snowflake commandId, IEditGlobalApplicationCommandPayload payload, RequestInfo info = default, CancellationToken ct = default) => throw new System.NotImplementedException();
    public ValueTask<Result<IApplicationCommand>> EditGuildApplicationCommandAsync(Snowflake applicationId, Snowflake guildId, Snowflake commandId, IEditGuildApplicationCommandPayload payload, RequestInfo info = default, CancellationToken ct = default) => throw new System.NotImplementedException();
    public ValueTask<Result<IApplicationCommandPermissions>> GetApplicationCommandPermissionsAsync(Snowflake applicationId, Snowflake guildId, Snowflake commandId, RequestInfo info = default, CancellationToken ct = default) => throw new System.NotImplementedException();
    public ValueTask<Result<IApplicationCommand>> GetGlobalApplicationCommandAsync(Snowflake applicationId, Snowflake commandId, RequestInfo info = default, CancellationToken ct = default) => throw new System.NotImplementedException();
    public ValueTask<Result<IReadOnlyList<IApplicationCommand>>> GetGlobalApplicationCommandsAsync(Snowflake applicationId, LocalizationQuery query = default, RequestInfo info = default, CancellationToken ct = default) => throw new System.NotImplementedException();
    public ValueTask<Result<IApplicationCommand>> GetGuildApplicationCommandAsync(Snowflake applicationId, Snowflake guildId, Snowflake commandId, RequestInfo info = default, CancellationToken ct = default) => throw new System.NotImplementedException();
    public ValueTask<Result<IReadOnlyList<IApplicationCommandPermissions>>> GetGuildApplicationCommandPermissionsAsync(Snowflake applicationId, Snowflake guildId, RequestInfo info = default, CancellationToken ct = default) => throw new System.NotImplementedException();
    public ValueTask<Result<IReadOnlyList<IApplicationCommand>>> GetGuildApplicationCommandsAsync(Snowflake applicationId, Snowflake guildId, LocalizationQuery query = default, RequestInfo info = default, CancellationToken ct = default) => throw new System.NotImplementedException();
}
