// This file is part of the DSharpPlus project.
//
// Copyright (c) 2015 Mike Santiago
// Copyright (c) 2016-2022 DSharpPlus Contributors
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

using Newtonsoft.Json;

namespace DSharpPlus.Entities
{
    /// <summary>
    /// Represents a permission for a application command.
    /// </summary>
    public class DiscordApplicationCommandPermission
    {
        /// <summary>
        /// The id of the role or the user this permission is for.
        /// </summary>
        [JsonProperty("id")]
        public ulong Id { get; internal set; }

        /// <summary>
        /// Gets the type of the permission.
        /// </summary>
        [JsonProperty("type")]
        public ApplicationCommandPermissionType Type { get; internal set; }

        /// <summary>
        /// Gets whether the command is enabled for the role or user.
        /// </summary>
        [JsonProperty("permission")]
        public bool Permission { get; internal set; }

        /// <summary>
        /// Represents a permission for a application command.
        /// </summary>
        /// <param name="role">The role to construct the permission for.</param>
        /// <param name="permission">Whether the command should be enabled for the role.</param>
        public DiscordApplicationCommandPermission(DiscordRole role, bool permission)
        {
            this.Id = role.Id;
            this.Type = ApplicationCommandPermissionType.Role;
            this.Permission = permission;
        }

        /// <summary>
        /// Represents a permission for a application command.
        /// </summary>
        /// <param name="member">The member to construct the permission for.</param>
        /// <param name="permission">Whether the command should be enabled for the role.</param>
        public DiscordApplicationCommandPermission(DiscordMember member, bool permission)
        {
            this.Id = member.Id;
            this.Type = ApplicationCommandPermissionType.User;
            this.Permission = permission;
        }

        internal DiscordApplicationCommandPermission() { }
    }
}
