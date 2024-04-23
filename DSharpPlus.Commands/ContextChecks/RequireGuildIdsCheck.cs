using System.Linq;
using System.Threading.Tasks;

namespace DSharpPlus.Commands.ContextChecks;

internal sealed class RequireGuildIdsCheck : IContextCheck<RequireGuildIdsAttribute>
{
    public ValueTask<string?> ExecuteCheckAsync(RequireGuildIdsAttribute attribute, CommandContext context)
    {
        string? result = context.Guild is null
            ? RequireGuildCheck.ErrorMessage
            : attribute.GuildIds.Contains(context.Guild.Id)
                ? null
                : "This command must be executed in a specific guild.";

        return ValueTask.FromResult(result);
    }
}
