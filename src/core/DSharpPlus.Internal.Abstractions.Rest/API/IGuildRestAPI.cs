// This Source Code form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

using DSharpPlus.Entities;
using DSharpPlus.Internal.Abstractions.Models;
using DSharpPlus.Internal.Abstractions.Rest.Payloads;
using DSharpPlus.Internal.Abstractions.Rest.Queries;
using DSharpPlus.Internal.Abstractions.Rest.Responses;
using DSharpPlus.Results;

namespace DSharpPlus.Internal.Abstractions.Rest.API;

/// <summary>
/// Provides access to guild-related rest API calls.
/// </summary>
public interface IGuildRestAPI
{
    /// <summary>
    /// Creates a new guild with the bot user as its owner. This endpoint can only be used by bots in less than
    /// 10 guilds.
    /// </summary>
    /// <param name="payload">The information to create this guild with.</param>
    /// <param name="info">Additional instructions regarding this request.</param>
    /// <param name="ct">A cancellation token for this operation.</param>
    /// <returns>The newly created guild.</returns>
    public ValueTask<Result<IGuild>> CreateGuildAsync
    (
        ICreateGuildPayload payload,
        RequestInfo info = default,
        CancellationToken ct = default
    );

    /// <summary>
    /// Fetches a guild from its snowflake identifier.
    /// </summary>
    /// <param name="guildId">The snowflake identifier of the guild in question.</param>
    /// <param name="query">Specifies whether the response should include total and online member counts.</param>
    /// <param name="info">Additional instructions regarding this request.</param>
    /// <param name="ct">A cancellation token for this operation.</param>
    public ValueTask<Result<IGuild>> GetGuildAsync
    (
        Snowflake guildId,
        GetGuildQuery query = default,
        RequestInfo info = default,
        CancellationToken ct = default
    );

    /// <summary>
    /// Fetches the guild preview for the specified guild.
    /// </summary>
    /// <param name="guildId">The snowflake identifier of the guild in question.</param>
    /// <param name="info">Additional instructions regarding this request.</param>
    /// <param name="ct">A cancellation token for this operation.</param>
    public ValueTask<Result<IGuildPreview>> GetGuildPreviewAsync
    (
        Snowflake guildId,
        RequestInfo info = default,
        CancellationToken ct = default
    );

    /// <summary>
    /// Modifies a guild.
    /// </summary>
    /// <param name="guildId">The snowflake identifier of the guild in question.</param>
    /// <param name="payload">The fields of this guild to modify.</param>
    /// <param name="reason">An optional audit log reason for the changes.</param>
    /// <param name="info">Additional instructions regarding this request.</param>
    /// <param name="ct">A cancellation token for this operation.</param>
    /// <returns>The updated guild.</returns>
    public ValueTask<Result<IGuild>> ModifyGuildAsync
    (
        Snowflake guildId,
        IModifyGuildPayload payload,
        string? reason = null,
        RequestInfo info = default,
        CancellationToken ct = default
    );

    /// <summary>
    /// Permanently deletes a guild. This user must own the guild.
    /// </summary>
    /// <param name="guildId">The snowflake identifier of the guild in question.</param>
    /// <param name="info">Additional instructions regarding this request.</param>
    /// <param name="ct">A cancellation token for this operation.</param>
    /// <returns>Whether or not the request succeeded.</returns>
    public ValueTask<Result> DeleteGuildAsync
    (
        Snowflake guildId,
        RequestInfo info = default,
        CancellationToken ct = default
    );

    /// <summary>
    /// Requests all channels for this guild from the API. This excludes thread channels.
    /// </summary>
    /// <param name="guildId">The snowflake identifier of the guild in question.</param>
    /// <param name="info">Additional instructions regarding this request.</param>
    /// <param name="ct">A cancellation token for this operation.</param>
    public ValueTask<Result<IReadOnlyList<IChannel>>> GetGuildChannelsAsync
    (
        Snowflake guildId,
        RequestInfo info = default,
        CancellationToken ct = default
    );

