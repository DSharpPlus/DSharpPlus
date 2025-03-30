using System;

using DSharpPlus.Commands.ContextChecks;

namespace DSharpPlus.Commands;

/// <summary>
/// Indicates whether a command is allowed to execute in DMs, and if so, what DMs.
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public class AllowDmsAttribute : ContextCheckAttribute
{
    /// <summary>
    /// Specifies where a command is allowed to execute.
    /// </summary>
    public DmUsageRule Usage { get; init; }

    /// <summary>
    /// Creates a new instance of this attribute.
    /// </summary>
    public AllowDmsAttribute(DmUsageRule usage = DmUsageRule.AllowBotDms) 
        => this.Usage = usage;
}
