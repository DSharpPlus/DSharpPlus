using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DSharpPlus.CommandsNext.Attributes
{
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, AllowMultiple = true, Inherited = true)]
    public sealed class RequirePermissionsAttribute : ConditionBaseAttribute
    {
        private static Dictionary<string, int> PermissionOrder { get; set; }

        /// <summary>
        /// Gets the permissions required by this attribute.
        /// </summary>
        public Permissions Permissions { get; private set; }

        /// <summary>
        /// Gets or sets this check's behaviour in DMs. True means the check will always pass in DMs, whereas false means that it will always fail.
        /// </summary>
        public bool IgnoreDms { get; set; } = true;

        public RequirePermissionsAttribute(Permissions permissions)
        {
            this.Permissions = permissions;
        }

        static RequirePermissionsAttribute()
        {
            PermissionOrder = new Dictionary<string, int>()
            {
                { "role", 2 },
                { "user", 1 }
            };
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
