using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using cn = DSharpPlus.CommandsNext;

namespace DSharpPlus.CommandsNext.Attributes
{
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, AllowMultiple = true, Inherited = true)]
    public sealed class RequiredPermissionsAttribute : ConditionBaseAttribute
    {
        private static Dictionary<string, int> PermissionOrder { get; set; }

        public cn.Permission Permissions { get; private set; }

        public RequiredPermissionsAttribute(cn.Permission permissions)
        {
            this.Permissions = permissions;
        }

        static RequiredPermissionsAttribute()
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
                return true;

            var usr = ctx.Member;
            var bot = await ctx.Guild.GetMemberAsync(ctx.Client.CurrentUser.Id);
            if (bot == null || usr == null)
                return false;

            // user > role > everyone
            // allow > deny > undefined
            // =>
            // user allow > user deny > role allow > role deny > everyone allow > everyone deny
            // thanks to meew0
            return true;/*

            var ou = ctx.Channel.PermissionOverwrites.FirstOrDefault(xo => xo.ID == usr.User.ID);
            if (ou != null)
            {
                
            }

            var os = ctx.Channel.PermissionOverwrites.Select(xo => xo.ID);
            var roles = ctx.Guild.Roles
                .Where(xr => os.Contains(xr.ID))
                .OrderByDescending(xr => xr.Position);*/
        }
    }
}
