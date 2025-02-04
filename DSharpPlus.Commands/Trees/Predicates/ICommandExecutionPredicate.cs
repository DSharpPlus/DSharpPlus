namespace DSharpPlus.Commands.Trees.Predicates;

/// <summary>
/// Represents a single, trivial, reusable condition for executing a command.
/// </summary>
public interface ICommandExecutionPredicate
{
    /// <summary>
    /// Checks whether the condition is fulfilled.
    /// </summary>
    /// <remarks>
    /// Implementers should ensure only absolutely necessary data is checked. Furthermore, predicates are performance-critical and must be
    /// as fast as possible; attempting to call async code or really doing anything but testing a trivial condition is incorrect usage of
    /// this API and should use context checks instead.
    /// </remarks>
    /// <param name="context">The in-flight context of the command being searched.</param>
    public bool IsFulfilled(AbstractContext context);
}
