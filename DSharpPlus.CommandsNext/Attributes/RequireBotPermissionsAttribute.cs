namespace DSharpPlus.CommandsNext.Attributes;
using System;
using System.Threading.Tasks;

using DSharpPlus.Entities;

/// <summary>
/// Defines that usage of this command is only possible when the bot is granted a specific permission.
/// </summary>
[AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
public sealed class RequireBotPermissionsAttribute : CheckBaseAttribute
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
    /// Defines that usage of this command is only possible when the bot is granted a specific permission.
    /// </summary>
    /// <param name="permissions">Permissions required to execute this command.</param>
    /// <param name="ignoreDms">Sets this check's behaviour in DMs. True means the check will always pass in DMs, whereas false means that it will always fail.</param>
    public RequireBotPermissionsAttribute(DiscordPermissions permissions, bool ignoreDms = true)
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

        DSharpPlus.Entities.DiscordMember bot = await ctx.Guild.GetMemberAsync(ctx.Client.CurrentUser.Id);
        if (bot == null)
        {
            return false;
        }

        if (bot.Id == ctx.Guild.OwnerId)
        {
            return true;
        }

        DiscordPermissions pbot = ctx.Channel.PermissionsFor(bot);

        return (pbot & DiscordPermissions.Administrator) != 0 || (pbot & Permissions) == Permissions;
    }
}
