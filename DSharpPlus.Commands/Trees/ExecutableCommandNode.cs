using System.Collections.Generic;

using DSharpPlus.Commands.Trees.Predicates;

namespace DSharpPlus.Commands.Trees;

/// <summary>
/// Represents a command node that may be executed. <see cref="ICommandNode.Children"/> will refer to a sum of this node's child nodes and overloads.
/// </summary>
public class ExecutableCommandNode : ICommandNode
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
    public ICommandNode? Parent { get; internal set; }

    /// <inheritdoc/>
    IReadOnlyList<ICommandNode> ICommandNode.Children => [.. this.Children, .. this.Overloads];

    /// <inheritdoc/>
    public IReadOnlyList<ICommandExecutionPredicate> Predicates { get; internal set; }

    /// <summary>
    /// Child branches and themselves-executable nodes of this command. This is unavailable for application commands.
    /// </summary>
    public IReadOnlyList<ICommandNode> Children { get; internal set; }

    /// <summary>
    /// Overloads of this command. Overloads may be distinguished by predicates or parameter list.
    /// </summary>
    public IReadOnlyList<CommandOverload> Overloads { get; internal set; }

    /// <summary>
    /// If there is a valid overload to use as an application command, this will be it. Null if this node is too deep into a tree
    /// or if no overload could be unambiguously chosen for an application command.
    /// </summary>
    /// <remarks>
    /// This property's absence only indicates that the <i>current node</i> is not reachable as an application command. Child nodes
    /// may, in fact, be reachable as application commands, and the tree must be walked up to the maximum permissible depth to
    /// conclusively determine whether an executable node can be executed as an application command.
    /// </remarks>
    public CommandOverload? CanonicalApplicationCommandOverload { get; internal set; }

    /// <inheritdoc/>
    public IReadOnlyDictionary<string, object> CustomMetadata { get; }
}
