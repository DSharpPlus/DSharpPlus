using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace DSharpPlus.Test.Tests
{
    public class GuildMembershipTests : BaseCommandModule
    {
        [Command]
        public async Task ModifyGuildMembership(CommandContext ctx)
        {
            await new DiscordGuildMembershipModifyBuilder()
                .WithEnabled(true)
                .WithDescription("An awesome test description")
                .WithField(new DiscordGuildMembershipScreeningField(MembershipScreeningFieldType.Terms, "This is a Test", new string[] { "I repeat this is a test" }, true))
                .ModifyAsync(ctx.Guild);
        }

        [Command]
        public async Task ModifyGuildMembershipAction(CommandContext ctx)
        {
            await ctx.Guild.ModifyMembershipScreeningFormAsync(x => {
                x.WithEnabled(false);
            });
        }
    }
}
