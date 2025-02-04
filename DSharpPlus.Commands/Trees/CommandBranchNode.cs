using System.Collections.Generic;

using DSharpPlus.Commands.Trees.Predicates;

namespace DSharpPlus.Commands.Trees;

/// <summary>
/// Represents an itself not executable branch node.
/// </summary>
public class CommandBranchNode : ICommandNode
{
    /// <inheritdoc/>
    public string Name { get; internal set; }

    /// <inheritdoc/>
    public IReadOnlyList<string> Aliases { get; internal set; }

    /// <inheritdoc/>
    public string Description { get; internal set; }

    /// <inheritdoc/>
    public ICommandNode? Parent { get; internal set; }

    /// <inheritdoc/>
    public IReadOnlyList<ICommandNode> Children { get; internal set; }

    /// <inheritdoc/>
    public IReadOnlyList<ICommandExecutionPredicate> Predicates { get; internal set; }

    /// <inheritdoc/>
    public IReadOnlyDictionary<string, object> CustomMetadata { get; }
}
