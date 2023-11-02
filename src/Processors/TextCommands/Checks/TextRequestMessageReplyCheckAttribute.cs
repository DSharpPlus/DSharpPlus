using System;
using System.Threading.Tasks;
using DSharpPlus.CommandAll.Checks;
using DSharpPlus.CommandAll.Commands;

namespace DSharpPlus.CommandAll.Processors.TextCommands.Checks
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method | AttributeTargets.Delegate)]
    public class TextRequestMessageReplyCheckAttribute(bool require = false) : ContextCheckAttribute
    {
        public bool RequireReplies { get; init; } = require;

        public override Task<bool> ExecuteCheckAsync(CommandContext context) => Task.FromResult(!RequireReplies || context.As<TextContext>().Message.ReferencedMessage is not null);
    }
}
