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

                // Verify that the lib resets the position when asked
                var builder = new DiscordMessageBuilder()
                    .WithContent("Testing the `Dictionary<string, stream>` Overload with resetting the postion turned on.")
                    .WithFiles(new Dictionary<string, Stream>() { { "ADumbFile1.txt", fs } }, true);

                await builder.SendAsync(ctx.Channel);
                await builder.SendAsync(ctx.Channel);

                builder.Clear();

                //Verify the lib doesnt reset the position.  THe file sent should have 0 bytes.
                builder.WithContent("Testing the `WithFile(Dictionary<string, stream> files)` Overload with resetting the postion turned off  The 2nd file sent should have 0 bytes.")
                    .WithFiles(new Dictionary<string, Stream>() { { "ADumbFile1.txt", fs } }, false);

                await builder.SendAsync(ctx.Channel);
                await builder.SendAsync(ctx.Channel);

                builder.Clear();

                fs.Position = 0;

                // Verify that the lib resets the position when asked
                builder.WithContent("Testing the `WithFile(Stream stream)` Overload with resetting the postion turned on.")
                    .WithFile(fs, true);

                await builder.SendAsync(ctx.Channel);
                await builder.SendAsync(ctx.Channel);

                builder.Clear();

                //Verify the lib doesnt reset the position.  THe file sent should have 0 bytes.
                builder.WithContent("Testing the `WithFile(Stream stream)` Overload with resetting the postion turned off.  The 2nd file sent should have 0 bytes.")
                    .WithFile(fs, false);

                await builder.SendAsync(ctx.Channel);
                await builder.SendAsync(ctx.Channel);

                builder.Clear();
                fs.Position = 0;


                // Verify that the lib resets the position when asked
                builder.WithContent("Testing the `WithFile(string fileName, Stream stream)` Overload with resetting the postion turned on.")
                    .WithFile("ADumbFile2.txt", fs, true);

                await builder.SendAsync(ctx.Channel);
                await builder.SendAsync(ctx.Channel);

                builder.Clear();

                //Verify the lib doesnt reset the position.  THe file sent should have 0 bytes.
                builder.WithContent("Testing the `WithFile(string fileName, Stream stream)` Overload with resetting the postion turned off.  The 2nd file sent should have 0 bytes.")
                    .WithFile("ADumbFile2.txt", fs, false);

                await builder.SendAsync(ctx.Channel);
                await builder.SendAsync(ctx.Channel);
            }
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
                //builder.WithFile(fileName);
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

        [Command("SendWebhookFiles")]
        public async Task SendWebhookFiles(CommandContext ctx)
        {
            var webhook = await ctx.Channel.CreateWebhookAsync("webhook-test");

            using (var fs = new FileStream("ADumbFile.txt", FileMode.Open, FileAccess.Read))
            {

                // Verify that the lib resets the position when asked
                var builder = new DiscordWebhookBuilder()
                    .WithContent("Testing the `AddFile(Dictionary<string, stream>)` Overload with resetting the postion turned on.")
                    .AddFiles(new Dictionary<string, Stream>() { { "ADumbFile1.txt", fs } }, true);

                await builder.SendAsync(webhook);
                await builder.SendAsync(webhook);

                builder.Clear();

                //Verify the lib doesnt reset the position.  THe file sent should have 0 bytes.
                builder.WithContent("Testing the `AddFile(Dictionary<string, stream> files)` Overload with resetting the postion turned off  The 2nd file sent should have 0 bytes.")
                    .AddFiles(new Dictionary<string, Stream>() { { "ADumbFile1.txt", fs } }, false);

                await builder.SendAsync(webhook);
                await builder.SendAsync(webhook);

                builder.Clear();

                fs.Position = 0;

                // Verify that the lib resets the position when asked
                builder.WithContent("Testing the `AddFile(Stream stream)` Overload with resetting the postion turned on.")
                    .AddFile(fs, true);

                await builder.SendAsync(webhook);
                await builder.SendAsync(webhook);

                builder.Clear();

                //Verify the lib doesnt reset the position.  THe file sent should have 0 bytes.
                builder.WithContent("Testing the `AddFile(Stream stream)` Overload with resetting the postion turned off.  The 2nd file sent should have 0 bytes.")
                    .AddFile(fs, false);

                await builder.SendAsync(webhook);
                await builder.SendAsync(webhook);

                builder.Clear();
                fs.Position = 0;


                // Verify that the lib resets the position when asked
                builder.WithContent("Testing the `AddFile(string fileName, Stream stream)` Overload with resetting the postion turned on.")
                    .AddFile("ADumbFile2.txt", fs, true);

                await builder.SendAsync(webhook);
                await builder.SendAsync(webhook);

                builder.Clear();

                //Verify the lib doesnt reset the position.  THe file sent should have 0 bytes.
                builder.WithContent("Testing the `AddFile(string fileName, Stream stream)` Overload with resetting the postion turned off.  The 2nd file sent should have 0 bytes.")
                    .AddFile("ADumbFile2.txt", fs, false);

                await builder.SendAsync(webhook);
                await builder.SendAsync(webhook);
            }

            await webhook.DeleteAsync();
        }

            [Command("chainreply")]
        public async Task ChainReplyAsync(CommandContext ctx)
        {
            DiscordMessageBuilder builder = new DiscordMessageBuilder();

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
