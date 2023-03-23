// This file is part of the DSharpPlus project.
//
// Copyright (c) 2015 Mike Santiago
// Copyright (c) 2016-2023 DSharpPlus Contributors
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using DSharpPlus.Entities;
using DSharpPlus.Net.Models;

namespace DSharpPlus
{
    public sealed partial class DiscordRestClient
    {
        /// <summary>
        /// Adds a member to a guild
        /// </summary>
        /// <param name="guildId">Guild ID</param>
        /// <param name="userId">User ID</param>
        /// <param name="accessToken">Access token</param>
        /// <param name="nick">User nickname</param>
        /// <param name="roles">User roles</param>
        /// <param name="muted">Whether this user should be muted on join</param>
        /// <param name="deafened">Whether this user should be deafened on join</param>
        public Task<DiscordMember> AddGuildMemberAsync(
            ulong guildId,
            ulong userId,
            string accessToken,
            string nick,
            IEnumerable<DiscordRole> roles,
            bool muted,
            bool deafened
        ) => this.ApiClient.AddGuildMemberAsync(guildId, userId, accessToken, nick, roles, muted, deafened);

        /// <summary>
        /// Gets all guild members
        /// </summary>
        /// <param name="guildId">Guild ID</param>
        /// <param name="limit">Member download limit</param>
        /// <param name="after">Gets members after this ID</param>
        public async Task<IReadOnlyList<DiscordMember>> ListGuildMembersAsync(ulong guildId, int? limit, ulong? after)
        {
            limit ??= 1000;
            var receiveMembers = new List<DiscordMember>();
            var receivedMemberCount = limit;
            var lastMemberId = after;
            while (receivedMemberCount == limit)
            {
                var transportMembers = await this.ApiClient.ListGuildMembersAsync(guildId, limit, lastMemberId == 0 ? null : lastMemberId);
                receivedMemberCount = transportMembers.Count;

                foreach (var transportMember in transportMembers)
                {
                    lastMemberId = transportMember.User.Id;
                    if (this.UserCache.ContainsKey(transportMember.User.Id))
                        continue;

                    this.UpdateUserCache(new DiscordUser(transportMember.User) { Discord = this });
                    receiveMembers.Add(new DiscordMember(transportMember) { Discord = this, _guild_id = guildId });
                }
            }

            return new ReadOnlyCollection<DiscordMember>(receiveMembers);
        }

        /// <summary>
        /// Add role to guild member
        /// </summary>
        /// <param name="guildId">Guild ID</param>
        /// <param name="userId">User ID</param>
        /// <param name="roleId">Role ID</param>
        /// <param name="reason">Reason this role gets added</param>
        public Task AddGuildMemberRoleAsync(ulong guildId, ulong userId, ulong roleId, string reason)
            => this.ApiClient.AddGuildMemberRoleAsync(guildId, userId, roleId, reason);

        /// <summary>
        /// Remove role from member
        /// </summary>
        /// <param name="guildId">Guild ID</param>
        /// <param name="userId">User ID</param>
        /// <param name="roleId">Role ID</param>
        /// <param name="reason">Reason this role gets removed</param>
        public Task RemoveGuildMemberRoleAsync(ulong guildId, ulong userId, ulong roleId, string reason)
            => this.ApiClient.RemoveGuildMemberRoleAsync(guildId, userId, roleId, reason);

        /// <summary>
        /// Gets guild member
        /// </summary>
        /// <param name="guildId">Guild ID</param>
        /// <param name="memberId">Member ID</param>
        public Task<DiscordMember> GetGuildMemberAsync(ulong guildId, ulong memberId)
            => this.ApiClient.GetGuildMemberAsync(guildId, memberId);

        /// <summary>
        /// Removes guild member
        /// </summary>
        /// <param name="guildId">Guild ID</param>
        /// <param name="userId">User ID</param>
        /// <param name="reason">Why this user was removed</param>
        public Task RemoveGuildMemberAsync(ulong guildId, ulong userId, string reason)
            => this.ApiClient.RemoveGuildMemberAsync(guildId, userId, reason);

        /// <summary>
        /// Modifies guild member.
        /// </summary>
        /// <param name="guildId">Guild ID</param>
        /// <param name="userId">User ID</param>
        /// <param name="nick">New nickname</param>
        /// <param name="roleIds">New roles</param>
        /// <param name="mute">Whether this user should be muted</param>
        /// <param name="deaf">Whether this user should be deafened</param>
        /// <param name="voiceChannelId">Voice channel to move this user to</param>
        /// <param name="communicationDisabledUntil">How long this member should be timed out for. Requires <see cref="Permissions.ModerateMembers"/> permission.</param>
        /// <param name="reason">Reason this user was modified</param>
        public Task ModifyGuildMemberAsync
        (
            ulong guildId,
            ulong userId,
            Optional<string> nick,
            Optional<IEnumerable<ulong>> roleIds,
            Optional<bool> mute,
            Optional<bool> deaf,
            Optional<ulong?> voiceChannelId,
            Optional<DateTimeOffset?> communicationDisabledUntil,
            string reason
        ) => this.ApiClient.ModifyGuildMemberAsync(guildId, userId, nick, roleIds, mute, deaf, voiceChannelId, communicationDisabledUntil, reason);

