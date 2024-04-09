using DSharpPlus.Entities;
using System;
using System.Threading.Tasks;

namespace DSharpPlus.CommandsNext.Attributes;

/// <summary>
/// Defines that usage of this command is restricted to members with specified permissions.
/// </summary>
[AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
public sealed class RequireUserPermissionsAttribute : CheckBaseAttribute
{
    /// <summary>
    /// Gets the permissions required by this attribute.
    /// </summary>
    public Permissions Permissions { get; }

    /// <summary>
    /// Gets this check's behaviour in DMs. True means the check will always pass in DMs, whereas false means that it will always fail.
    /// </summary>
    public bool IgnoreDms { get; } = true;

    /// <summary>
    /// Defines that usage of this command is restricted to members with specified permissions.
    /// </summary>
    /// <param name="permissions">Permissions required to execute this command.</param>
    /// <param name="ignoreDms">Sets this check's behaviour in DMs. True means the check will always pass in DMs, whereas false means that it will always fail.</param>
    public RequireUserPermissionsAttribute(Permissions permissions, bool ignoreDms = true)
    {
        this.Permissions = permissions;
        this.IgnoreDms = ignoreDms;
    }

    public override Task<bool> ExecuteCheckAsync(CommandContext ctx, bool help)
    {
        if (ctx.Guild == null)
        {
            return Task.FromResult(this.IgnoreDms);
        }

        DSharpPlus.Entities.DiscordMember? usr = ctx.Member;
        if (usr == null)
        {
            return Task.FromResult(false);
        }

        if (usr.Id == ctx.Guild.OwnerId)
        {
            return Task.FromResult(true);
        }

        Permissions pusr = ctx.Channel.PermissionsFor(usr);

        return (pusr & Permissions.Administrator) != 0
            ? Task.FromResult(true)
            : (pusr & this.Permissions) == this.Permissions ? Task.FromResult(true) : Task.FromResult(false);
    }
}
