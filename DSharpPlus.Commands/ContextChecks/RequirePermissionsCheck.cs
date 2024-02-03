#pragma warning disable IDE0046

using System.Threading.Tasks;

using DSharpPlus.Commands.Trees;

namespace DSharpPlus.Commands.ContextChecks;

internal sealed class RequirePermissionsCheck(RequireGuildCheck guildCheck) : IContextCheck<RequirePermissionsAttribute>
{
    public async ValueTask<string?> ExecuteCheckAsync(RequirePermissionsAttribute attribute, CommandContext context)
    {
        if (await guildCheck.ExecuteCheckAsync(null!, context) is not null)
        {
            return "This command must be executed within a guild.";
        }

        if (!context.Guild!.CurrentMember.PermissionsIn(context.Channel).HasPermission(attribute.BotPermissions))
        {
            return "The bot did not have the needed permissions to execute this command.";
        }

        if (!context.Member!.PermissionsIn(context.Channel).HasPermission(attribute.UserPermissions))
        {
            return "The executing user did not have the needed permissions to execute this command.";
        }

        return null;
    }
}
