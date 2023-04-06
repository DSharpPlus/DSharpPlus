// This file is part of the DSharpPlus project.
//
// Copyright (c) 2015 Mike Santiago
// Copyright (c) 2016-2023 DSharpPlus Contributors
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity.Extensions;

namespace DSharpPlus.Test;

public class TestBotCommands : BaseCommandModule
{
    public static ConcurrentDictionary<ulong, string> PrefixSettings { get; } = new();

    [Command("crosspost")]
    public async Task CrosspostAsync(CommandContext ctx, DiscordChannel chn, DiscordMessage msg)
    {
        DiscordMessage message = await chn.CrosspostMessageAsync(msg);
        await ctx.RespondAsync($":ok_hand: Message published: {message.Id}");
    }

    [Command("follow")]
    public async Task FollowAsync(CommandContext ctx, DiscordChannel channelToFollow, DiscordChannel targetChannel)
    {
        await channelToFollow.FollowAsync(targetChannel);
        await ctx.RespondAsync($":ok_hand: Following channel {channelToFollow.Mention} into {targetChannel.Mention} (Guild: {targetChannel.Guild.Id})");
    }

    [Command("setprefix"), Aliases("channelprefix"), Description("Sets custom command prefix for current channel. The bot will still respond to the default one."), RequireOwner]
    public async Task SetPrefixAsync(CommandContext ctx, [Description("The prefix to use for current channel.")] string prefix = null)
    {
        if (string.IsNullOrWhiteSpace(prefix))
        {
            if (PrefixSettings.TryRemove(ctx.Channel.Id, out _))
            {
                await ctx.RespondAsync("👍");
            }
            else
            {
                await ctx.RespondAsync("👎");
            }
        }
        else
        {
            PrefixSettings.AddOrUpdate(ctx.Channel.Id, prefix, (k, vold) => prefix);
            await ctx.RespondAsync("👍");
        }
    }

    [Command("sudo"), Description("Run a command as another user."), Hidden, RequireOwner]
    public async Task SudoAsync(CommandContext ctx, DiscordUser user, [RemainingText] string content)
    {
        Command? cmd = ctx.CommandsNext.FindCommand(content, out string? args);
        CommandContext fctx = ctx.CommandsNext.CreateFakeContext(user, ctx.Channel, content, ctx.Prefix, cmd, args);
        await ctx.CommandsNext.ExecuteCommandAsync(fctx);
    }

    [Group("bind"), Description("Various argument binder testing commands.")]
    public class Binding : BaseCommandModule
    {
        [Command("message"), Aliases("msg"), Description("Attempts to bind a message.")]
        public static Task MessageAsync(CommandContext ctx, DiscordMessage msg)
            => ctx.RespondAsync(embed: new DiscordEmbedBuilder()
                .WithTimestamp(msg.CreationTimestamp)
                .WithAuthor($"{msg.Author.Username}#{msg.Author.Discriminator}", msg.Author.AvatarUrl)
                .WithDescription(msg.Content));
    }

    [Command("editmention")]
    public async Task EditMentionsAsync(CommandContext ctx)
    {
        IDiscordMessageBuilder builder = new DiscordMessageBuilder()
            .WithContent("Mentioning <@&879398655130472508> and <@743323785549316197>")
            .WithReply(ctx.Message.Id, true)
            .AddMention(new RoleMention(879398655130472508));
        //.WithAllowedMention(new UserMention(743323785549316197));//.WithAllowedMention(new RoleMention(879398655130472508));

        DiscordMessage msg = await (builder as DiscordMessageBuilder).SendAsync(ctx.Channel);
        await msg.ModifyAsync("Mentioning <@&879398655130472508> and <@743323785549316197>, but edited!");
    }