    /// <summary>
    /// Creates a channel in this guild.
    /// </summary>
    /// <param name="guildId">The snowflake identifier of the parent guild.</param>
    /// <param name="payload">The shannel creation payload, containing all initializing data.</param>
    /// <param name="reason">An audit log reason for this operation.</param>
    /// <param name="info">Additional instructions regarding this request.</param>
    /// <param name="ct">A cancellation token for this operation.</param>
    /// <returns>The newly created channel.</returns>
    public ValueTask<Result<IChannel>> CreateGuildChannelAsync
    (
        Snowflake guildId,
        ICreateGuildChannelPayload payload,
        string? reason = null,
        RequestInfo info = default,
        CancellationToken ct = default
    );

    /// <summary>
    /// Moves channels in a guild.
    /// </summary>
    /// <param name="guildId">The snowflake identifier of the parent guild.</param>
    /// <param name="payload">Array of new channel data payloads, containing IDs and some optional data.</param>
    /// <param name="info">Additional instructions regarding this request.</param>
    /// <param name="ct">A cancellation token for this operation.</param>
    public ValueTask<Result> ModifyGuildChannelPositionsAsync
    (
        Snowflake guildId,
        IReadOnlyList<IModifyGuildChannelPositionsPayload> payload,
        RequestInfo info = default,
        CancellationToken ct = default
    );

    /// <summary>
    /// Queries all active thread channels in the given guild.
    /// </summary>
    /// <param name="guildId">The snowflake identifier of the queried guild.</param>
    /// <param name="info">Additional instructions regarding this request.</param>
    /// <param name="ct">A cancellation token for this operation.</param>
    /// <returns>
    /// A response payload object containing an array of thread channels and an array of thread member information
    /// for all threads the current user has joined.
    /// </returns>
    public ValueTask<Result<ListActiveGuildThreadsResponse>> ListActiveGuildThreadsAsync
    (
        Snowflake guildId,
        RequestInfo info = default,
        CancellationToken ct = default
    );

    /// <summary>
    /// Returns the given users associated guild member object.
    /// </summary>
    /// <param name="guildId">The snowflake identifier of the queried guild.</param>
    /// <param name="userId">The snowflake identifier of the user in question.</param>
    /// <param name="info">Additional instructions regarding this request.</param>
    /// <param name="ct">A cancellation token for this operation.</param>
    public ValueTask<Result<IGuildMember>> GetGuildMemberAsync
    (
        Snowflake guildId,
        Snowflake userId,
        RequestInfo info = default,
        CancellationToken ct = default
    );

    /// <summary>
    /// Returns a list of guild member objects.
    /// </summary>
    /// <param name="guildId">The snowflake identifier of the guild to be queried.</param>
    /// <param name="query">
    /// Contains information pertaining to request pagination. Up to 1000 users are allowed per request.
    /// </param>
    /// <param name="info">Additional instructions regarding this request.</param>
    /// <param name="ct">A cancellation token for this operation.</param>
    public ValueTask<Result<IReadOnlyList<IGuildMember>>> ListGuildMembersAsync
    (
        Snowflake guildId,
        ForwardsPaginatedQuery query = default,
        RequestInfo info = default,
        CancellationToken ct = default
    );

    /// <summary>
    /// Returns a list of guild member objects whose username or nickname starts with the given string.
    /// </summary>
    /// <param name="guildId">The snowflake identifier of the string in question.</param>
    /// <param name="query">The string to search for and the maximum amount of members to return; 1 - 1000.</param>
    /// <param name="info">Additional instructions regarding this request.</param>
    /// <param name="ct">A cancellation token for this operation.</param>
    public ValueTask<Result<IReadOnlyList<IGuildMember>>> SearchGuildMembersAsync
    (
        Snowflake guildId,
        SearchGuildMembersQuery query,
        RequestInfo info = default,
        CancellationToken ct = default
    );

    /// <summary>
    /// Adds a discord user to the given guild using their oauth2 access token.
    /// </summary>
    /// <param name="guildId">The snowflake identifier of the guild in question.</param>
    /// <param name="userId">The snowflake identifier of the user in question.</param>
    /// <param name="payload">A payload containing the OAuth2 token and initial information for the user.</param>
    /// <param name="info">Additional instructions regarding this request.</param>
    /// <param name="ct">A cancellation token for this operation.</param>
    /// <returns>The newly created guild member, or null if the member had already joined the guild.</returns>
    public ValueTask<Result<IGuildMember?>> AddGuildMemberAsync
    (
        Snowflake guildId,
        Snowflake userId,
        IAddGuildMemberPayload payload,
        RequestInfo info = default,
        CancellationToken ct = default
    );

