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
using System.Threading.Tasks;

namespace DSharpPlus.SlashCommands.Attributes
{
    /// <summary>
    /// Defines that usage of this command is restricted to members with specified permissions.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public sealed class SlashRequireUserPermissionsAttribute : SlashCheckBaseAttribute
    {
        /// <summary>
        /// Gets the permissions required by this attribute.
        /// </summary>
        public Permissions Permissions { get; }

        /// <summary>
        /// Gets or sets this check's behaviour in DMs. True means the check will always pass in DMs, whereas false means that it will always fail.
        /// </summary>
        public bool IgnoreDms { get; } = true;

        /// <summary>
        /// Defines that usage of this command is restricted to members with specified permissions.
        /// </summary>
        /// <param name="permissions">Permissions required to execute this command.</param>
        /// <param name="ignoreDms">Sets this check's behaviour in DMs. True means the check will always pass in DMs, whereas false means that it will always fail.</param>
        public SlashRequireUserPermissionsAttribute(Permissions permissions, bool ignoreDms = true)
        {
            this.Permissions = permissions;
            this.IgnoreDms = ignoreDms;
        }

        /// <summary>
        /// Runs checks.
        /// </summary>
        public override Task<bool> ExecuteChecksAsync(InteractionContext ctx)
        {
            return Task.FromResult(ctx.Guild == null
                ? this.IgnoreDms
                // The member is the guild owner or the member isn't null (should "always" be true) and has the permission in the current channel.
                : ctx.Guild.OwnerId == ctx.Member.Id || (ctx.Member != null && ctx.Channel.PermissionsFor(ctx.Member).HasPermission(this.Permissions)));
        }
    }
}
