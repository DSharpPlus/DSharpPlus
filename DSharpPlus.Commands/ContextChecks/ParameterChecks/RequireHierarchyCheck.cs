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
    public ValueTask<string?> ExecuteCheckAsync(RequireHigherBotHierarchyAttribute attribute, ParameterCheckInfo info, CommandContext context)
    {
        return info.Value switch
        {
            _ when context.Guild is null => ValueTask.FromResult<string?>(null),
            null => ValueTask.FromResult<string?>(null),
            DiscordRole role when context.Guild.CurrentMember.Hierarchy > role.Position => ValueTask.FromResult<string?>(null),
            DiscordRole => ValueTask.FromResult<string?>("The provided role was higher than the highest role of the bot user."),
            DiscordMember member when context.Guild.CurrentMember.Hierarchy > member.Hierarchy => ValueTask.FromResult<string?>(null),
            DiscordMember => ValueTask.FromResult<string?>("The provided member's highest role was higher than the highest role of the bot user."),
            _ => ValueTask.FromResult<string?>("The provided parameter was neither a role nor an user, failed to check hierarchy.")
        };
    }

    public ValueTask<string?> ExecuteCheckAsync(RequireHigherUserHierarchyAttribute attribute, ParameterCheckInfo info, CommandContext context)
    {
        return info.Value switch
        {
            _ when context.Guild is null => ValueTask.FromResult<string?>(null),
            DiscordRole role when context.Member!.Hierarchy > role.Position => ValueTask.FromResult<string?>(null),
            DiscordRole => ValueTask.FromResult<string?>("The provided role was higher than the highest role of the executing user."),
            DiscordMember member when context.Member!.Hierarchy > member.Hierarchy => ValueTask.FromResult<string?>(null),
            DiscordMember => ValueTask.FromResult<string?>("The provided member's highest role was higher than the highest role of the executing user."),
            _ => ValueTask.FromResult<string?>("The provided parameter was neither a role nor an user, failed to check hierarchy.")
        };
    }
}
