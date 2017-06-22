using System.Threading.Tasks;

namespace DSharpPlus.CommandsNext.Attributes
{
    public class RequireOwnerAttribute : ConditionBaseAttribute
    {
        public override Task<bool> CanExecute(CommandContext ctx)
        {
            if (ctx.Config.SelfBot)
                return Task.FromResult(ctx.Message.Author.Id == ctx.Client.CurrentUser.Id);
            return Task.FromResult(ctx.Client.CurrentApplication?.Owner.Id == ctx.User.Id);
        }
    }
}
