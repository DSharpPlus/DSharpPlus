namespace DSharpPlus.Commands.ContextChecks;

using System;
using System.Threading.Tasks;
using DSharpPlus.Commands.Trees;

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

    public override async Task<bool> ExecuteCheckAsync(CommandContext context) => await base.ExecuteCheckAsync(context)
        && context.Guild!.CurrentMember.PermissionsIn(context.Channel).HasPermission(this.BotPermissions)
        && context.Member!.PermissionsIn(context.Channel).HasPermission(this.UserPermissions);
}
