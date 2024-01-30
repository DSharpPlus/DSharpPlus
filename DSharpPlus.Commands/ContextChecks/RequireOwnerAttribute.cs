namespace DSharpPlus.Commands.ContextChecks;

using System;
using System.Linq;
using System.Threading.Tasks;
using DSharpPlus.Commands.Trees;

/// <summary>
/// Defines that usage of this command is restricted to the owner of the bot.
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method | AttributeTargets.Delegate)]
public class RequireOwnerAttribute : ContextCheckAttribute
{
    public override Task<bool> ExecuteCheckAsync(CommandContext context) => Task.FromResult(context.Client.CurrentApplication.Owners?.Contains(context.User) ?? context.User.Id == context.Client.CurrentUser.Id);
}
