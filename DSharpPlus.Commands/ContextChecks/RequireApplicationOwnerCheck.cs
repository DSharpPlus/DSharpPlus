using System.Linq;
using System.Threading.Tasks;

namespace DSharpPlus.Commands.ContextChecks;

internal sealed class RequireApplicationOwnerCheck : IContextCheck<RequireApplicationOwnerAttribute>
{
    public ValueTask<string?> ExecuteCheckAsync(RequireApplicationOwnerAttribute attribute, CommandContext context) =>
        ValueTask.FromResult(context.Client.CurrentApplication.Owners?.Contains(context.User) == true || context.User.Id == context.Client.CurrentUser.Id
            ? null
            : "This command must be executed by an owner of the application."
        );
}