    /// <summary>
    /// Modifies a given user in the given guild.
    /// </summary>
    /// <param name="guildId">The snowflake identifier of the guild in question.</param>
    /// <param name="userId">The snowflake identifier of the user in question.</param>
    /// <param name="payload">The edits to make to this member.</param>
    /// <param name="reason">An optional audit log reason.</param>
    /// <param name="info">Additional instructions regarding this request.</param>
    /// <param name="ct">A cancellation token for this operation.</param>
    /// <returns>The modified guild member.</returns>
    public ValueTask<Result<IGuildMember>> ModifyGuildMemberAsync
    (
        Snowflake guildId,
        Snowflake userId,
        IModifyGuildMemberPayload payload,
        string? reason = null,
        RequestInfo info = default,
        CancellationToken ct = default
    );

    /// <summary>
    /// Modifies the current user in the given guild. Currently, only setting the nickname is supported.
    /// </summary>
    /// <param name="guildId">The snowflake identifier of the guild in question.</param>
    /// <param name="payload">The payload containing the new nickname for the current user.</param>
    /// <param name="reason">An optional audit log reason.</param>
    /// <param name="info">Additional instructions regarding this request.</param>
    /// <param name="ct">A cancellation token for this operation.</param>
    /// <returns>The new current user event.</returns>
    public ValueTask<Result<IGuildMember>> ModifyCurrentMemberAsync
    (
        Snowflake guildId,
        IModifyCurrentMemberPayload payload,
        string? reason = null,
        RequestInfo info = default,
        CancellationToken ct = default
    );

    /// <summary>
    /// Adds a role to a guild member in a given guild.
    /// </summary>
    /// <param name="guildId">The snowflake identifier of the guild in question.</param>
    /// <param name="userId">The snowflake identifier of the user in question.</param>
    /// <param name="roleId">The snowflake identifier of the role in question.</param>
    /// <param name="reason">An optional audit log reason.</param>
    /// <param name="info">Additional instructions regarding this request.</param>
    /// <param name="ct">A cancellation token for this operation.</param>
    public ValueTask<Result> AddGuildMemberRoleAsync
    (
        Snowflake guildId,
        Snowflake userId,
        Snowflake roleId,
        string? reason = null,
        RequestInfo info = default,
        CancellationToken ct = default
    );

    /// <summary>
    /// Removes the given role from the given member in the given guild.
    /// </summary>
    /// <param name="guildId">The snowflake identifier of the guild in question.</param>
    /// <param name="userId">The snowflake identifier of the user in question.</param>
    /// <param name="roleId">The snowflake identifier of the role in question.</param>
    /// <param name="reason">An optional audit log reason.</param>
    /// <param name="info">Additional instructions regarding this request.</param>
    /// <param name="ct">A cancellation token for this operation.</param>
    public ValueTask<Result> RemoveGuildMemberRoleAsync
    (
        Snowflake guildId,
        Snowflake userId,
        Snowflake roleId,
        string? reason = null,
        RequestInfo info = default,
        CancellationToken ct = default
    );

    /// <summary>
    /// Kicks the given user from the given guild.
    /// </summary>
    /// <param name="guildId">The snowflake identifier of the guild in question.</param>
    /// <param name="userId">The snowflake identifier of the user in question.</param>
    /// <param name="reason">An optional audit log reason.</param>
    /// <param name="info">Additional instructions regarding this request.</param>
    /// <param name="ct">A cancellation token for this operation.</param>
    public ValueTask<Result> RemoveGuildMemberAsync
    (
        Snowflake guildId,
        Snowflake userId,
        string? reason = null,
        RequestInfo info = default,
        CancellationToken ct = default
    );

