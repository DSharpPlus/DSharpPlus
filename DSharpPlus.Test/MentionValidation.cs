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
using System.Text;
using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;

namespace DSharpPlus.Test;

public class MentionValidation : BaseCommandModule
{
    [Command("mention_test")]
    public async Task MentionTestAsync(CommandContext ctx, DiscordRole role)
    {
        StringBuilder progressBuilder = new StringBuilder();
        DiscordMessage progressMessage = await ctx.RespondAsync("Waiting for results");

        DiscordMessage defaultMentionMessage = await new DiscordMessageBuilder().WithContent($"{ctx.User.Mention}, {role.Mention} Default").SendAsync(ctx.Channel);
        DiscordMessage userMentionMessage = await new DiscordMessageBuilder().WithContent($"{ctx.User.Mention}, {role.Mention} User").WithAllowedMention(new UserMention(ctx.User)).SendAsync(ctx.Channel);
        DiscordMessage roleMentionMessage = await new DiscordMessageBuilder().WithContent($"{ctx.User.Mention}, {role.Mention} Role").WithAllowedMention(new RoleMention(role)).SendAsync(ctx.Channel);
        DiscordMessage noMentionMessage = await new DiscordMessageBuilder().WithContent($"{ctx.User.Mention}, {role.Mention} None").WithAllowedMentions(Mentions.None).SendAsync(ctx.Channel);

        DiscordMessage replyMessageWithoutMention = await new DiscordMessageBuilder().WithContent($"{ctx.User.Mention}, {role.Mention} Reply without mention").WithReply(ctx.Message.Id).SendAsync(ctx.Channel);
        DiscordMessage replyMessageWithMention = await new DiscordMessageBuilder().WithContent($"{ctx.User.Mention}, {role.Mention} Reply with mention").WithReply(ctx.Message.Id, true).SendAsync(ctx.Channel);

        int replyUserMentionCountBefore = replyMessageWithoutMention.MentionedUsers.Count;
        int replyRoleMentionCountBefore = replyMessageWithoutMention.MentionedRoles.Count;

        int replyWithMentionUserMentionCountBefore = replyMessageWithMention.MentionedUsers.Count;
        int replyWithMentionRoleMentionCountBefore = replyMessageWithMention.MentionedRoles.Count;

        DiscordMessage replyWithoutMentionUpdated = await replyMessageWithoutMention.ModifyAsync($"{role.Mention} Reply without mention");
        DiscordMessage replyWithMentionUpdated = await replyMessageWithMention.ModifyAsync($"{role.Mention} Reply with mention");


        int replyWithoutMentionUserMentionCountAfter = replyWithoutMentionUpdated.MentionedUsers.Count;
        int replyWithoutMentionRoleMentionCountAfter = replyWithoutMentionUpdated.MentionedRoles.Count;

        int replyWithMentionUserMentionCountAfter = replyWithMentionUpdated.MentionedUsers.Count;
        int replyWithMentionRoleMentionCountAfter = replyWithMentionUpdated.MentionedRoles.Count;

        if (defaultMentionMessage.MentionedUsers.Count is 0 && defaultMentionMessage.MentionedRoles.Count is 0)
        {
            progressBuilder.AppendLine("Default (No mentions) **PASSED**");
        }
        else
        {
            progressBuilder.AppendLine($"Default (No mentions) **FAILED** (User: Expected 0, got {defaultMentionMessage.MentionedUsers.Count} | Role: Expected 0 got {defaultMentionMessage.MentionedRoles.Count})");
        }

        if (userMentionMessage.MentionedUsers.Count is 1 && userMentionMessage.MentionedRoles.Count is 0)
        {
            progressBuilder.AppendLine("User mention without role **PASSED**");
        }
        else
        {
            progressBuilder.AppendLine($"User mention without role **FAILED** (User: Expected 1, got {userMentionMessage.MentionedUsers.Count} | Role: Expected 0 got {userMentionMessage.MentionedRoles.Count})");
        }

        if (roleMentionMessage.MentionedUsers.Count is 0 && roleMentionMessage.MentionedRoles.Count is 1)
        {
            progressBuilder.AppendLine("Role mention without user **PASSED**");
        }
        else
        {
            progressBuilder.AppendLine($"Role mention without user **FAILED** (User: Expected 0, got {roleMentionMessage.MentionedUsers.Count} | Role: Expected 1 got {roleMentionMessage.MentionedRoles.Count})");
        }

        if (noMentionMessage.MentionedUsers.Count is 0 && noMentionMessage.MentionedRoles.Count is 0)
        {
            progressBuilder.AppendLine("No mentions explicit **PASSED**");
        }
        else
        {
            progressBuilder.AppendLine($"No mentions explicit **FAILED** (User: Expected 0, got {noMentionMessage.MentionedUsers.Count} | Role: Expected 0 got {noMentionMessage.MentionedRoles.Count})");
        }

        if (replyUserMentionCountBefore is 0 && replyWithoutMentionUserMentionCountAfter is 0 && replyRoleMentionCountBefore is 0 && replyWithoutMentionRoleMentionCountAfter is 0)
        {
            progressBuilder.AppendLine("Reply edit (without mention) **PASSED**");
        }
        else
        {
            progressBuilder.AppendLine("Reply edit (without mention) **FAILED**");
        }

        if (replyWithMentionUserMentionCountBefore is 1 && replyWithMentionUserMentionCountAfter is 1 && replyWithMentionRoleMentionCountBefore is 0 && replyWithMentionRoleMentionCountAfter is 0)
        {
            progressBuilder.AppendLine("Reply edit (with mention) **PASSED**");
        }
        else
        {
            progressBuilder.AppendLine("Reply edit (with mention) **FAILED**");
        }

        progressBuilder.AppendLine(new string('-', 20) + "\n\n\u200b");
        await progressMessage.ModifyAsync(progressBuilder.ToString());
    }

}
