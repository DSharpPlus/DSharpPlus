using System;

using DSharpPlus.Entities;

namespace DSharpPlus.Commands.Trees.Predicates;

/// <summary>
/// Represents a predicate checking against the executing user. This cannot be enregistered with Discord for application commands.
/// </summary>
public class UserPredicate : ICommandExecutionPredicate
{
    private readonly Func<DiscordUser, bool> condition;

    public UserPredicate(Func<DiscordUser, bool> condition) => this.condition = condition;

    /// <inheritdoc/>
    public bool IsFulfilled(AbstractContext context) => this.condition(context.User);
}
