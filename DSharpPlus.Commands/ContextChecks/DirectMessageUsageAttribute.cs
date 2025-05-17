using System;

namespace DSharpPlus.Commands.ContextChecks;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method | AttributeTargets.Delegate)]
public class DirectMessageUsageAttribute(DirectMessageUsage usage = DirectMessageUsage.AllowDMs) : ContextCheckAttribute
{
    public DirectMessageUsage Usage { get; init; } = usage;
}

public enum DirectMessageUsage
{
    AllowDMs,
    DenyDMs,
    RequireDMs
}
