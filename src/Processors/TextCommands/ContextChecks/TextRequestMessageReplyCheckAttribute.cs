using System;
using System.Threading.Tasks;
using DSharpPlus.CommandAll.Commands;
using DSharpPlus.CommandAll.ContextChecks;

namespace DSharpPlus.CommandAll.Processors.TextCommands.ContextChecks
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method | AttributeTargets.Delegate)]
    public class TextRequestMessageReplyCheckAttribute(bool require = false) : ContextCheckAttribute
    {
        public bool RequireReplies { get; init; } = require;

        public override Task<bool> ExecuteCheckAsync(CommandContext context) => Task.FromResult(!RequireReplies || context.As<TextContext>().Message.ReferencedMessage is not null);
    }
}
