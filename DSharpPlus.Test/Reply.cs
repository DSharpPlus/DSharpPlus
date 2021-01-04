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
                ? await new DiscordMessageBuilder()
                    .WithContent("This is a reply! :)")
                    .WithReply(ctx.Message.Id, true)
                    .SendAsync(ctx.Channel)
                : await new DiscordMessageBuilder()
                    .WithContent($"You requested me to say \"{response}\"")
                    .WithReply(ctx.Message.Id, true)
                    .SendAsync(ctx.Channel);
        }

        [Command, Priority(2)]
        public async Task ReplyAsync(CommandContext ctx, DiscordUser user, [RemainingText]string response = "")
        {
            if (ctx.Message.Reference?.Message is null)
                await ctx.RespondAsync("You need to reply to a message for this :(");
            else
            {
                await new DiscordMessageBuilder()
                    .WithContent(":)")
                    .WithReply(ctx.Message.Reference.Message.Id, true)
                    .SendAsync(ctx.Channel);
            }
        }
        
        [Command, Priority(1)]
        public async Task ReplyAsync(CommandContext ctx, bool mention, [RemainingText]string response = "")
        {
            _ = response is ""
               ? await new DiscordMessageBuilder()
                   .WithContent("This is a reply! :)")
                   .WithReply(ctx.Message.Id, mention)
                   .SendAsync(ctx.Channel)
               : await new DiscordMessageBuilder()
                   .WithContent($"You requested me to say \"{response}\"")
                   .WithReply(ctx.Message.Id, mention)
                   .SendAsync(ctx.Channel);
        }
    }
}