    [Command("mention"), Description("Attempts to mention a user")]
    public async Task MentionablesAsync(CommandContext ctx, DiscordUser user)
    {
        string content = $"Hey, {user.Mention}! Listen!";
        await ctx.Channel.SendMessageAsync("✔ should ping, ❌ should not ping.");

        await ctx.Channel.SendMessageAsync("✔ Default Behaviour: " + content);                                                                                            //Should ping User

        await new DiscordMessageBuilder()
            .WithContent("✔ UserMention(user): " + content)
            .WithAllowedMentions(new IMention[] { new UserMention(user) })
            .SendAsync(ctx.Channel)
            ;                                                                                                                      //Should ping user

        await new DiscordMessageBuilder()
            .WithContent("✔ UserMention(): " + content)
            .WithAllowedMentions(new IMention[] { new UserMention() })
            .SendAsync(ctx.Channel)
            ;                                                                                                                      //Should ping user

        await new DiscordMessageBuilder()
            .WithContent("✔ User Mention Everyone & Self: " + content)
            .WithAllowedMentions(new IMention[] { new UserMention(), new UserMention(user) })
            .SendAsync(ctx.Channel)
            ;                                                                                                                      //Should ping user


        await new DiscordMessageBuilder()
           .WithContent("✔ UserMention.All: " + content)
           .WithAllowedMentions(new IMention[] { UserMention.All })
           .SendAsync(ctx.Channel)
           ;                                                                                                                       //Should ping user

        await new DiscordMessageBuilder()
           .WithContent("❌ Empty Mention Array: " + content)
           .WithAllowedMentions(Array.Empty<IMention>())
           .SendAsync(ctx.Channel)
           ;                                                                                                                       //Should ping no one

        await new DiscordMessageBuilder()
           .WithContent("❌ UserMention(SomeoneElse): " + content)
           .WithAllowedMentions(new IMention[] { new UserMention(545836271960850454L) })
           .SendAsync(ctx.Channel)
           ;                                                                                                                       //Should ping no one (@user was not pinged)

        await new DiscordMessageBuilder()
           .WithContent("❌ Everyone():" + content)
           .WithAllowedMentions(new IMention[] { new EveryoneMention() })
           .SendAsync(ctx.Channel)
           ;                                                                                                                       //Should ping no one (@everyone was not pinged)
    }

    [Command("editMention"), Description("Attempts to mention a user via edit message")]
    public async Task EditMentionablesAsync(CommandContext ctx, DiscordUser user)
    {
        string origcontent = $"Hey, silly! Listen!";
        string newContent = $"Hey, {user.Mention}! Listen!";

        await ctx.Channel.SendMessageAsync("✔ should ping, ❌ should not ping.");

        DiscordMessage test1Msg = await ctx.Channel.SendMessageAsync("✔ Default Behaviour: " + origcontent);
        await new DiscordMessageBuilder()
           .WithContent("✔ Default Behaviour: " + newContent)
           .ModifyAsync(test1Msg)
           ;                                                                                                                               //Should ping User

        DiscordMessage test2Msg = await ctx.Channel.SendMessageAsync("✔ UserMention(user): " + origcontent);
        await new DiscordMessageBuilder()
           .WithContent("✔ UserMention(user): " + newContent)
           .WithAllowedMentions(new IMention[] { new UserMention(user) })
           .ModifyAsync(test2Msg)
           ;                                                                                                                               //Should ping user

        DiscordMessage test3Msg = await ctx.Channel.SendMessageAsync("✔ UserMention(): " + origcontent);
        await new DiscordMessageBuilder()
           .WithContent("✔ UserMention(): " + newContent)
           .WithAllowedMentions(new IMention[] { new UserMention() })
           .ModifyAsync(test3Msg)
           ;                                                                                                                               //Should ping user

        DiscordMessage test4Msg = await ctx.Channel.SendMessageAsync("✔ User Mention Everyone & Self: " + origcontent);
        await new DiscordMessageBuilder()
           .WithContent("✔ User Mention Everyone & Self: " + newContent)
           .WithAllowedMentions(new IMention[] { new UserMention(), new UserMention(user) })
           .ModifyAsync(test4Msg)
           ;                                                                                                                               //Should ping user

        DiscordMessage test5Msg = await ctx.Channel.SendMessageAsync("✔ UserMention.All: " + origcontent);
        await new DiscordMessageBuilder()
           .WithContent("✔ UserMention.All: " + newContent)
           .WithAllowedMentions(new IMention[] { UserMention.All })
           .ModifyAsync(test5Msg)
           ;                                                                                                                               //Should ping user

        DiscordMessage test6Msg = await ctx.Channel.SendMessageAsync("❌ Empty Mention Array: " + origcontent);
        await new DiscordMessageBuilder()
           .WithContent("❌ Empty Mention Array: " + newContent)
           .WithAllowedMentions(Array.Empty<IMention>())
           .ModifyAsync(test6Msg)
           ;                                                                                                                               //Should ping no one

        DiscordMessage test7Msg = await ctx.Channel.SendMessageAsync("❌ UserMention(SomeoneElse): " + origcontent);
        await new DiscordMessageBuilder()
           .WithContent("❌ UserMention(SomeoneElse): " + newContent)
           .WithAllowedMentions(new IMention[] { new UserMention(777677298316214324) })
           .ModifyAsync(test7Msg)
           ;                                                                                                                               //Should ping no one (@user was not pinged)

        DiscordMessage test8Msg = await ctx.Channel.SendMessageAsync("❌ Everyone(): " + origcontent);
        await new DiscordMessageBuilder()
           .WithContent("❌ Everyone(): " + newContent)
           .WithAllowedMentions(new IMention[] { new EveryoneMention() })
           .ModifyAsync(test8Msg)
           ;                                                                                                                               //Should ping no one (@everyone was not pinged)
    }

