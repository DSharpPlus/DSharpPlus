using System;
using System.Linq;
using System.Threading.Tasks;

namespace DSharpPlus.CommandsNext.Attributes
{
    /// <summary>
    /// Defines that usage of this command is restricted to the owner of the bot.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public sealed class RequireOwnerAttribute : CheckBaseAttribute
    {
        public override Task<bool> ExecuteCheckAsync(CommandContext ctx, bool help)
        {
            var app = ctx.Client.CurrentApplication;
            var me = ctx.Client.CurrentUser;

            return app != null ? Task.FromResult(app.Owners.Any(x => x.Id == ctx.User.Id)) : Task.FromResult(ctx.User.Id == me.Id);
        }
    }
}
