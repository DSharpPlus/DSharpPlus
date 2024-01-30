using System;
using System.Threading.Tasks;
using DSharpPlus.CommandAll.Commands;
using DSharpPlus.CommandAll.ContextChecks;

namespace DSharpPlus.CommandAll.Processors.TextCommands.ContextChecks
{
    [AttributeUsage(AttributeTargets.Parameter)]
    public class TextMessageReplyAttribute(bool require = false) : ContextCheckAttribute
    {
        public bool RequireReplies { get; init; } = require;

        public override Task<bool> ExecuteCheckAsync(CommandContext context) => Task.FromResult(!RequireReplies || context.As<TextCommandContext>().Message.ReferencedMessage is not null);
    }
}
