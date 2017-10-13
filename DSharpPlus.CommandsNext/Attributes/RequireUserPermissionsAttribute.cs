using System;
using System.Threading.Tasks;
using DSharpPlus.Enums;

namespace DSharpPlus.CommandsNext.Attributes
{
    /// <summary>
    /// Defines that usage of this command is restricted to members with specified permissions.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
    public sealed class RequireUserPermissionsAttribute : CheckBaseAttribute
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
        public RequireUserPermissionsAttribute(Permissions permissions)
        {
            Permissions = permissions;
        }

        public override Task<bool> CanExecute(CommandContext ctx, bool help)
        {
            if (ctx.Guild == null)
            {
                return Task.FromResult(IgnoreDms);
            }

            var usr = ctx.Member;
            if (usr == null)
            {
                return Task.FromResult(false);
            }

            if (usr.Id == ctx.Guild.OwnerId)
            {
                return Task.FromResult(true);
            }

            var pusr = ctx.Channel.PermissionsFor(usr);

            if ((pusr & Permissions.Administrator) != 0)
            {
                return Task.FromResult(true);
            }

            if ((pusr & Permissions) == Permissions)
            {
                return Task.FromResult(true);
            }

            return Task.FromResult(false);
        }
    }
}
