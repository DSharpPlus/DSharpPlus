namespace DSharpPlus.Commands.Processors.TextCommands.ContextChecks;

using System;
using DSharpPlus.Commands.ContextChecks;

[AttributeUsage(AttributeTargets.Parameter)]
public class TextMessageReplyAttribute(bool require = false) : ContextCheckAttribute
{
    public bool RequireReplies { get; init; } = require;
}
