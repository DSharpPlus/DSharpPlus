namespace DSharpPlus.Commands.ContextChecks;
using System.Threading.Tasks;

internal sealed class RequireGuildCheck : IContextCheck<RequireGuildAttribute>
{
    internal const string ErrorMessage = "This command must be executed in a guild.";

    public ValueTask<string?> ExecuteCheckAsync(RequireGuildAttribute attribute, CommandContext context)
        => ValueTask.FromResult(context.Guild is null ? ErrorMessage : null);
}
