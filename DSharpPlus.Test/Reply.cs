using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;

namespace DSharpPlus.Test
{
    public class Reply : BaseCommandModule
    {
        [Command, Priority(0)]
        public async Task ReplyAsync(CommandContext ctx, [RemainingText]string response = "")
        {
            _ = response is ""
                ? await ctx.RespondAsync("This is a reply! :)", message_id: ctx.Message.Id)
                : await ctx.RespondAsync($"You requested me to say \"{response}\"", message_id: ctx.Message.Id);
        }

        [Command, Priority(2)]
        public async Task ReplyAsync(CommandContext ctx, DiscordUser user, [RemainingText]string response = "")
        {
            if (ctx.Message.Reference?.Message is null)
                await ctx.RespondAsync("You need to reply to a message for this :(");
            else
            {
                await ctx.RespondAsync(":)", message_id: ctx.Message.Reference.Message.Id, mention: true);
            }
        }
        
        [Command, Priority(1)]
        public async Task ReplyAsync(CommandContext ctx, bool mention, [RemainingText]string response = "")
        {
            _ = response is ""
                ? await ctx.RespondAsync("This is a reply! :)", message_id: ctx.Message.Id, mention: mention)
                : await ctx.RespondAsync($"You requested me to say \"{response}\"", message_id: ctx.Message.Id, mention: mention);
        }
    }
}