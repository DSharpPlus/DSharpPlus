namespace DSharpPlus.Commands.ContextChecks;

using System;
using System.Threading.Tasks;
using DSharpPlus.Commands.Trees;
/// <summary>
/// Gets the permissions required by this attribute.
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method | AttributeTargets.Delegate)]
public class DirectMessageUsageAttribute(DirectMessageUsage usage = DirectMessageUsage.AllowDMs) : ContextCheckAttribute
{
    public DirectMessageUsage Usage { get; init; } = usage;

    public override Task<bool> ExecuteCheckAsync(CommandContext context) => Task.FromResult(!context.Channel.IsPrivate || this.Usage switch
    {
        DirectMessageUsage.AllowDMs => true,
        DirectMessageUsage.DenyDMs => false,
        DirectMessageUsage.RequireDMs => context.Channel.IsPrivate,
        _ => false,
    });
}

public enum DirectMessageUsage
{
    AllowDMs,
    DenyDMs,
    RequireDMs
}
