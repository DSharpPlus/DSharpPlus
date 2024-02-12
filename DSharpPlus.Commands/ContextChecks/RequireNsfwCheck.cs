using System.Threading.Tasks;

namespace DSharpPlus.Commands.ContextChecks;

internal sealed class RequireNsfwCheck : IContextCheck<RequireNsfwAttribute>
{
    public ValueTask<string?> ExecuteCheckAsync(RequireNsfwAttribute attribute, CommandContext context)
    {
        return ValueTask.FromResult
        (
            context.Channel.IsPrivate || context.Channel.IsNSFW || (context.Guild is not null && context.Guild.IsNSFW)
                ? "This command must be executed in a NSFW channel."
                : null
        );
    }
}
