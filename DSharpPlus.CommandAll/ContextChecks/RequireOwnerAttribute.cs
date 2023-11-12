namespace DSharpPlus.CommandAll.ContextChecks;

using System;
using System.Linq;
using System.Threading.Tasks;
using DSharpPlus.CommandAll.Commands;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method | AttributeTargets.Delegate)]
public class RequireOwnerAttribute : ContextCheckAttribute
{
    public override Task<bool> ExecuteCheckAsync(CommandContext context) => Task.FromResult(context.Client.CurrentApplication.Owners?.Contains(context.User) ?? context.User.Id == context.Client.CurrentUser.Id);
}
