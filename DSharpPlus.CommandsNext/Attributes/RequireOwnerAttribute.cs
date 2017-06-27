using System.Threading.Tasks;

namespace DSharpPlus.CommandsNext.Attributes
{
    public class RequireOwnerAttribute : ConditionBaseAttribute
    {
        public override Task<bool> CanExecute(CommandContext ctx)
        {
            var cfg = ctx.Config;
            var app = ctx.Client.CurrentApplication;
            var me = ctx.Client.CurrentApplication;

            if (cfg.SelfBot)
                return Task.FromResult(ctx.User.Id == me.Id);

            if (app != null)
                return Task.FromResult(ctx.User.Id == app.Owner.Id);

            return Task.FromResult(ctx.User.Id == me.Id);
        }
    }
}
