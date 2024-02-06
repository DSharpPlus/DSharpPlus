namespace DSharpPlus.Commands.ContextChecks;

using System;
using System.Threading.Tasks;
using DSharpPlus.Commands.Trees;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method | AttributeTargets.Delegate)]
public class RequireNsfwAttribute : ContextCheckAttribute
{
    public override Task<bool> ExecuteCheckAsync(CommandContext context) => Task.FromResult(context.Channel.IsPrivate || context.Channel.IsNSFW || (context.Guild is not null && context.Guild.IsNSFW));
}
