namespace DSharpPlus.Commands.ContextChecks;
using System.Diagnostics;
using System.Threading.Tasks;

internal sealed class DirectMessageUsageCheck : IContextCheck<DirectMessageUsageAttribute>
{
    public ValueTask<string?> ExecuteCheckAsync(DirectMessageUsageAttribute attribute, CommandContext context)
    {
        if (context.Channel.IsPrivate && attribute.Usage is not DirectMessageUsage.DenyDMs)
        {
            return ValueTask.FromResult<string?>(null);
        }
        else if (!context.Channel.IsPrivate && attribute.Usage is not DirectMessageUsage.RequireDMs)
        {
            return ValueTask.FromResult<string?>(null);
        }
        else
        {
            string dmStatus = context.Channel.IsPrivate
                ? "inside a DM"
                : "outside a DM";

            string requirement = attribute.Usage switch
            {
                DirectMessageUsage.DenyDMs => "denies DM usage",
                DirectMessageUsage.RequireDMs => "requires DM usage",
                _ => throw new UnreachableException
                (
                    $"DirectMessageUsageCheck reached an unreachable branch: {attribute.Usage}, IsPrivate was {context.Channel.IsPrivate}"
                )
            };

            return ValueTask.FromResult<string?>($"The executed command {requirement} but was executed {dmStatus}.");
        }
    }
}
