using System;
using DSharpPlus.Entities;

namespace DSharpPlus.Commands.ContextChecks;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method | AttributeTargets.Delegate)]
public class RequirePermissionsAttribute : RequireGuildAttribute
{
    public DiscordPermission[] BotPermissions { get; init; }
    public DiscordPermission[] UserPermissions { get; init; }

    public RequirePermissionsAttribute(params DiscordPermission[] permissions) => this.BotPermissions = this.UserPermissions = permissions;
    public RequirePermissionsAttribute(DiscordPermission[] botPermissions, DiscordPermission[] userPermissions)
    {
        this.BotPermissions = botPermissions;
        this.UserPermissions = userPermissions;
    }
}
