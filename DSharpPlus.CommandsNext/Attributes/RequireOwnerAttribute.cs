using System;
using System.Threading.Tasks;

namespace DSharpPlus.CommandsNext.Attributes
{
    /// <summary>
    /// Defines that usage of this command is restricted to the owner of the bot.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
    public sealed class RequireOwnerAttribute : CheckBaseAttribute
    {
        public override Task<bool> CanExecute(CommandContext ctx, bool help)
        {
            var cfg = ctx.Config;
            var app = ctx.Client.CurrentApplication;
            var me = ctx.Client.CurrentUser;

            if (cfg.SelfBot)
                return Task.FromResult(ctx.User.Id == me.Id);

            if (app != null)
                return Task.FromResult(ctx.User.Id == app.Owner.Id);

            return Task.FromResult(ctx.User.Id == me.Id);
        }
    }
}