    /// <summary>
    /// Returns a list of bans from the given guild. This endpoint is paginated.
    /// </summary>
    /// <param name="guildId">Snowflake identifier of the guild in question.</param>
    /// <param name="query">The query parameters used for pagination. Up to 1000 bans can be returned.</param>
    /// <param name="info">Additional instructions regarding this request.</param>
    /// <param name="ct">A cancellation token for this operation.</param>
    /// <returns>An array of <see cref="IBan"/> objects, representing the bans in the guild.</returns>
    public ValueTask<Result<IReadOnlyList<IBan>>> GetGuildBansAsync
    (
        Snowflake guildId,
        PaginatedQuery query = default,
        RequestInfo info = default,
        CancellationToken ct = default
    );

    /// <summary>
    /// Returns the ban object for the given user.
    /// </summary>
    /// <param name="guildId">The snowflake identifier of the guild in question.</param>
    /// <param name="userId">The snowflake identifier of the user in question.</param>
    /// <param name="info">Additional instructions regarding this request.</param>
    /// <param name="ct">A cancellation token for this operation.</param>
    public ValueTask<Result<IBan>> GetGuildBanAsync
    (
        Snowflake guildId,
        Snowflake userId,
        RequestInfo info = default,
        CancellationToken ct = default
    );

    /// <summary>
    /// Bans the given user from the given guild.
    /// </summary>
    /// <param name="guildId">The snowflake identifier of the guild in question.</param>
    /// <param name="userId">The snowflake identifier of the user in question.</param>
    /// <param name="query">
    /// Specifies how many seconds of message history from this user shall be purged, between 0 and
    /// 604800, which equals 7 days.
    /// </param>
    /// <param name="reason">Specifies an audit log reason for the ban.</param>
    /// <param name="info">Additional instructions regarding this request.</param>
    /// <param name="ct">A cancellation token for this operation.</param>
    public ValueTask<Result> CreateGuildBanAsync
    (
        Snowflake guildId,
        Snowflake userId,
        CreateGuildBanQuery query = default,
        string? reason = null,
        RequestInfo info = default,
        CancellationToken ct = default
    );

    /// <summary>
    /// Removes a ban from the given guild for the given user.
    /// </summary>
    /// <param name="guildId">The snowflake identifier of the guild in question.</param>
    /// <param name="userId">The snowflake identifier of the user in question.</param>
    /// <param name="reason">An optional audit log reason for the ban.</param>
    /// <param name="info">Additional instructions regarding this request.</param>
    /// <param name="ct">A cancellation token for this operation.</param>
    public ValueTask<Result> RemoveGuildBanAsync
    (
        Snowflake guildId,
        Snowflake userId,
        string? reason = null,
        RequestInfo info = default,
        CancellationToken ct = default
    );

    /// <summary>
    /// Bans up to 200 users from the given guild.
    /// </summary>
    /// <param name="guildId">The snowflake identifier of the guild in question.</param>
    /// <param name="payload">
    /// The snowflake identifiers of the users to ban, and the amount of seconds to delete messages from.
    /// </param>
    /// <param name="reason">An optional audit log reason for the bans.</param>
    /// <param name="info">Additional instructions regarding this request.</param>
    /// <param name="ct">A cancellation token for this operation.</param>
    /// <returns>The snowflake identifiers of users that were banned and users that could not be banned.</returns>
    public ValueTask<Result<BulkGuildBanResponse>> BulkGuildBanAsync
    (
        Snowflake guildId,
        IBulkGuildBanPayload payload,
        string? reason = null,
        RequestInfo info = default,
        CancellationToken ct = default
    );

    /// <summary>
    /// Fetches the role list of the specified guild.
    /// </summary>
    /// <param name="guildId">The snowflake identifier of the guild in question.</param>
    /// <param name="info">Additional instructions regarding this request.</param>
    /// <param name="ct">A cancellation token for this operation.</param>
    public ValueTask<Result<IReadOnlyList<IRole>>> GetGuildRolesAsync
    (
        Snowflake guildId,
        RequestInfo info = default,
        CancellationToken ct = default
    );

