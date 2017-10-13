using System;
using System.Threading.Tasks;
using DSharpPlus.Enums;

namespace DSharpPlus.CommandsNext.Attributes
{
    /// <summary>
    /// Defines that usage of this command is restricted to members with specified permissions. This check also verifies that the bot has the same permissions.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
    public sealed class RequirePermissionsAttribute : CheckBaseAttribute
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
        /// Defines that usage of this command is restricted to members with specified permissions. This check also verifies that the bot has the same permissions.
        /// </summary>
        /// <param name="permissions">Permissions required to execute this command.</param>
        public RequirePermissionsAttribute(Permissions permissions)
        {
            Permissions = permissions;
        }

        public override async Task<bool> CanExecute(CommandContext ctx, bool help)
        {
            if (ctx.Guild == null)
            {
                return IgnoreDms;
            }

            var usr = ctx.Member;
            if (usr == null)
            {
                return false;
            }

            var pusr = ctx.Channel.PermissionsFor(usr);

            var bot = await ctx.Guild.GetMemberAsync(ctx.Client.CurrentUser.Id);
            if (bot == null)
            {
                return false;
            }

            var pbot = ctx.Channel.PermissionsFor(bot);

            var usrok = ctx.Guild.OwnerId == usr.Id;
            var botok = ctx.Guild.OwnerId == bot.Id;

            if (!usrok)
            {
                usrok = (pusr & Permissions.Administrator) != 0 || (pusr & Permissions) == Permissions;
            }

            if (!botok)
            {
                botok = (pbot & Permissions.Administrator) != 0 || (pbot & Permissions) == Permissions;
            }

            return usrok && botok;
        }
    }
}
