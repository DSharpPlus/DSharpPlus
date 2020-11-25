using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;

namespace DSharpPlus.Test
{
    public class Reply : BaseCommandModule
    {
        [Command]
        public async Task ReplyAsync(CommandContext ctx, [RemainingText]string response = "")
        {
            _ = response is ""
                ? await ctx.RespondAsync("This is a reply! :)", message_id: ctx.Message.Id)
                : await ctx.RespondAsync($"You requested me to say {response}", message_id: ctx.Message.Id);
        }
    }
}