using System;
using System.Threading.Tasks;

namespace DSharpPlus.CommandsNext.Attributes
{
    /// <summary>
    /// Defines that usage of this command is restricted to members with specified permissions.
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
        /// Defines that usage of this command is restricted to members with specified permissions.
        /// </summary>
        /// <param name="permissions">Permissions required to execute this command.</param>
        public RequirePermissionsAttribute(Permissions permissions)
        {
            this.Permissions = permissions;
        }

        public override async Task<bool> CanExecute(CommandContext ctx)
        {
            if (ctx.Guild == null)
                return this.IgnoreDms;

            var usr = ctx.Member;
            if (usr == null)
                return false;
            var pusr = ctx.Channel.PermissionsFor(usr);

            var bot = await ctx.Guild.GetMemberAsync(ctx.Client.CurrentUser.Id);
            if (bot == null)
                return false;
            var pbot = ctx.Channel.PermissionsFor(bot);

            if ((pusr & this.Permissions) == this.Permissions && (pbot & this.Permissions) == this.Permissions)
                return true;

            return false;
        }
    }
}
