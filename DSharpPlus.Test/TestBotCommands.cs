using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
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
                => ctx.RespondAsync(embed: new DiscordEmbedBuilder()
                    .WithTimestamp(msg.CreationTimestamp)
                    .WithAuthor($"{msg.Author.Username}#{msg.Author.Discriminator}", msg.Author.AvatarUrl)
                    .WithDescription(msg.Content));
		}


        [Command("mention"), Description("Attempts to mention a user")]
        public async Task MentionablesAsync(CommandContext ctx, DiscordUser user)
        {
            string content = $"Hey, {user.Mention}! Listen!";
            await ctx.Channel.SendMessageAsync("✔ should ping, ❌ should not ping.").ConfigureAwait(false);                                                                                           

            await ctx.Channel.SendMessageAsync("✔ Default Behaviour: " + content).ConfigureAwait(false);                                                                                            //Should ping User

            await new DiscordMessageBuilder()
                .WithContent("✔ UserMention(user): " + content)
                .WithAllowedMentions(new IMention[] { new UserMention(user) })
                .SendAsync(ctx.Channel)
                .ConfigureAwait(false);                                                                                                                      //Should ping user

            await new DiscordMessageBuilder()
                .WithContent("✔ UserMention(): " + content)
                .WithAllowedMentions(new IMention[] { new UserMention() })
                .SendAsync(ctx.Channel)
                .ConfigureAwait(false);                                                                                                                      //Should ping user

            await new DiscordMessageBuilder()
                .WithContent("✔ User Mention Everyone & Self: " + content)
                .WithAllowedMentions(new IMention[] { new UserMention(), new UserMention(user) })
                .SendAsync(ctx.Channel)
                .ConfigureAwait(false);                                                                                                                      //Should ping user


            await new DiscordMessageBuilder()
               .WithContent("✔ UserMention.All: " + content)
               .WithAllowedMentions(new IMention[] { UserMention.All })
               .SendAsync(ctx.Channel)
               .ConfigureAwait(false);                                                                                                                       //Should ping user

            await new DiscordMessageBuilder()
               .WithContent("❌ Empty Mention Array: " + content)
               .WithAllowedMentions(new IMention[0])
               .SendAsync(ctx.Channel)
               .ConfigureAwait(false);                                                                                                                       //Should ping no one

            await new DiscordMessageBuilder()
               .WithContent("❌ UserMention(SomeoneElse): " + content)
               .WithAllowedMentions(new IMention[] { new UserMention(545836271960850454L) })
               .SendAsync(ctx.Channel)
               .ConfigureAwait(false);                                                                                                                       //Should ping no one (@user was not pinged)

            await new DiscordMessageBuilder()
               .WithContent("❌ Everyone():" + content)
               .WithAllowedMentions(new IMention[] { new EveryoneMention() })
               .SendAsync(ctx.Channel)
               .ConfigureAwait(false);                                                                                                                       //Should ping no one (@everyone was not pinged)
        }

        [Command("editMention"), Description("Attempts to mention a user via edit message")]
        public async Task EditMentionablesAsync(CommandContext ctx, DiscordUser user)
        {
            string origcontent = $"Hey, silly! Listen!";
            string newContent = $"Hey, {user.Mention}! Listen!";

            await ctx.Channel.SendMessageAsync("✔ should ping, ❌ should not ping.").ConfigureAwait(false);

            var test1Msg = await ctx.Channel.SendMessageAsync("✔ Default Behaviour: " + origcontent).ConfigureAwait(false);
            await new DiscordMessageBuilder()
               .WithContent("✔ Default Behaviour: " + newContent)
               .ModifyAsync(test1Msg)
               .ConfigureAwait(false);                                                                                                                               //Should ping User

            var test2Msg = await ctx.Channel.SendMessageAsync("✔ UserMention(user): " + origcontent).ConfigureAwait(false);      
            await new DiscordMessageBuilder()
               .WithContent("✔ UserMention(user): " + newContent)
               .WithAllowedMentions(new IMention[] { new UserMention(user) })
               .ModifyAsync(test2Msg)
               .ConfigureAwait(false);                                                                                                                               //Should ping user

            var test3Msg = await ctx.Channel.SendMessageAsync("✔ UserMention(): " + origcontent).ConfigureAwait(false);
            await new DiscordMessageBuilder()
               .WithContent("✔ UserMention(): " + newContent)
               .WithAllowedMentions(new IMention[] { new UserMention() })
               .ModifyAsync(test3Msg)
               .ConfigureAwait(false);                                                                                                                               //Should ping user

            var test4Msg = await ctx.Channel.SendMessageAsync("✔ User Mention Everyone & Self: " + origcontent).ConfigureAwait(false);
            await new DiscordMessageBuilder()
               .WithContent("✔ User Mention Everyone & Self: " + newContent)
               .WithAllowedMentions(new IMention[] { new UserMention(), new UserMention(user) })
               .ModifyAsync(test4Msg)
               .ConfigureAwait(false);                                                                                                                               //Should ping user

            var test5Msg = await ctx.Channel.SendMessageAsync("✔ UserMention.All: " + origcontent).ConfigureAwait(false);
            await new DiscordMessageBuilder()
               .WithContent("✔ UserMention.All: " + newContent)
               .WithAllowedMentions(new IMention[] { UserMention.All })
               .ModifyAsync(test5Msg)
               .ConfigureAwait(false);                                                                                                                               //Should ping user

            var test6Msg = await ctx.Channel.SendMessageAsync("❌ Empty Mention Array: " + origcontent).ConfigureAwait(false);
            await new DiscordMessageBuilder()
               .WithContent("❌ Empty Mention Array: " + newContent)
               .WithAllowedMentions(new IMention[0])
               .ModifyAsync(test6Msg)
               .ConfigureAwait(false);                                                                                                                               //Should ping no one

            var test7Msg = await ctx.Channel.SendMessageAsync("❌ UserMention(SomeoneElse): " + origcontent).ConfigureAwait(false);
            await new DiscordMessageBuilder()
               .WithContent("❌ UserMention(SomeoneElse): " + newContent)
               .WithAllowedMentions(new IMention[] { new UserMention(777677298316214324) })
               .ModifyAsync(test7Msg)
               .ConfigureAwait(false);                                                                                                                               //Should ping no one (@user was not pinged)

            var test8Msg = await ctx.Channel.SendMessageAsync("❌ Everyone(): " + origcontent).ConfigureAwait(false);
            await new DiscordMessageBuilder()
               .WithContent("❌ Everyone(): " + newContent)
               .WithAllowedMentions(new IMention[] { new EveryoneMention() })
               .ModifyAsync(test8Msg)
               .ConfigureAwait(false);                                                                                                                               //Should ping no one (@everyone was not pinged)
        }

        [Command("SendSomeFile")]
        public async Task SendSomeFile(CommandContext ctx)
        {
            using (var fs = new FileStream("ADumbFile.txt", FileMode.Open, FileAccess.Read))
            {
                await new DiscordMessageBuilder()
                    .WithContent("Here is a really dumb file that i am testing with.")
                    .WithFiles(new Dictionary<string, Stream>() { { "ADumbFile1.txt", fs } })
                    .SendAsync(ctx.Channel)
                    .ConfigureAwait(false);

                fs.Position = 0;

                await new DiscordMessageBuilder()
                    .WithContent("Here is a really dumb file that i am testing with.")
                    .WithFile(fs)
                    .SendAsync(ctx.Channel)
                    .ConfigureAwait(false);

                fs.Position = 0;

                await new DiscordMessageBuilder()
                    .WithContent("Here is a really dumb file that i am testing with.")
                    .WithFile("ADumbFile2.txt", fs)
                    .SendAsync(ctx.Channel)
                    .ConfigureAwait(false);               
            }

            await new DiscordMessageBuilder()
                   .WithContent("Here is a really dumb file that i am testing with.")
                   .WithFile("./ADumbFile.txt")
                   .SendAsync(ctx.Channel)
                   .ConfigureAwait(false);
        }

        [Command("CreateSomeFile")]
        public async Task CreateSomeFile(CommandContext ctx, string fileName, [RemainingText]string fileBody)
        {
            using (var fs = new FileStream(fileName, FileMode.Create, FileAccess.ReadWrite))
            using (var sw = new StreamWriter(fs))
            {
                await sw.WriteAsync(fileBody);
            }

            using (var fs = new FileStream($"another {fileName}", FileMode.Create, FileAccess.ReadWrite))
            using (var sw = new StreamWriter(fs))
            {
                sw.AutoFlush = true;
                await sw.WriteLineAsync(fileBody);
                fs.Position = 0;
                var builder = new DiscordMessageBuilder();
                builder.WithContent("Here is a really dumb file that i am testing with.");
                builder.WithFile(fileName);
                builder.WithFile(fs);

                foreach (var file in builder.Files)
                {

                }

                await builder.SendAsync(ctx.Channel);
                //Testing to make sure the stream sent in is not disposed.

                await sw.WriteLineAsync("Another" + fileBody);
            }
            File.Delete(fileName);
            File.Delete("another " + fileName);
        }
    }
}
