using System.Collections.Generic;

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
    public string LowercasedName { get; internal set; }

    /// <inheritdoc/>
    public string Description { get; internal set; }

    /// <inheritdoc/>
    public ICommandNode? Parent { get; internal set; }

    /// <inheritdoc/>
    public IReadOnlyList<ICommandNode> Children { get; internal set; }

    /// <inheritdoc/>
    public NodeMetadataCollection Metadata { get; }
}
