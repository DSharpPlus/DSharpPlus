
using System;

using DSharpPlus.Entities;

namespace DSharpPlus.Commands.ContextChecks;
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method | AttributeTargets.Delegate)]
public class RequirePermissionsAttribute : RequireGuildAttribute
{
    public DiscordPermissions BotPermissions { get; init; }
    public DiscordPermissions UserPermissions { get; init; }

    public RequirePermissionsAttribute(DiscordPermissions permissions) => BotPermissions = UserPermissions = permissions;
    public RequirePermissionsAttribute(DiscordPermissions botPermissions, DiscordPermissions userPermissions)
    {
        BotPermissions = botPermissions;
        UserPermissions = userPermissions;
    }
}
