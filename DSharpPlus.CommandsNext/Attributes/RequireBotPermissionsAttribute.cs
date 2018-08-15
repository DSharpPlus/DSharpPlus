﻿using System;
using System.Threading.Tasks;

namespace DSharpPlus.CommandsNext.Attributes
{
    /// <summary>
    /// Defines that usage of this command is only possible when the bot is granted a specific permission.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public sealed class RequireBotPermissionsAttribute : CheckBaseAttribute
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
        /// Defines that usage of this command is only possible when the bot is granted a specific permission.
        /// </summary>
        /// <param name="permissions">Permissions required to execute this command.</param>
        public RequireBotPermissionsAttribute(Permissions permissions)
        {
            this.Permissions = permissions;
        }

        public override async Task<bool> ExecuteCheckAsync(CommandContext ctx, bool help)
        {
            if (ctx.Guild == null)
                return this.IgnoreDms;

            var bot = await ctx.Guild.GetMemberAsync(ctx.Client.CurrentUser.Id).ConfigureAwait(false);
            if (bot == null)
                return false;

            if (bot.Id == ctx.Guild.OwnerId)
                return true;

            var pbot = ctx.Channel.PermissionsFor(bot);

            if ((pbot & Permissions.Administrator) != 0)
                return true;

            if ((pbot & this.Permissions) == this.Permissions)
                return true;

            return false;
        }
    }
}
