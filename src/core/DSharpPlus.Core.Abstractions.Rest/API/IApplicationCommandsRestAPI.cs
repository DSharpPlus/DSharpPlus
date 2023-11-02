// This Source Code form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

using DSharpPlus.Core.Abstractions.Models;
using DSharpPlus.Core.Abstractions.Rest.Payloads;
using DSharpPlus.Core.Abstractions.Rest.Responses;

using Remora.Results;

namespace DSharpPlus.Core.Abstractions.Rest.API;

/// <summary>
/// Provides access to application commands-related API calls.
/// </summary>
// https://discord.com/developers/docs/interactions/application-commands
public interface IApplicationCommandsRestAPI
{
    /// <summary>
    /// Fetches all global application commands for your application.
    /// </summary>
    /// <param name="applicationId">The snowflake identifier of your application.</param>
    /// <param name="withLocalizations">Indicates whether to include full localizations in the returned objects.</param>
    /// <param name="info">Additional instructions regarding this request.</param>
    /// <param name="ct">A cancellation token for this operation.</param>
    /// <returns>An array of application commands.</returns>
    public ValueTask<Result<IReadOnlyList<IApplicationCommand>>> GetGlobalApplicationCommandsAsync
    (
        Snowflake applicationId,
        bool? withLocalizations = null,
        RequestInfo info = default,
        CancellationToken ct = default
    );

    /// <summary>
    /// Creates a new global application command for your application.
    /// </summary>
    /// <param name="applicationId">The snowflake identifier of your application.</param>
    /// <param name="payload">The command you wish to create.</param>
    /// <param name="info">Additional instructions regarding this request.</param>
    /// <param name="ct">A cancellation token for this operation.</param>
    /// <returns>A value indicating whether this command was newly created as well as the command object.</returns>
    public ValueTask<Result<CreateApplicationCommandResponse>> CreateGlobalApplicationCommandAsync
    (
        Snowflake applicationId,
        ICreateGlobalApplicationCommandPayload payload,
        RequestInfo info = default,
        CancellationToken ct = default
    );

    /// <summary>
    /// Fetches a global application command for your application.
    /// </summary>
    /// <param name="applicationId">The snowflake identifier of your application.</param>
    /// <param name="commandId">The snowflake identifier of the command to fetch.</param>
    /// <param name="info">Additional instructions regarding this request.</param>
    /// <param name="ct">A cancellation token for this operation.</param>
    /// <returns>The requested application command.</returns>
    public ValueTask<Result<IApplicationCommand>> GetGlobalApplicationCommandAsync
    (
        Snowflake applicationId,
        Snowflake commandId,
        RequestInfo info = default,
        CancellationToken ct = default
    );

    /// <summary>
    /// Edits a global application command for your application.
    /// </summary>
    /// <param name="applicationId">The snowflake identifier of your application.</param>
    /// <param name="commandId">The snowflake identifier of the command to edit.</param>
    /// <param name="payload">A payload containing the fields to edit with their new values.</param>
    /// <param name="info">Additional instructions regarding this request.</param>
    /// <param name="ct">A cancellation token for this operation.</param>
    /// <returns>The edited application command.</returns>
    public ValueTask<Result<IApplicationCommand>> EditGlobalApplicationCommandAsync
    (
        Snowflake applicationId,
        Snowflake commandId,
        IEditGlobalApplicationCommandPayload payload,
        RequestInfo info = default,
        CancellationToken ct = default
    );

    /// <summary>
    /// Deletes a global application command for your application.
    /// </summary>
    /// <param name="applicationId">The snowflake identifier of your application.</param>
    /// <param name="commandId">The snowflake identifier of the command to delete.</param>
    /// <param name="info">Additional instructions regarding this request.</param>
    /// <param name="ct">A cancellation token for this operation.</param>
    /// <returns>A value indicating the success of this operation.</returns>
    public ValueTask<Result> DeleteGlobalApplicationCommandAsync
    (
        Snowflake applicationId,
        Snowflake commandId,
        RequestInfo info = default,
        CancellationToken ct = default
    );

    /// <summary>
    /// Bulk-overwrites global application commands for your application with the provided commands.
    /// </summary>
    /// <remarks>
    /// This will overwrite all types of application commands: slash/chat input commands, user context menu
    /// commands and message context menu commands. Commands that did not already exist will count towards the
    /// daily application command creation limits, commands that did exist will not.
    /// </remarks>
    /// <param name="applicationId">The snowflake identifier of your application.</param>
    /// <param name="payload">The application commands to create.</param>
    /// <param name="info">Additional instructions regarding this request.</param>
    /// <param name="ct">A cancellation token for this operation.</param>
    /// <returns>The full list of application commands for your application after overwriting.</returns>
    public ValueTask<Result<IReadOnlyList<IApplicationCommand>>> BulkOverwriteGlobalApplicationCommandsAsync
    (
        Snowflake applicationId,
        IReadOnlyList<ICreateGlobalApplicationCommandPayload> payload,
        RequestInfo info = default,
        CancellationToken ct = default
    );
}