    /// <summary>
    /// Creates a role in a given guild.
    /// </summary>
    /// <param name="guildId">The snowflake identifier of the guild in question.</param>
    /// <param name="payload">The information to initialize the role with.</param>
    /// <param name="reason">An optional audit log reason.</param>
    /// <param name="info">Additional instructions regarding this request.</param>
    /// <param name="ct">A cancellation token for this operation.</param>
    /// <returns>The newly created role.</returns>
    public ValueTask<Result<IRole>> CreateGuildRoleAsync
    (
        Snowflake guildId,
        ICreateGuildRolePayload payload,
        string? reason = null,
        RequestInfo info = default,
        CancellationToken ct = default
    );

    /// <summary>
    /// Modifies the positions of roles in the role list.
    /// </summary>
    /// <param name="guildId">The snowflake identifier of the guild in question.</param>
    /// <param name="payload">The new positions for the roles.</param>
    /// <param name="reason">An optional audit log reason for this action.</param>
    /// <param name="info">Additional instructions regarding this request.</param>
    /// <param name="ct">A cancellation token for this operation.</param>
    /// <returns>The newly ordered list of roles for this guild.</returns>
    public ValueTask<Result<IReadOnlyList<IRole>>> ModifyGuildRolePositionsAsync
    (
        Snowflake guildId,
        IReadOnlyList<IModifyGuildRolePositionsPayload> payload,
        string? reason = null,
        RequestInfo info = default,
        CancellationToken ct = default
    );

    /// <summary>
    /// Modifies the settings of a specific role.
    /// </summary>
    /// <param name="guildId">The snowflake identifier of the guild the role belongs to.</param>
    /// <param name="roleId">The snowflake identifier of the role in question.</param>
    /// <param name="payload">The new role settings for this role.</param>
    /// <param name="reason">An optional audit log reason.</param>
    /// <param name="info">Additional instructions regarding this request.</param>
    /// <param name="ct">A cancellation token for this operation.</param>
    /// <returns>The modified role object.</returns>
    public ValueTask<Result<IRole>> ModifyGuildRoleAsync
    (
        Snowflake guildId,
        Snowflake roleId,
        IModifyGuildRolePayload payload,
        string? reason = null,
        RequestInfo info = default,
        CancellationToken ct = default
    );

    /// <summary>
    /// Modifies a guild's MFA level.
    /// </summary>
    /// <param name="guildId">The snowflake identifier of the guild in question.</param>
    /// <param name="payload">The new MFA level for this guild.</param>
    /// <param name="reason">An optional audit log reason.</param>
    /// <param name="info">Additional instructions regarding this request.</param>
    /// <param name="ct">A cancellation token for this operation.</param>
    /// <returns>The updated MFA level.</returns>
    public ValueTask<Result<DiscordMfaLevel>> ModifyGuildMFALevelAsync
    (
        Snowflake guildId,
        IModifyGuildMfaLevelPayload payload,
        string? reason = null,
        RequestInfo info = default,
        CancellationToken ct = default
    );

    /// <summary>
    /// Deletes a role from a guild.
    /// </summary>
    /// <param name="guildId">The snowflake identifier of the guild the role belongs to.</param>
    /// <param name="roleId">The snowflake identifier of the role in question.</param>
    /// <param name="reason">An optional audit log reason.</param>
    /// <param name="info">Additional instructions regarding this request.</param>
    /// <param name="ct">A cancellation token for this operation.</param>
    public ValueTask<Result> DeleteGuildRoleAsync
    (
        Snowflake guildId,
        Snowflake roleId,
        string? reason,
        RequestInfo info = default,
        CancellationToken ct = default
    );

    /// <summary>
    /// Queries how many users would be kicked from a given guild in a prune.
    /// </summary>
    /// <param name="guildId">The snowflake identifier of the guild in question.</param>
    /// <param name="query">Provides additional information on which members to count.</param>
    /// <param name="info">Additional instructions regarding this request.</param>
    /// <param name="ct">A cancellation token for this operation.</param>
    public ValueTask<Result<GetGuildPruneCountResponse>> GetGuildPruneCountAsync
    (
        Snowflake guildId,
        GetGuildPruneCountQuery query = default,
        RequestInfo info = default,
        CancellationToken ct = default
    );

