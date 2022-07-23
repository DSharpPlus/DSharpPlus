using System;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;

namespace ToffyDiscord.Commands;


public class ModerationModule : BaseCommandModule
{
    [Command("ban")]
    [Description("Бан юзера")]
    [RequirePermissions(Permissions.BanMembers)]
    [Hidden]
    public async Task Ban(CommandContext ctx, DiscordMember member, int days = 1, string reason = "default")
    {
        await ctx.TriggerTypingAsync();
        try
        {
            await ctx.Guild.BanMemberAsync(member, days, reason);
            await ctx.PromotionResponseAsync(
                $"Юзер @{member.Username}#{member.Discriminator} був вилучений адміністратором {ctx.User.Username}");
        }
        catch (Exception)
        {
            await ctx.PromotionResponseAsync($"Юзер {member.Username} не може бути заблокованим");
        }
    }


    [Command("unban")]
    [Description("Разбан юзера")]
    [RequirePermissions(Permissions.BanMembers)]
    [Hidden]
    public async Task Unban(CommandContext ctx, DiscordUser member)
    {
        await ctx.TriggerTypingAsync();
        try
        {
            await ctx.Guild.UnbanMemberAsync(member);
            await ctx.RespondAsync(
                $"Юзер @{member.Username}#{member.Discriminator} був разблокований адміністратором {ctx.User.Username}");
        }
        catch (Exception)
        {
            await ctx.PromotionResponseAsync($"Юзер {member.Username} не може бути разблокованим");
        }
    }

    [Command("showbans")]
    public async Task ShowBan(CommandContext ctx)
    {
        await ctx.TriggerTypingAsync();
        var bans = await ctx.Guild.GetBansAsync();
        string bansList = "";
        int count = 1;
        foreach (var ban in bans)
        {
            bansList += $"{count}. {ban.User.Username}#{ban.User.Discriminator}\n";
            count++;
        }
        await ctx.PromotionResponseAsync("Погані люди:\n"+bansList);
    }
}
