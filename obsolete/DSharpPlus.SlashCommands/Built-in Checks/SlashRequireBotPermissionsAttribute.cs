using System;
using System.Threading.Tasks;
using DSharpPlus.Entities;

namespace DSharpPlus.SlashCommands.Attributes;

/// <summary>
/// Defines that usage of this slash command is only possible when the bot is granted a specific permission.
/// </summary>
[AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
public sealed class SlashRequireBotPermissionsAttribute : SlashCheckBaseAttribute
{
    /// <summary>
    /// Gets the permissions required by this attribute.
    /// </summary>
    public DiscordPermission[] Permissions { get; }

    /// <summary>
    /// Gets or sets this check's behaviour in DMs. True means the check will always pass in DMs, whereas false means that it will always fail.
    /// </summary>
    public bool IgnoreDms { get; } = true;

    /// <summary>
    /// Defines that usage of this slash command is only possible when the bot is granted a specific permission.
    /// </summary>
    /// <param name="permissions">Permissions required to execute this command.</param>
    /// <param name="ignoreDms">Sets this check's behaviour in DMs. True means the check will always pass in DMs, whereas false means that it will always fail.</param>
    public SlashRequireBotPermissionsAttribute(bool ignoreDms = true, params DiscordPermission[] permissions)
    {
        this.Permissions = permissions;
        this.IgnoreDms = ignoreDms;
    }

    /// <summary>
    /// Runs checks.
    /// </summary>
    public override async Task<bool> ExecuteChecksAsync(InteractionContext ctx)
    {
        if (ctx.Guild == null)
        {
            return this.IgnoreDms;
        }

        Entities.DiscordMember bot = await ctx.Guild.GetMemberAsync(ctx.Client.CurrentUser.Id);
        if (bot == null)
        {
            return false;
        }

        if (bot.Id == ctx.Guild.OwnerId)
        {
            return true;
        }

        DiscordPermissions pbot = ctx.Channel.PermissionsFor(bot);

        return pbot.HasAllPermissions(this.Permissions);
    }
}
