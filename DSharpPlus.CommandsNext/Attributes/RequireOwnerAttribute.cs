using System.Threading.Tasks;

namespace DSharpPlus.CommandsNext.Attributes
{
    public class RequireOwnerAttribute : ConditionBaseAttribute
    {
        public override Task<bool> CanExecute(CommandContext ctx)
        {
            if (ctx.Client.CurrentApplication.Owner.Id == ctx.User.Id)
                return Task.FromResult(true);
            return Task.FromResult(false);
        }
    }
}