    [Command("SendSomeFile")]
    public async Task SendSomeFileAsync(CommandContext ctx)
    {
        using (FileStream fs = new("ADumbFile.txt", FileMode.Open, FileAccess.Read))
        {

            // Verify that the lib resets the position when asked
            DiscordMessageBuilder builder = new DiscordMessageBuilder()
                .WithContent("Testing the `Dictionary<string, stream>` Overload with resetting the postion turned on.")
                .AddFiles(new Dictionary<string, Stream>() { { "ADumbFile1.txt", fs } }, true);

            await builder.SendAsync(ctx.Channel);
            await builder.SendAsync(ctx.Channel);

            builder.Clear();

            //Verify the lib doesnt reset the position.  THe file sent should have 0 bytes.
            builder.WithContent("Testing the `WithFile(Dictionary<string, stream> files)` Overload with resetting the postion turned off  The 2nd file sent should have 0 bytes.")
                .AddFiles(new Dictionary<string, Stream>() { { "ADumbFile1.txt", fs } }, false);

            await builder.SendAsync(ctx.Channel);
            await builder.SendAsync(ctx.Channel);

            builder.Clear();

            fs.Position = 0;

            // Verify that the lib resets the position when asked
            builder.WithContent("Testing the `WithFile(Stream stream)` Overload with resetting the postion turned on.")
                .AddFile(fs, true);

            await builder.SendAsync(ctx.Channel);
            await builder.SendAsync(ctx.Channel);

            builder.Clear();

            //Verify the lib doesnt reset the position.  THe file sent should have 0 bytes.
            builder.WithContent("Testing the `WithFile(Stream stream)` Overload with resetting the postion turned off.  The 2nd file sent should have 0 bytes.")
                .AddFile(fs, false);

            await builder.SendAsync(ctx.Channel);
            await builder.SendAsync(ctx.Channel);

            builder.Clear();
            fs.Position = 0;


            // Verify that the lib resets the position when asked
            builder.WithContent("Testing the `WithFile(string fileName, Stream stream)` Overload with resetting the postion turned on.")
                .AddFile("ADumbFile2.txt", fs, true);

            await builder.SendAsync(ctx.Channel);
            await builder.SendAsync(ctx.Channel);

            builder.Clear();

            //Verify the lib doesnt reset the position.  THe file sent should have 0 bytes.
            builder.WithContent("Testing the `WithFile(string fileName, Stream stream)` Overload with resetting the postion turned off.  The 2nd file sent should have 0 bytes.")
                .AddFile("ADumbFile2.txt", fs, false);

            await builder.SendAsync(ctx.Channel);
            await builder.SendAsync(ctx.Channel);
        }
    }

