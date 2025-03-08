using System;
using System.Collections.Generic;
using DSharpPlus.Entities;

namespace DSharpPlus.Commands.ContextChecks;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method | AttributeTargets.Delegate)]
public class RequirePermissionsAttribute : RequireGuildAttribute
{
    public DiscordPermissions BotPermissions { get; init; }
    public DiscordPermissions UserPermissions { get; init; }

    public RequirePermissionsAttribute(params DiscordPermission[] permissions) => this.BotPermissions = this.UserPermissions = new((IReadOnlyList<DiscordPermission>)permissions);
    public RequirePermissionsAttribute(DiscordPermission[] botPermissions, DiscordPermission[] userPermissions)
    {
        this.BotPermissions = new((IReadOnlyList<DiscordPermission>)botPermissions);
        this.UserPermissions = new((IReadOnlyList<DiscordPermission>)userPermissions);
    }
}
