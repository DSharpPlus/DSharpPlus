namespace DSharpPlus.CommandsNext.Attributes;

using System;
using System.Threading.Tasks;

using DSharpPlus.Entities;

/// <summary>
/// Defines that usage of this command is restricted to members with specified permissions. This check also verifies that the bot has the same permissions.
/// </summary>
[AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
public sealed class RequirePermissionsAttribute : CheckBaseAttribute
{
    /// <summary>
    /// Gets the permissions required by this attribute.
    /// </summary>
    public DiscordPermissions Permissions { get; }

    /// <summary>
    /// Gets this check's behaviour in DMs. True means the check will always pass in DMs, whereas false means that it will always fail.
    /// </summary>
    public bool IgnoreDms { get; } = true;

    /// <summary>
    /// Defines that usage of this command is restricted to members with specified permissions. This check also verifies that the bot has the same permissions.
    /// </summary>
    /// <param name="permissions">Permissions required to execute this command.</param>
    /// <param name="ignoreDms">Sets this check's behaviour in DMs. True means the check will always pass in DMs, whereas false means that it will always fail.</param>
    public RequirePermissionsAttribute(DiscordPermissions permissions, bool ignoreDms = true)
    {
        Permissions = permissions;
        IgnoreDms = ignoreDms;
    }

    public override async Task<bool> ExecuteCheckAsync(CommandContext ctx, bool help)
    {
        if (ctx.Guild == null)
        {
            return IgnoreDms;
        }

        DSharpPlus.Entities.DiscordMember? usr = ctx.Member;
        if (usr == null)
        {
            return false;
        }

        DiscordPermissions pusr = ctx.Channel.PermissionsFor(usr);

        DSharpPlus.Entities.DiscordMember bot = await ctx.Guild.GetMemberAsync(ctx.Client.CurrentUser.Id);
        if (bot == null)
        {
            return false;
        }

        DiscordPermissions pbot = ctx.Channel.PermissionsFor(bot);

        bool usrok = ctx.Guild.OwnerId == usr.Id;
        bool botok = ctx.Guild.OwnerId == bot.Id;

        if (!usrok)
        {
            usrok = (pusr & DiscordPermissions.Administrator) != 0 || (pusr & Permissions) == Permissions;
        }

        if (!botok)
        {
            botok = (pbot & DiscordPermissions.Administrator) != 0 || (pbot & Permissions) == Permissions;
        }

        return usrok && botok;
    }
}
