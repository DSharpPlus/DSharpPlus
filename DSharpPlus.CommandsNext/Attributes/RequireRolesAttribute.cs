// This file is part of the DSharpPlus project.
//
// Copyright (c) 2015 Mike Santiago
// Copyright (c) 2016-2021 DSharpPlus Contributors
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

namespace DSharpPlus.CommandsNext.Attributes
{
    /// <summary>
    /// Defines that usage of this command is restricted to members with specified role. Note that it's much preferred to restrict access using <see cref="RequirePermissionsAttribute"/>.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public sealed class RequireRolesAttribute : CheckBaseAttribute
    {
        /// <summary>
        /// Gets the name of the role required to execute this command.
        /// </summary>
        public IReadOnlyList<string> RoleNames { get; }

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
        }

        public override Task<bool> ExecuteCheckAsync(CommandContext ctx, bool help)
        {
            if (ctx.Guild == null || ctx.Member == null)
                return Task.FromResult(false);

            var rns = ctx.Member.Roles.Select(xr => xr.Name);
            var rnc = rns.Count();
            var ins = rns.Intersect(this.RoleNames, ctx.CommandsNext.GetStringComparer());
            var inc = ins.Count();

            return this.CheckMode switch
            {
                RoleCheckMode.All => Task.FromResult(this.RoleNames.Count == inc),
                RoleCheckMode.SpecifiedOnly => Task.FromResult(rnc == inc),
                RoleCheckMode.None => Task.FromResult(inc == 0),
                _ => Task.FromResult(inc > 0),
            };
        }
    }

    /// <summary>
    /// Specifies how does <see cref="RequireRolesAttribute"/> check for roles.
    /// </summary>
    public enum RoleCheckMode
    {
        /// <summary>
        /// Member is required to have any of the specified roles.
        /// </summary>
        Any,

        /// <summary>
        /// Member is required to have all of the specified roles.
        /// </summary>
        All,

        /// <summary>
        /// Member is required to have exactly the same roles as specified; no extra roles may be present.
        /// </summary>
        SpecifiedOnly,

        /// <summary>
        /// Member is required to have none of the specified roles.
        /// </summary>
        None
    }
}
