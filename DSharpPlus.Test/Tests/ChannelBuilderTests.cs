using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using System.Threading.Tasks;

namespace DSharpPlus.Test.Tests
{
    public class ChannelBuilderTests : BaseCommandModule
    {
        [Command]
        public async Task CreateCategory(CommandContext ctx, [RemainingText] string name)
        {
            var builder = await new DiscordChannelCreateBuilder()
                .WithType(ChannelType.Category)
                .WithName(name)
                .CreateAsync(ctx.Guild);
        }

        [Command]
        public async Task CreateTextChannel(CommandContext ctx, [RemainingText] string name)
        {
            await ctx.Guild.CreateChannelAsync(x =>
            {
                x.WithName(name)
                .WithType(ChannelType.Text)
                .WithRateLimit(25)
                .WithTopic("This is an awesome topic");
            });
        }

        [Command]
        public async Task CreateVoiceChannel(CommandContext ctx, [RemainingText] string name)
        {
            await ctx.Guild.CreateChannelAsync(x =>
            {
                x.WithName(name)
                .WithType(ChannelType.Voice)
                .WithRateLimit(25);
            });
        }

        [Command]
        public async Task MoveChannelToCategory(CommandContext ctx, DiscordChannel category, DiscordChannel channel)
        {
            await channel.ModifyAsync(x =>
            {
                x.WithParentId(category.Id);
            });
        }
    }
}
