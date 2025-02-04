namespace DSharpPlus.Commands.Trees.Predicates;

/// <summary>
/// Checks whether the site of this command is a NSFW channel. This will be enregistered with discord for application commands.
/// </summary>
public class InNsfwChannelPredicate : ICommandExecutionPredicate
{
    /// <inheritdoc/>
    public bool IsFulfilled(AbstractContext context) => context.Channel.IsPrivate || context.Channel.IsNSFW;
}
