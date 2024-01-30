namespace DSharpPlus.Commands.ContextChecks;

using System;
using System.Threading.Tasks;
using DSharpPlus.Commands.Trees;

/// <summary>
/// Defines that usage of this command is restricted to NSFW channels.
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method | AttributeTargets.Delegate)]
public class RequireNsfwAttribute : ContextCheckAttribute
{
    public override Task<bool> ExecuteCheckAsync(CommandContext context) => Task.FromResult(context.Channel.IsPrivate || context.Channel.IsNSFW || (context.Guild is not null && context.Guild.IsNSFW));
}
