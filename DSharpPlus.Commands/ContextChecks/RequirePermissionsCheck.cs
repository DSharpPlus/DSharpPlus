#pragma warning disable IDE0046

using DSharpPlus.Entities;
using System.Threading.Tasks;

namespace DSharpPlus.Commands.ContextChecks;

internal sealed class RequirePermissionsCheck : IContextCheck<RequirePermissionsAttribute>
{
    public ValueTask<string?> ExecuteCheckAsync(RequirePermissionsAttribute attribute, CommandContext context)
    {
        if (context.Guild is null)
        {
            return ValueTask.FromResult<string?>(RequireGuildCheck.ErrorMessage);
        }

        if (!context.Guild!.CurrentMember.PermissionsIn(context.Channel).HasPermission(attribute.BotPermissions))
        {
            return ValueTask.FromResult<string?>("The bot did not have the needed permissions to execute this command.");
        }

        if (!context.Member!.PermissionsIn(context.Channel).HasPermission(attribute.UserPermissions))
        {
            return ValueTask.FromResult<string?>("The executing user did not have the needed permissions to execute this command.");
        }

        return ValueTask.FromResult<string?>(null);
    }
}
