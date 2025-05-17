using System;
using DSharpPlus.Commands.ContextChecks;

namespace DSharpPlus.Commands.Processors.TextCommands.ContextChecks;

[AttributeUsage(AttributeTargets.Parameter)]
public class TextMessageReplyAttribute(bool require = false) : ContextCheckAttribute
{
    public bool RequiresReply { get; init; } = require;
}
