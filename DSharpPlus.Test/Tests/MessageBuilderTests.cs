using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace DSharpPlus.Test.Tests
{
    public class MessageBuilderTests : BaseCommandModule
    {
        [Command("mention"), Description("Attempts to mention a user")]
        public async Task MentionablesAsync(CommandContext ctx, DiscordUser user)
        {
            string content = $"Hey, {user.Mention}! Listen!";
            await ctx.Channel.SendMessageAsync("✔ should ping, ❌ should not ping.").ConfigureAwait(false);

            await ctx.Channel.SendMessageAsync("✔ Default Behaviour: " + content).ConfigureAwait(false);                                                     //Should ping User

            await new DiscordMessageCreateBuilder()
                .WithContent("✔ UserMention(user): " + content)
                .WithAllowedMentions(new IMention[] { new UserMention(user) })
                .SendAsync(ctx.Channel)
                .ConfigureAwait(false);                                                                                                                      //Should ping user

            await new DiscordMessageCreateBuilder()
                .WithContent("✔ UserMention(): " + content)
                .WithAllowedMentions(new IMention[] { new UserMention() })
                .SendAsync(ctx.Channel)
                .ConfigureAwait(false);                                                                                                                      //Should ping user

            await new DiscordMessageCreateBuilder()
                .WithContent("✔ User Mention Everyone & Self: " + content)
                .WithAllowedMentions(new IMention[] { new UserMention(), new UserMention(user) })
                .SendAsync(ctx.Channel)
                .ConfigureAwait(false);                                                                                                                      //Should ping user


            await new DiscordMessageCreateBuilder()
               .WithContent("✔ UserMention.All: " + content)
               .WithAllowedMentions(new IMention[] { UserMention.All })
               .SendAsync(ctx.Channel)
               .ConfigureAwait(false);                                                                                                                       //Should ping user

            await new DiscordMessageCreateBuilder()
               .WithContent("❌ Empty Mention Array: " + content)
               .WithAllowedMentions(new IMention[0])
               .SendAsync(ctx.Channel)
               .ConfigureAwait(false);                                                                                                                       //Should ping no one

            await new DiscordMessageCreateBuilder()
               .WithContent("❌ UserMention(SomeoneElse): " + content)
               .WithAllowedMentions(new IMention[] { new UserMention(545836271960850454L) })
               .SendAsync(ctx.Channel)
               .ConfigureAwait(false);                                                                                                                       //Should ping no one (@user was not pinged)

            await new DiscordMessageCreateBuilder()
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
            await new DiscordMessageModifyBuilder()
               .WithContent("✔ Default Behaviour: " + newContent)
               .ModifyAsync(test1Msg)
               .ConfigureAwait(false);                                                                                                                               //Should ping User

            var test2Msg = await ctx.Channel.SendMessageAsync("✔ UserMention(user): " + origcontent).ConfigureAwait(false);
            await new DiscordMessageModifyBuilder()
               .WithContent("✔ UserMention(user): " + newContent)
               .WithAllowedMentions(new IMention[] { new UserMention(user) })
               .ModifyAsync(test2Msg)
               .ConfigureAwait(false);                                                                                                                               //Should ping user

            var test3Msg = await ctx.Channel.SendMessageAsync("✔ UserMention(): " + origcontent).ConfigureAwait(false);
            await new DiscordMessageModifyBuilder()
               .WithContent("✔ UserMention(): " + newContent)
               .WithAllowedMentions(new IMention[] { new UserMention() })
               .ModifyAsync(test3Msg)
               .ConfigureAwait(false);                                                                                                                               //Should ping user

            var test4Msg = await ctx.Channel.SendMessageAsync("✔ User Mention Everyone & Self: " + origcontent).ConfigureAwait(false);
            await new DiscordMessageModifyBuilder()
               .WithContent("✔ User Mention Everyone & Self: " + newContent)
               .WithAllowedMentions(new IMention[] { new UserMention(), new UserMention(user) })
               .ModifyAsync(test4Msg)
               .ConfigureAwait(false);                                                                                                                               //Should ping user

            var test5Msg = await ctx.Channel.SendMessageAsync("✔ UserMention.All: " + origcontent).ConfigureAwait(false);
            await new DiscordMessageModifyBuilder()
               .WithContent("✔ UserMention.All: " + newContent)
               .WithAllowedMentions(new IMention[] { UserMention.All })
               .ModifyAsync(test5Msg)
               .ConfigureAwait(false);                                                                                                                               //Should ping user

            var test6Msg = await ctx.Channel.SendMessageAsync("❌ Empty Mention Array: " + origcontent).ConfigureAwait(false);
            await new DiscordMessageModifyBuilder()
               .WithContent("❌ Empty Mention Array: " + newContent)
               .WithAllowedMentions(new IMention[0])
               .ModifyAsync(test6Msg)
               .ConfigureAwait(false);                                                                                                                               //Should ping no one

            var test7Msg = await ctx.Channel.SendMessageAsync("❌ UserMention(SomeoneElse): " + origcontent).ConfigureAwait(false);
            await new DiscordMessageModifyBuilder()
               .WithContent("❌ UserMention(SomeoneElse): " + newContent)
               .WithAllowedMentions(new IMention[] { new UserMention(777677298316214324) })
               .ModifyAsync(test7Msg)
               .ConfigureAwait(false);                                                                                                                               //Should ping no one (@user was not pinged)

            var test8Msg = await ctx.Channel.SendMessageAsync("❌ Everyone(): " + origcontent).ConfigureAwait(false);
            await new DiscordMessageModifyBuilder()
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
                var builder = new DiscordMessageCreateBuilder()
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
        public async Task CreateSomeFile(CommandContext ctx, string fileName, [RemainingText] string fileBody)
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
                var builder = new DiscordMessageCreateBuilder();
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
                var builder = new DiscordWebhookMessageCreateBuilder()
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
    }
}
