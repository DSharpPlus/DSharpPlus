using System;

using DSharpPlus.Entities;

namespace DSharpPlus.Commands.Trees.Predicates;

/// <summary>
/// Represents a predicate checking against the channel the command is being executed in. This cannot be enregistered with Discord for application commands.
/// </summary>
public class ChannelPredicate : ICommandExecutionPredicate
{
    private readonly Func<DiscordChannel, bool> condition;

    public ChannelPredicate(Func<DiscordChannel, bool> condition) => this.condition = condition;

    /// <inheritdoc/>
    public bool IsFulfilled(AbstractContext context) => this.condition(context.Channel);
}