    /// <summary>
    /// Initiates a prune from the guild in question.
    /// </summary>
    /// <param name="guildId">The snowflake identifier of the guild in question.</param>
    /// <param name="payload">Contains additional information on which users to consider.</param>
    /// <param name="reason">Optional audit log reason for the prune.</param>
    /// <param name="info">Additional instructions regarding this request.</param>
    /// <param name="ct">A cancellation token for this operation.</param>
    /// <returns>The amount of users pruned.</returns>
    public ValueTask<Result<BeginGuildPruneResponse>> BeginGuildPruneAsync
    (
        Snowflake guildId,
        IBeginGuildPrunePayload payload,
        string? reason = null,
        RequestInfo info = default,
        CancellationToken ct = default
    );

    /// <summary>
    /// Queries all available voice regions for this guild, including VIP regions.
    /// </summary>
    /// <param name="guildId">The snowflake identifier of the guild in question.</param>
    /// <param name="info">Additional instructions regarding this request.</param>
    /// <param name="ct">A cancellation token for this operation.</param>
    public ValueTask<Result<IReadOnlyList<IVoiceRegion>>> GetGuildVoiceRegionsAsync
    (
        Snowflake guildId,
        RequestInfo info = default,
        CancellationToken ct = default
    );

    /// <summary>
    /// Returns a list of all active invites for this guild.
    /// </summary>
    /// <param name="guildId">The snowflake identifier of the guild in question.</param>
    /// <param name="info">Additional instructions regarding this request.</param>
    /// <param name="ct">A cancellation token for this operation.</param>
    public ValueTask<Result<IReadOnlyList<IInvite>>> GetGuildInvitesAsync
    (
        Snowflake guildId,
        RequestInfo info = default,
        CancellationToken ct = default
    );

    /// <summary>
    /// Returns a list of up to 50 active integrations for this guild. If a guild has more integrations,
    /// they cannot be accessed.
    /// </summary>
    /// <param name="guildId">The snowflake identifier of the guild in question.</param>
    /// <param name="info">Additional instructions regarding this request.</param>
    /// <param name="ct">A cancellation token for this operation.</param>
    public ValueTask<Result<IReadOnlyList<IIntegration>>> GetGuildIntegrationsAsync
    (
        Snowflake guildId,
        RequestInfo info = default,
        CancellationToken ct = default
    );

    /// <summary>
    /// Deletes an integration from the given guild.
    /// </summary>
    /// <param name="guildId">The snowflake identifier of the guild in question.</param>
    /// <param name="integrationId">The snowflake identifier of the integration to be deleted.</param>
    /// <param name="reason">An optional audit log reason.</param>
    /// <param name="info">Additional instructions regarding this request.</param>
    /// <param name="ct">A cancellation token for this operation.</param>
    public ValueTask<Result> DeleteGuildIntegrationAsync
    (
        Snowflake guildId,
        Snowflake integrationId,
        string? reason = null,
        RequestInfo info = default,
        CancellationToken ct = default
    );

    /// <summary>
    /// Queries the guild widget settings for the specified guild.
    /// </summary>
    /// <param name="guildId">The snowflake identifier of the guild to be queried.</param>
    /// <param name="info">Additional instructions regarding this request.</param>
    /// <param name="ct">A cancellation token for this operation.</param>
    public ValueTask<Result<IGuildWidgetSettings>> GetGuildWidgetSettingsAsync
    (
        Snowflake guildId,
        RequestInfo info = default,
        CancellationToken ct = default
    );

    /// <summary>
    /// Modifies the <see cref="IGuildWidget"/> for the specified guild.
    /// </summary>
    /// <param name="guildId">The snowflake identifier of the guild in question.</param>
    /// <param name="settings">The new settings for this guild widget.</param>
    /// <param name="reason">An optional audit log reason.</param>
    /// <param name="info">Additional instructions regarding this request.</param>
    /// <param name="ct">A cancellation token for this operation.</param>
    /// <returns>The new guild widget object.</returns>
    public ValueTask<Result<IGuildWidget>> ModifyGuildWidgetAsync
    (
        Snowflake guildId,
        IGuildWidgetSettings settings,
        string? reason = null,
        RequestInfo info = default,
        CancellationToken ct = default
    );

