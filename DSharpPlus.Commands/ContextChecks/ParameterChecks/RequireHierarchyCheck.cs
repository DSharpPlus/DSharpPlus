#pragma warning disable IDE0046 // no quintuple nested ternaries today

using System.Threading.Tasks;

using DSharpPlus.Entities;

namespace DSharpPlus.Commands.ContextChecks.ParameterChecks;

/// <summary>
/// Executes the checks for requiring a hierarchical order between the bot/executor and a parameter.
/// </summary>
public sealed class RequireHierarchyCheck : 
    IParameterCheck<RequireHigherBotHierarchyAttribute>,
    IParameterCheck<RequireHigherUserHierarchyAttribute>
{
    public ValueTask<string?> ExecuteCheckAsync
    (
        RequireHigherBotHierarchyAttribute attribute, 
        ParameterCheckInfo info, 
        CommandContext context
    )
    {
        // no value is always hierarchically lower than any user
        if (info.Value is null)
        {
            return ValueTask.FromResult<string?>(null);
        }

        // no guild means no hierarchy.
        if (context.Guild is null)
        {
            return ValueTask.FromResult<string?>(null);
        }

        if (info.Value is DiscordRole role)
        {
            return ValueTask.FromResult
            (
                context.Guild.CurrentMember.Hierarchy > role.Position
                    ? null
                    : "The provided role was higher than the highest role of the bot user."
            );
        }

        if (info.Value is DiscordMember member)
        {
            return ValueTask.FromResult
            (
                context.Guild.CurrentMember.Hierarchy > member.Hierarchy
                    ? null
                    : "The provided member's highest role was higher than the highest role of the bot user."
            );
        }

        return ValueTask.FromResult<string?>("The provided parameter was neither a role nor an user, failed to check hierarchy.");
    }

    public ValueTask<string?> ExecuteCheckAsync
    (
        RequireHigherUserHierarchyAttribute attribute, 
        ParameterCheckInfo info,
        CommandContext context
    )
    {
        // no value is always hierarchically lower than any user
        if (info.Value is null)
        {
            return ValueTask.FromResult<string?>(null);
        }

        // no guild means no hierarchy.
        if (context.Guild is null)
        {
            return ValueTask.FromResult<string?>(null);
        }

        if (info.Value is DiscordRole role)
        {
            return ValueTask.FromResult
            (
                // this should? never be null if we're in a guild
                context.Member!.Hierarchy > role.Position
                    ? null
                    : "The provided role was higher than the highest role of the executing user."
            );
        }

        if (info.Value is DiscordMember member)
        {
            return ValueTask.FromResult
            (
                context.Member!.Hierarchy > member.Hierarchy
                    ? null
                    : "The provided member's highest role was higher than the highest role of the executing user."
            );
        }

        return ValueTask.FromResult<string?>("The provided parameter was neither a role nor an user, failed to check hierarchy.");
    }
}
