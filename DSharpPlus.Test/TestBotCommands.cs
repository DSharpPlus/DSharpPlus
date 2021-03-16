using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;

namespace DSharpPlus.Test
{
    public class TestBotCommands : BaseCommandModule
	{
		public static ConcurrentDictionary<ulong, string> PrefixSettings { get; } = new ConcurrentDictionary<ulong, string>();

		[Command("crosspost")]
		public async Task CrosspostAsync(CommandContext ctx, DiscordChannel chn, DiscordMessage msg)
		{
			var message = await chn.CrosspostMessageAsync(msg).ConfigureAwait(false);
			await ctx.RespondAsync($":ok_hand: Message published: {message.Id}").ConfigureAwait(false);
		}

		[Command("follow")]
		public async Task FollowAsync(CommandContext ctx, DiscordChannel channelToFollow, DiscordChannel targetChannel)
		{
			await channelToFollow.FollowAsync(targetChannel).ConfigureAwait(false);
			await ctx.RespondAsync($":ok_hand: Following channel {channelToFollow.Mention} into {targetChannel.Mention} (Guild: {targetChannel.Guild.Id})").ConfigureAwait(false);
		}

		[Command("setprefix"), Aliases("channelprefix"), Description("Sets custom command prefix for current channel. The bot will still respond to the default one."), RequireOwner]
		public async Task SetPrefixAsync(CommandContext ctx, [Description("The prefix to use for current channel.")] string prefix = null)
		{
			if (string.IsNullOrWhiteSpace(prefix))
				if (PrefixSettings.TryRemove(ctx.Channel.Id, out _))
					await ctx.RespondAsync("👍").ConfigureAwait(false);
				else
					await ctx.RespondAsync("👎").ConfigureAwait(false);
			else
			{
				PrefixSettings.AddOrUpdate(ctx.Channel.Id, prefix, (k, vold) => prefix);
				await ctx.RespondAsync("👍").ConfigureAwait(false);
			}
		}

		[Command("sudo"), Description("Run a command as another user."), Hidden, RequireOwner]
		public async Task SudoAsync(CommandContext ctx, DiscordUser user, [RemainingText] string content)
		{
            var cmd = ctx.CommandsNext.FindCommand(content, out var args);
            var fctx = ctx.CommandsNext.CreateFakeContext(user, ctx.Channel, content, ctx.Prefix, cmd, args);
            await ctx.CommandsNext.ExecuteCommandAsync(fctx).ConfigureAwait(false);
		}

        [Group("bind"), Description("Various argument binder testing commands.")]
        public class Binding : BaseCommandModule
        {
            [Command("message"), Aliases("msg"), Description("Attempts to bind a message.")]
            public Task MessageAsync(CommandContext ctx, DiscordMessage msg)
                => ctx.RespondAsync(embed: new EmbedBuilder()
                    .WithTimestamp(msg.CreationTimestamp)
                    .WithAuthor($"{msg.Author.Username}#{msg.Author.Discriminator}", msg.Author.AvatarUrl)
                    .WithDescription(msg.Content));
		}

        [Command("chainreply")]
        public async Task ChainReplyAsync(CommandContext ctx)
        {
            MessageCreateBuilder builder = new MessageCreateBuilder();

            StringBuilder contentBuilder = new StringBuilder();

            ulong reply = ctx.Message.Id;
            bool ping = false;

            if (ctx.Message.MessageType == MessageType.Reply)
            {
                contentBuilder.AppendLine("Chaining a reply");
                reply = ctx.Message.ReferencedMessage.Id;
                if (ping = ctx.Message.MentionedUsers.Contains(ctx.Message.ReferencedMessage.Author))
                {
                    contentBuilder.AppendLine("Pinging the user with the reply as it appears that is what you did.");
                }
                else
                {
                    contentBuilder.AppendLine("Not pinging the user as that does not appear to be what you did.");
                }
            }
            else
            {
                contentBuilder.AppendLine("I mean, ok, you just tried to chain a non-existent reply but whatever.");
            }

            builder
                .WithContent(contentBuilder.ToString())
                .WithReply(reply, ping);

            await ctx.RespondAsync(builder);
        }

        [Command("mentionusers")]
        public async Task MentionAllMentionedUsers(CommandContext ctx, [RemainingText][Description("Just a string so that DSharpPlus will parse no matter what I say")] string mentionedUsers)
        {
            var content = "You didn't have any users to mention";
            if (ctx.Message.MentionedUsers.Any())
                content = string.Join(", ", ctx.Message.MentionedUsers.Select(usr => usr.Mention));

            await ctx.RespondAsync(content);
        }

        [Command("mentionroles")]
        public async Task MentionAllMentionedRoles(CommandContext ctx, [RemainingText][Description("Just a string so that DSharpPlus will parse no matter what I say")] string mentionedRoles)
        {
            var content = "You didn't have any roles to mention";
            if (ctx.Message.MentionedRoles.Any())
                content = string.Join(", ", ctx.Message.MentionedRoles.Select(role => role.Mention));

            await ctx.RespondAsync(content);
        }

        [Command("mentionchannels")]
        public async Task MentionChannels(CommandContext ctx, [RemainingText][Description("Just a string so that DSharpPlus will parse no matter what I say")] string mentionedChannels)
        {
            var content = "You didn't have any channels to mention";
            if (ctx.Message.MentionedChannels.Any())
                content = string.Join(", ", ctx.Message.MentionedChannels.Select(role => role.Mention));

            await ctx.RespondAsync(content);
        }

        [Command("getmessagementions")]
        public async Task GetMessageMentions(CommandContext ctx, ulong msgId)
        {
            var msg = await ctx.Channel.GetMessageAsync(msgId);
            var contentBuilder = new StringBuilder("You didn't mention any user, channel, or role.");

            if (msg.MentionedUsers.Any() || msg.MentionedRoles.Any() || msg.MentionedChannels.Any())
            {
                contentBuilder.Clear();
                contentBuilder.AppendLine(msg.MentionedUsers.Any() ? string.Join(", ", msg.MentionedUsers.Select(usr => usr.Mention)) : string.Empty);
                contentBuilder.AppendLine(msg.MentionedChannels.Any() ? string.Join(", ", msg.MentionedChannels.Select(usr => usr.Mention)) : string.Empty);
                contentBuilder.AppendLine(msg.MentionedRoles.Any() ? string.Join(", ", msg.MentionedRoles.Select(usr => usr.Mention)) : string.Empty);
            }

            await ctx.RespondAsync(contentBuilder.ToString());
        }
    }
}