        /// <summary>
        /// Modifies a member
        /// </summary>
        /// <param name="memberId">Member ID</param>
        /// <param name="guildId">Guild ID</param>
        /// <param name="action">Modifications</param>
        public async Task ModifyAsync(ulong memberId, ulong guildId, Action<MemberEditModel> action)
        {
            var memberEditModel = new MemberEditModel();
            action(memberEditModel);

            if (memberEditModel.VoiceChannel.IsDefined(out var voiceChannel) && voiceChannel!.Type is not ChannelType.Voice or ChannelType.Stage)
                throw new ArgumentException("Given channel is not a voice or stage channel.", nameof(memberEditModel.VoiceChannel));

            if (memberEditModel.Nickname.HasValue && this.CurrentUser.Id == memberId)
            {
                // TODO: Update ModifyCurrentMemberAsync on DiscordApiClient
                await this.ApiClient.ModifyCurrentMemberAsync(
                    guildId,
                    memberEditModel.Nickname.Value,
                    memberEditModel.AuditLogReason
                );

                await this.ApiClient.ModifyGuildMemberAsync(
                    guildId,
                    memberId,
                    Optional.FromNoValue<string>(),
                    memberEditModel.Roles.IfPresent(roles => roles.Select(role => role.Id)),
                    memberEditModel.Muted,
                    memberEditModel.Deafened,
                    memberEditModel.VoiceChannel.IfPresent(voiceChannel => voiceChannel?.Id),
                    default,
                    memberEditModel.AuditLogReason
                );
            }
            else
            {
                await this.ApiClient.ModifyGuildMemberAsync(
                    guildId,
                    memberId,
                    memberEditModel.Nickname,
                    memberEditModel.Roles.IfPresent(roles => roles.Select(role => role.Id)),
                    memberEditModel.Muted,
                    memberEditModel.Deafened,
                    memberEditModel.VoiceChannel.IfPresent(voiceChannel => voiceChannel?.Id),
                    memberEditModel.CommunicationDisabledUntil,
                    memberEditModel.AuditLogReason
                );
            }
        }

        /// <summary>
        /// Changes the current user in a guild.
        /// </summary>
        /// <param name="guildId">Guild ID</param>
        /// <param name="nickname">Nickname to set</param>
        /// <param name="reason">Audit log reason</param>
        public Task ModifyCurrentMemberAsync(ulong guildId, string nickname, string reason)
            => this.ApiClient.ModifyCurrentMemberAsync(guildId, nickname, reason);

        /// <summary>
        /// Searches the given guild for members who's display name start with the specified name.
        /// </summary>
        /// <param name="guildId">The ID of the guild to search.</param>
        /// <param name="name">The name to search for.</param>
        /// <param name="limit">The maximum amount of members to return. Max 1000. Defaults to 1.</param>
        /// <returns>The members found, if any.</returns>
        public Task<IReadOnlyList<DiscordMember>> SearchMembersAsync(ulong guildId, string name, int? limit = 1)
            => this.ApiClient.SearchMembersAsync(guildId, name, limit);

        /// <summary>
        /// Get a guild's prune count.
        /// </summary>
        /// <param name="guildId">Guild ID</param>
        /// <param name="days">Days to check for</param>
        /// <param name="includeRoles">The roles to be included in the prune.</param>
        public Task<int> GetGuildPruneCountAsync(ulong guildId, int days, IEnumerable<ulong> includeRoles)
            => this.ApiClient.GetGuildPruneCountAsync(guildId, days, includeRoles);

        /// <summary>
        /// Begins a guild prune.
        /// </summary>
        /// <param name="guildId">Guild ID</param>
        /// <param name="days">Days to prune for</param>
        /// <param name="computePruneCount">Whether to return the prune count after this method completes. This is discouraged for larger guilds.</param>
        /// <param name="includeRoles">The roles to be included in the prune.</param>
        /// <param name="reason">Reason why this guild was pruned</param>
        public Task<int?> BeginGuildPruneAsync(ulong guildId, int days, bool computePruneCount, IEnumerable<ulong> includeRoles, string reason)
            => this.ApiClient.BeginGuildPruneAsync(guildId, days, computePruneCount, includeRoles, reason);
    }
}
