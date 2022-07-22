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
    [Description("Бан пользователя")]
    [RequirePermissions(Permissions.BanMembers)]
    [Hidden]
    public async Task Ban(CommandContext ctx, [Description("Блокируемый пользователь")] DiscordMember member,
        [Description("За сколько дней удалить сообщения?")] int days,
        [RemainingText, Description("Причина")] string reason)
    {
        await ctx.TriggerTypingAsync();
        var guild = new DiscordGuild();
        guild = member.Guild;
        try
        {
            await guild.BanMemberAsync(member, days, reason);
            await ctx.RespondAsync(
                $"Пользователь @{member.Username}#{member.Discriminator} был исключён администратором {ctx.User.Username}");
        }
        catch (Exception)
        {
            await ctx.RespondAsync($"Пользователь {member.Username} не может быть заблокирован");
        }
    }
}
