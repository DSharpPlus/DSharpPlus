namespace DSharpPlus.Commands.Processors.TextCommands.ContextChecks;

using System.Threading.Tasks;
using DSharpPlus.Commands;
using DSharpPlus.Commands.ContextChecks;

internal sealed class TextMessageReplyCheck : IContextCheck<TextMessageReplyAttribute>
{
    public ValueTask<string?> ExecuteCheckAsync(TextMessageReplyAttribute attribute, CommandContext context)
    {
        return ValueTask.FromResult
        (
            !attribute.RequireReplies || context.As<TextCommandContext>().Message.ReferencedMessage is not null
                ? null
                : "This command requires to be used in reply to a message."
        );
    }
}
