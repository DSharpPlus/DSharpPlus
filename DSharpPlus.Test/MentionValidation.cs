// This file is part of the DSharpPlus project.
//
// Copyright (c) 2015 Mike Santiago
// Copyright (c) 2016-2022 DSharpPlus Contributors
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

namespace DSharpPlus.Test
{
    public class MentionValidation : BaseCommandModule
    {
        [Command("mention_test")]
        public async Task MentionTestAsync(CommandContext ctx, DiscordRole role)
        {
            var progressBuilder = new StringBuilder();
            var progressMessage = await ctx.RespondAsync("Waiting for results");

            var m1 = await new DiscordMessageBuilder().WithContent($"{ctx.User.Mention}, {role.Mention} Default").SendAsync(ctx.Channel);
            var m2 = await new DiscordMessageBuilder().WithContent($"{ctx.User.Mention}, {role.Mention} User").WithAllowedMention(new UserMention(ctx.User)).SendAsync(ctx.Channel);
            var m3 = await new DiscordMessageBuilder().WithContent($"{ctx.User.Mention}, {role.Mention} Role").WithAllowedMention(new RoleMention(role)).SendAsync(ctx.Channel);
            var m4 = await new DiscordMessageBuilder().WithContent($"{ctx.User.Mention}, {role.Mention} None").WithAllowedMentions(Mentions.None).SendAsync(ctx.Channel);

            var me1 = await new DiscordMessageBuilder().WithContent($"{ctx.User.Mention}, {role.Mention} Reply without mention").WithReply(ctx.Message.Id).SendAsync(ctx.Channel);
            var me2 = await new DiscordMessageBuilder().WithContent($"{ctx.User.Mention}, {role.Mention} Reply with mention").WithReply(ctx.Message.Id, true).SendAsync(ctx.Channel);

            var me1UserBefore = me1.MentionedUsers.Count;
            var me1RoleBefore = me1.MentionedRoles.Count;

            var me2UserBefore = me2.MentionedUsers.Count;
            var me2RoleBefore = me2.MentionedRoles.Count;

            await me1.ModifyAsync($"{role.Mention} Reply without mention");
            await me2.ModifyAsync($"{role.Mention} Reply with mention");


            var me1UserAfter = me1.MentionedUsers.Count;
            var me1RoleAfter = me1.MentionedRoles.Count;

            var me2UserAfter = me2.MentionedUsers.Count;
            var me2RoleAfter = me2.MentionedRoles.Count;

            if (m1.MentionedUsers.Count is 0 && m1.MentionedRoles.Count is 0)
                progressBuilder.AppendLine("Default (No mentions) **PASSED**");
            else
                progressBuilder.AppendLine($"Default (No mentions) **FAILED** (User: Expected 0, got {m1.MentionedUsers.Count} | Role: Expected 0 got {m1.MentionedRoles.Count})");

            if (m2.MentionedUsers.Count is 1 && m2.MentionedRoles.Count is 0)
                progressBuilder.AppendLine("User mention without role **PASSED**");
            else
                progressBuilder.AppendLine($"User mention without role **FAILED** (User: Expected 1, got {m2.MentionedUsers.Count} | Role: Expected 0 got {m2.MentionedRoles.Count})");

            if (m3.MentionedUsers.Count is 0 && m3.MentionedRoles.Count is 1)
                progressBuilder.AppendLine("Role mention without user **PASSED**");
            else
                progressBuilder.AppendLine($"Role mention without user **FAILED** (User: Expected 0, got {m3.MentionedUsers.Count} | Role: Expected 1 got {m3.MentionedRoles.Count})");

            if (m4.MentionedUsers.Count is 0 && m4.MentionedRoles.Count is 0)
                progressBuilder.AppendLine("No mentions explicit **PASSED**");
            else
                progressBuilder.AppendLine($"No mentions explicit **FAILED** (User: Expected 0, got {m4.MentionedUsers.Count} | Role: Expected 0 got {m4.MentionedRoles.Count})");

            if (me1UserBefore is 0 && me1UserAfter is 0 && me1RoleBefore is 0 && me1RoleAfter is 0)
                progressBuilder.AppendLine("Reply edit (without mention) **PASSED**");
            else
                progressBuilder.AppendLine("Reply edit (without mention) **FAILED**");

            if (me2UserBefore is 1 && me2UserAfter is 1 && me2RoleBefore is 0 && me2RoleAfter is 0)
                progressBuilder.AppendLine("Reply edit (with mention) **PASSED**");
            else
                progressBuilder.AppendLine("Reply edit (with mention) **FAILED**");

            progressBuilder.AppendLine(new string('-', 20) + "\n\n\u200b");
            await progressMessage.ModifyAsync(progressBuilder.ToString());
        }

    }
}
