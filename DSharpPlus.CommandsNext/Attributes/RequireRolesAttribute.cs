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

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace DSharpPlus.CommandsNext.Attributes
{
    /// <summary>
    /// Defines that usage of this command is restricted to members with specified role. Note that it's much preferred to restrict access using <see cref="RequirePermissionsAttribute"/>.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public sealed class RequireRolesAttribute : CheckBaseAttribute
    {
        /// <summary>
        /// Gets the names of roles required to execute this command.
        /// </summary>
        public IReadOnlyList<string> RoleNames { get; }

        /// <summary>
        /// Gets the IDs of roles required to execute this command.
        /// </summary>
        public IReadOnlyList<ulong> RoleIds { get; }

        /// <summary>
        /// Gets the role checking mode. Refer to <see cref="RoleCheckMode"/> for more information.
        /// </summary>
        public RoleCheckMode CheckMode { get; }

        /// <summary>
        /// Defines that usage of this command is restricted to members with specified role. Note that it's much preferred to restrict access using <see cref="RequirePermissionsAttribute"/>.
        /// </summary>
        /// <param name="checkMode">Role checking mode.</param>
        /// <param name="roleNames">Names of the role to be verified by this check.</param>
        public RequireRolesAttribute(RoleCheckMode checkMode, params string[] roleNames)
        {
            this.CheckMode = checkMode;
            this.RoleNames = new ReadOnlyCollection<string>(roleNames);
            this.RoleIds = new List<ulong>().AsReadOnly();
        }

        /// <summary>
        /// Defines that usage of this command is restricted to members with the specified role.
        /// Note that it is much preferred to restrict access using <see cref="RequirePermissionsAttribute"/>.
        /// </summary>
        /// <param name="checkMode">Role checking mode.</param>
        /// <param name="roleIds">IDs of the roles to be verified by this check.</param>
        public RequireRolesAttribute(RoleCheckMode checkMode, params ulong[] roleIds)
        {
            this.CheckMode = checkMode;
            this.RoleIds = new ReadOnlyCollection<ulong>(roleIds);
            this.RoleNames = new List<string>().AsReadOnly();
        }

        /// <summary>
        /// Defines that usage of this command is restricted to members with the specified role.
        /// Note that it is much preferred to restrict access using <see cref="RequirePermissionsAttribute"/>.
        /// </summary>
        /// <param name="checkMode">Role checking mode.</param>
        /// <param name="roleNames">Names of the role to be verified by this check.</param>
        /// <param name="roleIds">IDs of the roles to be verified by this check.</param>
        public RequireRolesAttribute(RoleCheckMode checkMode, string[] roleNames, ulong[] roleIds)
        {
            this.CheckMode = checkMode;
            this.RoleIds = new ReadOnlyCollection<ulong>(roleIds);
            this.RoleNames = new ReadOnlyCollection<string>(roleNames);
        }

        public override Task<bool> ExecuteCheckAsync(CommandContext ctx, bool help)
        {
            if (ctx.Guild == null || ctx.Member == null)
                return Task.FromResult(false);

            if ((this.CheckMode.HasValue(RoleCheckMode.MatchNames) && !this.CheckMode.HasValue(RoleCheckMode.MatchIds)) || this.RoleIds.Count == 0)
            {
                var roleNames = ctx.Member.Roles.Select(xr => xr.Name);
                var roleNameCount = roleNames.Count();
                var intersectNames = roleNames.Intersect(this.RoleNames, ctx.CommandsNext.GetStringComparer());
                var intersectCount = intersectNames.Count();

                return this.CheckMode switch
                {
                    RoleCheckMode.All => Task.FromResult(this.RoleNames.Count == intersectCount),
                    RoleCheckMode.SpecifiedOnly => Task.FromResult(roleNameCount == intersectCount),
                    RoleCheckMode.None => Task.FromResult(intersectCount == 0),
                    _ => Task.FromResult(intersectCount > 0),
                };
            }
            else if ((!this.CheckMode.HasValue(RoleCheckMode.MatchNames) && this.CheckMode.HasValue(RoleCheckMode.MatchIds)) || this.RoleNames.Count == 0)
            {
                var roleIds = ctx.Member.RoleIds;
                var roleIdCount = roleIds.Count();
                var intersectIds = roleIds.Intersect(this.RoleIds);
                var intersectIdCount = intersectIds.Count();

                return this.CheckMode switch
                {
                    RoleCheckMode.All => Task.FromResult(this.RoleNames.Count == intersectIdCount),
                    RoleCheckMode.SpecifiedOnly => Task.FromResult(roleIdCount == intersectIdCount),
                    RoleCheckMode.None => Task.FromResult(intersectIdCount == 0),
                    _ => Task.FromResult(intersectIdCount > 0),
                };
            }
            else
            {
                var roleIds = ctx.Member.RoleIds
                    .Concat(ctx.Member.Roles.Select(xm => xm.Id));

                var roleNames = ctx.Member.Roles.Select(xm => xm.Name)
                    .Concat(ctx.Member.Roles.Select(xm => xm.Name));

                var intersectIds = roleIds.Intersect(this.RoleIds);
                var intersectNames = roleNames.Intersect(this.RoleNames);

                return this.CheckMode switch
                {
                    RoleCheckMode.All => Task.FromResult(this.RoleNames.Count == intersectNames.Count()
                        && this.RoleIds.Count == intersectIds.Count()),
                    RoleCheckMode.SpecifiedOnly => Task.FromResult(roleIds.Count() == intersectIds.Count()
                        && roleNames.Count() == intersectNames.Count()),
                    RoleCheckMode.None => Task.FromResult(!intersectIds.Any() && !intersectNames.Any()),
                    _ => Task.FromResult(intersectIds.Any() || intersectNames.Any())
                };
            }
        }
    }

    /// <summary>
    /// Specifies how <see cref="RequireRolesAttribute"/> checks for roles.
    /// </summary>
    [Flags]
    public enum RoleCheckMode
    {
        /// <summary>
        /// Member is required to have none of the specified roles.
        /// </summary>
        None = 0,


        /// <summary>
        /// Member is required to have all of the specified roles.
        /// </summary>
        All = 1,

        /// <summary>
        /// Member is required to have any of the specified roles.
        /// </summary>
        Any = 2,

        /// <summary>
        /// Member is required to have exactly the same roles as specified; no extra roles may be present.
        /// </summary>
        SpecifiedOnly = 4,

        /// <summary>
        /// Instructs the check to evaluate for matching role names.
        /// </summary>
        MatchNames = 8,

        /// <summary>
        /// Instructs the check to evaluate for matching role IDs.
        /// </summary>
        MatchIds = 16
    }
}
