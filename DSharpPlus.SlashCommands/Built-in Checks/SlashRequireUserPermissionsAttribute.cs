
using System;
using System.Threading.Tasks;

using DSharpPlus.Entities;

namespace DSharpPlus.SlashCommands.Attributes;
/// <summary>
/// Defines that usage of this command is restricted to members with specified permissions.
/// </summary>
[AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
public sealed class SlashRequireUserPermissionsAttribute : SlashCheckBaseAttribute
{
    /// <summary>
    /// Gets the permissions required by this attribute.
    /// </summary>
    public DiscordPermissions Permissions { get; }

    /// <summary>
    /// Gets or sets this check's behaviour in DMs. True means the check will always pass in DMs, whereas false means that it will always fail.
    /// </summary>
    public bool IgnoreDms { get; } = true;

    /// <summary>
    /// Defines that usage of this command is restricted to members with specified permissions.
    /// </summary>
    /// <param name="permissions">Permissions required to execute this command.</param>
    /// <param name="ignoreDms">Sets this check's behaviour in DMs. True means the check will always pass in DMs, whereas false means that it will always fail.</param>
    public SlashRequireUserPermissionsAttribute(DiscordPermissions permissions, bool ignoreDms = true)
    {
        Permissions = permissions;
        IgnoreDms = ignoreDms;
    }

    /// <summary>
    /// Runs checks.
    /// </summary>
    public override Task<bool> ExecuteChecksAsync(InteractionContext ctx)
    {
        if (ctx.Guild == null)
        {
            return Task.FromResult(IgnoreDms);
        }

        Entities.DiscordMember usr = ctx.Member;
        if (usr == null)
        {
            return Task.FromResult(false);
        }

        if (usr.Id == ctx.Guild.OwnerId)
        {
            return Task.FromResult(true);
        }

        DiscordPermissions pusr = ctx.Channel.PermissionsFor(usr);

        return (pusr & DiscordPermissions.Administrator) != 0
            ? Task.FromResult(true)
            : (pusr & Permissions) == Permissions ? Task.FromResult(true) : Task.FromResult(false);
    }
}
