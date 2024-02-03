using System.Threading.Tasks;

using DSharpPlus.Commands.Trees;

namespace DSharpPlus.Commands.ContextChecks;

internal sealed class RequireGuildCheck : IContextCheck<RequireGuildAttribute>
{
    public ValueTask<string?> ExecuteCheckAsync(RequireGuildAttribute attribute, CommandContext context)
        => ValueTask.FromResult(context.Guild is null ? "This command must be executed in a guild." : null);
}