    /// <summary>
    /// Returns the guild widget for the specified guild.
    /// </summary>
    /// <param name="guildId">The snowflake identifier for the guild in question.</param>
    /// <param name="info">Additional instructions regarding this request.</param>
    /// <param name="ct">A cancellation token for this operation.</param>
    public ValueTask<Result<IGuildWidget>> GetGuildWidgetAsync
    (
        Snowflake guildId,
        RequestInfo info = default,
        CancellationToken ct = default
    );

    /// <summary>
    /// Queries the vanity invite URL for this guild, if available.
    /// </summary>
    /// <param name="guildId">The snowflake identifier of the guild in question.</param>
    /// <param name="info">Additional instructions regarding this request.</param>
    /// <param name="ct">A cancellation token for this operation.</param>
    public ValueTask<Result<IInvite>> GetGuildVanityUrlAsync
    (
        Snowflake guildId,
        RequestInfo info = default,
        CancellationToken ct = default
    );

    /// <summary>
    /// Returns the guild widget image as a binary stream.
    /// </summary>
    /// <param name="guildId">The snowflake identifier of the guild in question.</param>
    /// <param name="query">The widget style, either "shield" (default) or "banner1" through "banner4".</param>
    /// <param name="info">Additional instructions regarding this request.</param>
    /// <param name="ct">A cancellation token for this operation.</param>
    public ValueTask<Result<Stream>> GetGuildWidgetImageAsync
    (
        Snowflake guildId,
        GetGuildWidgetImageQuery query = default,
        RequestInfo info = default,
        CancellationToken ct = default
    );

    /// <summary>
    /// Returns the welcome screen of the given guild.
    /// </summary>
    /// <param name="guildId">The snowflake identifier of the guild in question.</param>
    /// <param name="info">Additional instructions regarding this request.</param>
    /// <param name="ct">A cancellation token for this operation.</param>
    public ValueTask<Result<IWelcomeScreen>> GetGuildWelcomeScreenAsync
    (
        Snowflake guildId,
        RequestInfo info = default,
        CancellationToken ct = default
    );

    /// <summary>
    /// Modifies the welcome screen of the given guild.
    /// </summary>
    /// <param name="guildId">The snowflake identifier of the guild in question.</param>
    /// <param name="payload">The information to modify the welcome screen with.</param>
    /// <param name="reason">An optional audit log reason.</param>
    /// <param name="info">Additional instructions regarding this request.</param>
    /// <param name="ct">A cancellation token for this operation.</param>
    /// <returns>The newly updated welcome screen.</returns>
    public ValueTask<Result<IWelcomeScreen>> ModifyGuildWelcomeScreenAsync
    (
        Snowflake guildId,
        IModifyGuildWelcomeScreenPayload payload,
        string? reason = null,
        RequestInfo info = default,
        CancellationToken ct = default
    );

    /// <summary>
    /// Returns the guild onboarding object for the specified guild.
    /// </summary>
    /// <param name="guildId">The snowflake identifier of the guild in question.</param>
    /// <param name="info">Additional instructions regarding this request.</param>
    /// <param name="ct">A cancellation token for this operation.</param>
    public ValueTask<Result<IOnboarding>> GetGuildOnboardingAsync
    (
        Snowflake guildId,
        RequestInfo info = default,
        CancellationToken ct = default
    );

    /// <summary>
    /// Modifies the onboarding configuration of the given guild.
    /// </summary>
    /// <param name="guildId">The snowflake identifier of the guild in question.</param>
    /// <param name="payload">The information to modify the onboarding configuration with.</param>
    /// <param name="reason">An optional audit log reason.</param>
    /// <param name="info">Additional instructions regarding this request.</param>
    /// <param name="ct">A cancellation token for this operation.</param>
    /// <returns>The newly updated onboarding configuration.</returns>
    public ValueTask<Result<IOnboarding>> ModifyGuildOnboardingAsync
    (
        Snowflake guildId,
        IModifyGuildOnboardingPayload payload,
        string? reason = null,
        RequestInfo info = default,
        CancellationToken ct = default
    );
}
