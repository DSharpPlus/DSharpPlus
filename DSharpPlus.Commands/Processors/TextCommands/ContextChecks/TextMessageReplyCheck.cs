using System.Threading.Tasks;
using DSharpPlus.Commands.ContextChecks;

namespace DSharpPlus.Commands.Processors.TextCommands.ContextChecks;

internal sealed class TextMessageReplyCheck : IContextCheck<TextMessageReplyAttribute>
{
    public ValueTask<string?> ExecuteCheckAsync(TextMessageReplyAttribute attribute, CommandContext context) =>
        ValueTask.FromResult(!attribute.RequiresReply || context.As<TextCommandContext>().Message.ReferencedMessage is not null
            ? null
            : "This command requires to be used in reply to a message."
        );
}
