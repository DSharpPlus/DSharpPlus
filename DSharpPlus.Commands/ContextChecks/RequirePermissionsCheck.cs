#pragma warning disable IDE0046

using System.Threading.Tasks;

using DSharpPlus.Commands.Processors.SlashCommands;

namespace DSharpPlus.Commands.ContextChecks;

internal sealed class RequirePermissionsCheck : IContextCheck<RequirePermissionsAttribute>
{
    public ValueTask<string?> ExecuteCheckAsync(RequirePermissionsAttribute attribute, CommandContext context)
    {
        if (context is SlashCommandContext slashContext)
        {
            if (!slashContext.Interaction.AppPermissions.HasAllPermissions(attribute.BotPermissions))
            {
                return ValueTask.FromResult<string?>("The bot did not have the needed permissions to execute this command.");
            }

            return ValueTask.FromResult<string?>(null);
        }
        else if (context.Guild is null)
        {
            return ValueTask.FromResult<string?>(RequireGuildCheck.ErrorMessage);
        }
        else if (!context.Guild!.CurrentMember.PermissionsIn(context.Channel).HasAllPermissions(attribute.BotPermissions))
        {
            return ValueTask.FromResult<string?>("The bot did not have the needed permissions to execute this command.");
        }
        else if (!context.Member!.PermissionsIn(context.Channel).HasAllPermissions(attribute.UserPermissions))
        {
            return ValueTask.FromResult<string?>("The executing user did not have the needed permissions to execute this command.");
        }

        return ValueTask.FromResult<string?>(null);
    }
}