    [Command("CreateSomeFile")]
    public async Task CreateSomeFileAsync(CommandContext ctx, string fileName, [RemainingText] string fileBody)
    {
        using (FileStream fs = new(fileName, FileMode.Create, FileAccess.ReadWrite))
        using (StreamWriter sw = new(fs))
        {
            await sw.WriteAsync(fileBody);
        }

        using (FileStream fs = new($"another {fileName}", FileMode.Create, FileAccess.ReadWrite))
        using (StreamWriter sw = new(fs))
        {
            sw.AutoFlush = true;
            await sw.WriteLineAsync(fileBody);
            fs.Position = 0;
            DiscordMessageBuilder builder = new();
            builder.WithContent("Here is a really dumb file that i am testing with.");
            //builder.WithFile(fileName);
            builder.AddFile(fs);

            foreach (DiscordMessageFile file in builder.Files)
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
    public async Task SendWebhookFilesAsync(CommandContext ctx)
    {
        DiscordWebhook webhook = await ctx.Channel.CreateWebhookAsync("webhook-test");

        using (FileStream fs = new("ADumbFile.txt", FileMode.Open, FileAccess.Read))
        {

            // Verify that the lib resets the position when asked
            DiscordWebhookBuilder builder = new DiscordWebhookBuilder()
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
        DiscordMessageBuilder builder = new();

        StringBuilder contentBuilder = new();

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
    public async Task MentionAllMentionedUsersAsync(CommandContext ctx, [RemainingText][Description("Just a string so that DSharpPlus will parse no matter what I say")] string mentionedUsers)
    {
        string content = "You didn't have any users to mention";
        if (ctx.Message.MentionedUsers.Any())
        {
            content = string.Join(", ", ctx.Message.MentionedUsers.Select(usr => usr.Mention));
        }

        await ctx.RespondAsync(content);
    }

    [Command("mentionroles")]
    public async Task MentionAllMentionedRolesAsync(CommandContext ctx, [RemainingText][Description("Just a string so that DSharpPlus will parse no matter what I say")] string mentionedRoles)
    {
        string content = "You didn't have any roles to mention";
        if (ctx.Message.MentionedRoles.Any())
        {
            content = string.Join(", ", ctx.Message.MentionedRoles.Select(role => role.Mention));
        }

        await ctx.RespondAsync(content);
    }

    [Command("mentionchannels")]
    public async Task MentionChannelsAsync(CommandContext ctx, [RemainingText][Description("Just a string so that DSharpPlus will parse no matter what I say")] string mentionedChannels)
    {
        string content = "You didn't have any channels to mention";
        if (ctx.Message.MentionedChannels.Any())
        {
            content = string.Join(", ", ctx.Message.MentionedChannels.Select(role => role.Mention));
        }

        await ctx.RespondAsync(content);
    }

    [Command("getmessagementions")]
    public async Task GetMessageMentionsAsync(CommandContext ctx, ulong msgId)
    {
        DiscordMessage msg = await ctx.Channel.GetMessageAsync(msgId);
        StringBuilder contentBuilder = new("You didn't mention any user, channel, or role.");

        if (msg.MentionedUsers.Any() || msg.MentionedRoles.Any() || msg.MentionedChannels.Any())
        {
            contentBuilder.Clear();
            contentBuilder.AppendLine(msg.MentionedUsers.Any() ? string.Join(", ", msg.MentionedUsers.Select(usr => usr.Mention)) : string.Empty);
            contentBuilder.AppendLine(msg.MentionedChannels.Any() ? string.Join(", ", msg.MentionedChannels.Select(usr => usr.Mention)) : string.Empty);
            contentBuilder.AppendLine(msg.MentionedRoles.Any() ? string.Join(", ", msg.MentionedRoles.Select(usr => usr.Mention)) : string.Empty);
        }

        await ctx.RespondAsync(contentBuilder.ToString());
    }
    [Command("getattachmenttype")]
    public async Task GetAttachmentsTypesAsync(CommandContext ctx, ulong? messageId = null)
    {
        if (messageId is null)
        {
            messageId = ctx.Message.Id;
        }

        DiscordMessage message = await ctx.Channel.GetMessageAsync(messageId.Value);
        StringBuilder contentBuilder = new("Message has no attachment.");


        if (message.Attachments.Any())
        {
            contentBuilder.Clear();
            foreach (DiscordAttachment attachment in message.Attachments)
            {
                contentBuilder.AppendLine($"{attachment.FileName} is {attachment.MediaType}");
            }
        }
        await ctx.RespondAsync(contentBuilder.ToString());
    }
    [Command("createvoices")]
    public async Task CreateVoiceChannelsAsync(CommandContext ctx, DiscordChannel channel)
    {
        if (channel.Type != ChannelType.Voice)
        {
            await ctx.RespondAsync("Channel is not a voice channel.");
            return;
        }

        DiscordChannel cNull = await ctx.Guild.CreateVoiceChannelAsync(channel.Name + " [Null]", channel.Parent, null, null, null, null, null);
        DiscordChannel cAuto = await ctx.Guild.CreateVoiceChannelAsync(channel.Name + " [Auto]", channel.Parent, null, null, null, VideoQualityMode.Auto, null);
        DiscordChannel cFull = await ctx.Guild.CreateVoiceChannelAsync(channel.Name + " [Full]", channel.Parent, null, null, null, VideoQualityMode.Full, null);

        await ctx.RespondAsync($"{cNull.Mention}, {cAuto.Mention}, and {cFull.Mention} created. Delete channels? (Y)");
        Interactivity.InteractivityResult<DiscordMessage> result = await ctx.Message.GetNextMessageAsync(m => m.Content.Equals("Y", StringComparison.OrdinalIgnoreCase), TimeSpan.FromMinutes(1));

        if (!result.TimedOut)
        {
            await cNull.DeleteAsync();
            await cAuto.DeleteAsync();
            await cFull.DeleteAsync();
        }
    }

    [Command("createchannel")]
    public async Task CreateGuildChannelsAsync(CommandContext ctx, [RemainingText] string name)
    {
        if (string.IsNullOrEmpty(name))
        {
            await ctx.RespondAsync("You must provide a channel name");
            return;
        }

        DiscordChannel channel = await ctx.Guild.CreateTextChannelAsync(name);
        DiscordMessage msg = await ctx.RespondAsync($"{channel.Mention} created. Delete? (Y)");
        Interactivity.InteractivityResult<DiscordMessage> result = await ctx.Message.GetNextMessageAsync(m => m.Content.Equals("y", StringComparison.OrdinalIgnoreCase), TimeSpan.FromMinutes(1));

        if (!result.TimedOut)
        {
            await channel.DeleteAsync();
            await ctx.RespondAsync("Channel deleted");
        }
    }

    [Command("pc")]
    [Aliases("purgechat")]
    [Description("Purges chat")]
    [RequirePermissions(Permissions.ManageChannels)]
    public async Task PurgeChatAsync(CommandContext ctx)
    {
        DiscordChannel channel = ctx.Channel;
        int z = ctx.Channel.Position;
        DiscordChannel x = await channel.CloneAsync();
        await channel.DeleteAsync();
        await x.ModifyPositionAsync(z);
        DiscordEmbedBuilder embed2 = new DiscordEmbedBuilder()
            .WithTitle("✅ Purged")
            .WithFooter($"foo");
        await x.SendMessageAsync(embed: embed2);
    }
    [Command("ping"), Aliases("p")]
    public async Task PingAsync(CommandContext ctx)
    {
        await ctx.TriggerTypingAsync();
        await ctx.RespondAsync($"Pong: {ctx.Client.Ping}ms");
    }

    [Command("parsedate")]
    public async Task ParseDateTimeAsync(CommandContext ctx, DateTimeOffset dto)
        => await ctx.RespondAsync(dto.ToString("yyyy-MM-ddTHH:mm:ss.ffffffzzz"));

    [Command("longlongman")]
    public async Task LongLongManAsync(CommandContext ctx)
    {
        await ctx.Message.CreateReactionAsync(DiscordEmoji.FromName(ctx.Client, ":mshourglass:"));
        await Task.Delay(2_000);
        await ctx.RespondAsync(DiscordEmoji.FromName(ctx.Client, ":msokhand:"));
    }
}
