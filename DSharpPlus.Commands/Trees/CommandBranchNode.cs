using System.Collections.Generic;

namespace DSharpPlus.Commands.Trees;

/// <summary>
/// Represents an itself not executable branch node.
/// </summary>
public sealed class CommandBranchNode : ICommandNode
{
    /// <inheritdoc/>
    public required string Name { get; init; }

    /// <inheritdoc/>
    public required IReadOnlyList<string> Aliases { get; init; }

    /// <inheritdoc/>
    public required string LowercasedName { get; init; }

    /// <inheritdoc/>
    public required string Description { get; init; }

    /// <inheritdoc/>
    public required ICommandNode? Parent { get; init; }

    /// <inheritdoc/>
    public required IReadOnlyList<ICommandNode> Children { get; init; }

    /// <inheritdoc/>
    public required NodeMetadataCollection Metadata { get; init; }
}
