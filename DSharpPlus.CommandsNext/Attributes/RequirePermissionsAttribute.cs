using System;
using System.Threading.Tasks;
using DSharpPlus.Entities;

namespace DSharpPlus.CommandsNext.Attributes;

/// <summary>
/// Defines that usage of this command is restricted to members with specified permissions. This check also verifies that the bot has the same permissions.
/// </summary>
[AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
public sealed class RequirePermissionsAttribute : CheckBaseAttribute
{
    /// <summary>
    /// Gets the permissions required by this attribute.
    /// </summary>
    public DiscordPermission[] Permissions { get; }

    /// <summary>
    /// Gets this check's behaviour in DMs. True means the check will always pass in DMs, whereas false means that it will always fail.
    /// </summary>
    public bool IgnoreDms { get; } = true;

    /// <summary>
    /// Defines that usage of this command is restricted to members with specified permissions. This check also verifies that the bot has the same permissions.
    /// </summary>
    /// <param name="permissions">Permissions required to execute this command.</param>
    /// <param name="ignoreDms">Sets this check's behaviour in DMs. True means the check will always pass in DMs, whereas false means that it will always fail.</param>
    public RequirePermissionsAttribute(bool ignoreDms = true, params DiscordPermission[] permissions)
    {
        this.Permissions = permissions;
        this.IgnoreDms = ignoreDms;
    }

    public override async Task<bool> ExecuteCheckAsync(CommandContext ctx, bool help)
    {
        if (ctx.Guild is null)
        {
            return this.IgnoreDms;
        }

        DiscordMember? user = ctx.Member;
        if (user is null)
        {
            return false;
        }

        DiscordPermissions userPermissions = ctx.Channel.PermissionsFor(user);

        DiscordMember bot = await ctx.Guild.GetMemberAsync(ctx.Client.CurrentUser.Id);
        if (bot is null)
        {
            return false;
        }

        DiscordPermissions botPermissions = ctx.Channel.PermissionsFor(bot);

        bool userIsOwner = ctx.Guild.OwnerId == user.Id;
        bool botIsOwner = ctx.Guild.OwnerId == bot.Id;

        if (!userIsOwner)
        {
            userIsOwner = userPermissions.HasAllPermissions([..this.Permissions]);
        }

        if (!botIsOwner)
        {
            botIsOwner = botPermissions.HasAllPermissions([..this.Permissions]);
        }

        return userIsOwner && botIsOwner;
    }
}
