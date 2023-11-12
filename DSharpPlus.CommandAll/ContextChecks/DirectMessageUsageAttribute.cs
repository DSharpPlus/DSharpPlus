namespace DSharpPlus.CommandAll.ContextChecks;

using System;
using System.Threading.Tasks;
using DSharpPlus.CommandAll.Commands;

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
