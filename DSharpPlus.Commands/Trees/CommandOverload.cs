using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using DSharpPlus.Commands.ContextChecks;
using DSharpPlus.Commands.Trees.Predicates;

namespace DSharpPlus.Commands.Trees;

/// <summary>
/// Represents an overload of a command. Overloads may be distinguished by predicates or parameter list. No two overloads of the same
/// executable node may match in both predicates and parameter list.
/// </summary>
public class CommandOverload : ICommandNode
{
    /// <inheritdoc/>
    public string Name { get; internal set; }

    /// <inheritdoc/>
    public IReadOnlyList<string> Aliases { get; internal set; }

    /// <inheritdoc/>
    public string LowercasedName { get; internal set; }

    /// <inheritdoc/>
    public string Description { get; internal set; }

    /// <inheritdoc/>
    /// <remarks>
    /// This is guaranteed to be an <see cref="ExecutableCommandNode"/>.
    /// </remarks>
    public ICommandNode? Parent { get; internal set; }

    /// <inheritdoc/>
    IReadOnlyList<ICommandNode> ICommandNode.Children => [];

    /// <inheritdoc/>
    public IReadOnlyList<ICommandExecutionPredicate> Predicates { get; internal set; }

    /// <summary>
    /// The parameters of this command, excluding the command context.
    /// </summary>
    public IReadOnlyList<CommandParameter> Parameters { get; internal set; }

    /// <summary>
    /// The executing function for this command.
    /// </summary>
    public Func<CommandContext, object?[], IServiceProvider, ValueTask> Execute { get; internal set; }

    /// <summary>
    /// The check attributes applicable to this command. This does not necessarily correlate to the list of executed checks,
    /// which may vary based on context and registered check implementations.
    /// </summary>
    public IReadOnlyList<ContextCheckAttribute> CheckAttributes { get; internal set; }

    /// <inheritdoc/>
    public IReadOnlyDictionary<string, object> CustomMetadata { get; }
}
