namespace DSharpPlus.Commands.ContextChecks;

using System;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method | AttributeTargets.Delegate)]
public class RequirePermissionsAttribute : RequireGuildAttribute
{
    public Permissions BotPermissions { get; init; }
    public Permissions UserPermissions { get; init; }

    public RequirePermissionsAttribute(Permissions permissions) => this.BotPermissions = this.UserPermissions = permissions;
    public RequirePermissionsAttribute(Permissions botPermissions, Permissions userPermissions)
    {
        this.BotPermissions = botPermissions;
        this.UserPermissions = userPermissions;
    }
}
