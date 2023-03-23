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
using System.IO;
using System.Threading.Tasks;
using DSharpPlus.Entities;
using DSharpPlus.Net.Abstractions;
using DSharpPlus.Net.Models;

namespace DSharpPlus
{
    public sealed partial class DiscordRestClient
    {
        /// <summary>
        /// Updates a role's position
        /// </summary>
        /// <param name="guildId">Guild ID</param>
        /// <param name="roleId">Role ID</param>
        /// <param name="position">Role position</param>
        /// <param name="reason">Reason this position was modified</param>
        public Task UpdateRolePositionAsync(ulong guildId, ulong roleId, int position, string? reason = null) => this.ApiClient.ModifyGuildRolePositionAsync
        (
            guildId,
            new List<RestGuildRoleReorderPayload>() { new RestGuildRoleReorderPayload { RoleId = roleId, Position = position } },
            reason
        );

        /// <summary>
        /// Gets roles
        /// </summary>
        /// <param name="guildId">Guild ID</param>
        public Task<IReadOnlyList<DiscordRole>> GetGuildRolesAsync(ulong guildId)
            => this.ApiClient.GetGuildRolesAsync(guildId);

        /// <summary>
        /// Modifies a role
        /// </summary>
        /// <param name="guildId">Guild ID</param>
        /// <param name="roleId">Role ID</param>
        /// <param name="name">New role name</param>
        /// <param name="permissions">New role permissions</param>
        /// <param name="color">New role color</param>
        /// <param name="hoist">Whether this role should be hoisted</param>
        /// <param name="mentionable">Whether this role should be mentionable</param>
        /// <param name="reason">Why this role was modified</param>
        /// <param name="icon">The icon to add to this role</param>
        /// <param name="emoji">The emoji to add to this role. Must be unicode.</param>
        public Task<DiscordRole> ModifyGuildRoleAsync
        (
            ulong guildId,
            ulong roleId,
            string name,
            Permissions? permissions,
            DiscordColor? color,
            bool? hoist,
            bool? mentionable,
            string reason,
            Stream icon,
            DiscordEmoji emoji
        ) => this.ApiClient.ModifyGuildRoleAsync
        (
            guildId,
            roleId,
            name,
            permissions,
            color.HasValue ? color.Value.Value : null,
            hoist,
            mentionable,
            reason,
            icon,
            emoji?.ToString()
        );

        /// <summary>
        /// Modifies a role
        /// </summary>
        /// <param name="roleId">Role ID</param>
        /// <param name="guildId">Guild ID</param>
        /// <param name="action">Modifications</param>
        public Task ModifyGuildRoleAsync(ulong roleId, ulong guildId, Action<RoleEditModel> action)
        {
            var roleEditModel = new RoleEditModel();
            action(roleEditModel);
            return this.ModifyGuildRoleAsync(
                guildId,
                roleId,
                roleEditModel.Name,
                roleEditModel.Permissions,
                roleEditModel.Color,
                roleEditModel.Hoist,
                roleEditModel.Mentionable,
                roleEditModel.AuditLogReason,
                roleEditModel.Icon,
                roleEditModel.Emoji
            );
        }

        /// <summary>
        /// Deletes a role
        /// </summary>
        /// <param name="guildId">Guild ID</param>
        /// <param name="roleId">Role ID</param>
        /// <param name="reason">Reason why this role was deleted</param>
        public Task DeleteGuildRoleAsync(ulong guildId, ulong roleId, string reason)
            => this.ApiClient.DeleteRoleAsync(guildId, roleId, reason);

        /// <summary>
        /// Creates a new role
        /// </summary>
        /// <param name="guildId">Guild ID</param>
        /// <param name="name">Role name</param>
        /// <param name="permissions">Role permissions</param>
        /// <param name="color">Role color</param>
        /// <param name="hoist">Whether this role should be hoisted</param>
        /// <param name="mentionable">Whether this role should be mentionable</param>
        /// <param name="reason">Reason why this role was created</param>
        /// <param name="icon">The icon to add to this role</param>
        /// <param name="emoji">The emoji to add to this role. Must be unicode.</param>
        public Task<DiscordRole> CreateGuildRoleAsync
        (
            ulong guildId,
            string name,
            Permissions? permissions,
            int? color,
            bool? hoist,
            bool? mentionable,
            string reason,
            Stream? icon = null,
            DiscordEmoji? emoji = null
        ) => this.ApiClient.CreateGuildRoleAsync
        (
            guildId,
            name,
            permissions,
            color,
            hoist,
            mentionable,
            reason,
            icon,
            emoji?.ToString()
        );
    }
}
